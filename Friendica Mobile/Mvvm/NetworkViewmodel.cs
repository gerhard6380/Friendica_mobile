using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using Friendica_Mobile.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace Friendica_Mobile.Mvvm
{
    class NetworkViewmodel : BindableClass
    {
        //public FriendicaNewPost newPost;

        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public Thickness BottomAppBarMargin { get { return App.Settings.BottomAppBarMargin; } }

        public double ListViewWidth { get { return App.Settings.ShellWidth; } }

        private bool _showDragArea;
        public bool ShowDragArea
        {
            get { return _showDragArea; }
            set { _showDragArea = value;
                OnPropertyChanged("ShowDragArea"); }
        }

        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
        }

        // containing sample data which will be shown in chronological order
        private ObservableCollection<FriendicaPostExtended> _networkPostsSample;
        public ObservableCollection<FriendicaPostExtended> NetworkPostsSample
        {
            get { return _networkPostsSample; }
            set
            {
                _networkPostsSample = value;
                OnPropertyChanged("NetworkPostsSample");
            }
        }

        // containing sample data which will be shown in thread mode
        private ObservableCollection<FriendicaThread> _networkThreadsSample;
        public ObservableCollection<FriendicaThread> NetworkThreadsSample
        {
            get { return _networkThreadsSample; }
            set
            {
                _networkThreadsSample = value;
                OnPropertyChanged("NetworkThreadsSample");
            }
        }

        // containing data which will be shown in chronological order
        private ObservableCollection<FriendicaPostExtended> _networkPosts;
        public ObservableCollection<FriendicaPostExtended> NetworkPosts
        {
            get { return _networkPosts; }
            set { _networkPosts = value;
                OnPropertyChanged("NetworkPosts");
            }
        }

        // containing data which will be shown in conversation mode
        private ObservableCollection<FriendicaThread> _networkThreads;
        public ObservableCollection<FriendicaThread> NetworkThreads
        {
            get { return _networkThreads; }
            set { _networkThreads = value;
                OnPropertyChanged("NetworkThreads"); }
        }

        // toggle to decide which mode is displayed (chronological or conversational)
        private bool _isVisibleChronological;
        public bool IsVisibleChronological
        {
            get { return _isVisibleChronological; }
            set { _isVisibleChronological = value;
                OnPropertyChanged("IsVisibleChronological"); }
        }

        private bool _isLoadingInitial;
        public bool IsLoadingInitial
        {
            get { return _isLoadingInitial; }
            set { _isLoadingInitial = value;
                RefreshNetworkCommand.RaiseCanExecuteChanged();
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingInitial"); }
        }


        // set Property if refresh is currently in progress (showing progress bar and preventing from clicking Refresh again)
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { _isRefreshing = value;
                RefreshNetworkCommand.RaiseCanExecuteChanged();
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsRefreshing"); }
        }

        // set Property if loading next entries is currently in progress (showing progress bar and preventing from next loading to start)
        private bool _isLoadingNextEntries;
        public bool IsLoadingNextEntries
        {
            get { return _isLoadingNextEntries; }
            set
            {
                _isLoadingNextEntries = value;
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingNextEntries");
            }
        }

        private bool _isLoadedContacts;
        public bool IsLoadedContacts
        {
            get { return _isLoadedContacts; }
            set { _isLoadedContacts = value;
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadedContacts"); }
        }

        private bool _isSendingNewPost;
        public bool IsSendingNewPost
        {
            get { return _isSendingNewPost; }
            set
            {
                _isSendingNewPost = value;
                RefreshNetworkCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsSendingNewPost");
            }
        }


        private bool _noDataAvailable;
        public bool NoDataAvailable
        {
            get { return _noDataAvailable; }
            set { _noDataAvailable = value;
                OnPropertyChanged("NoDataAvailable"); }
        }

        private bool _showScrollToTop;
        public bool ShowScrollToTop
        {
            get { return _showScrollToTop; }
            set { _showScrollToTop = value;
                OnPropertyChanged("ShowScrollToTop"); }
        }

        private Brush _scrollToTopColor;
        public Brush ScrollToTopColor
        {
            get { return _scrollToTopColor; }
            set { _scrollToTopColor = value;
                OnPropertyChanged("ScrollToTopColor"); }
        }

        public event EventHandler ButtonShowThreadClicked;
        public event EventHandler ButtonAddCommentClicked;
        public event EventHandler ButtonShowProfileClicked;

        private FriendicaPostExtended _postToShowThread;
        public FriendicaPostExtended PostToShowThread
        {
            get { return _postToShowThread; }
            set { _postToShowThread = value; }
        }

        // refresh button
        Mvvm.Command _refreshNetworkCommand;
        public Mvvm.Command RefreshNetworkCommand { get { return _refreshNetworkCommand ?? (_refreshNetworkCommand = new Mvvm.Command(ExecuteRefresh, CanRefresh)); } }
        private bool CanRefresh()
        {
            if (_isLoadingInitial || _isRefreshing || IsSendingNewPost)
                return false;
            else
                return true;
        }

        private async void ExecuteRefresh()
        {
            IsRefreshing = true;

            // if we are displaying sample data we cannot do a correct refresh, so we need to wait some seconds before removing "loading" flag
            if (NoSettings)
            {
                await Task.Delay(3000);
                IsRefreshing = false;                
            }
            else
            {
                // remove borders indicating new entries and set counter to zero
                SetNewEntriesToSeen();

                // Load data newer than current max from server
                if (App.Settings.FriendicaServer != "" && App.Settings.FriendicaServer != "http://"
                    && App.Settings.FriendicaServer != "https://" && App.Settings.FriendicaServer != null)
                {
                    LoadNewEntries();
                }
            }
        }


        // add button - needed to react with enabling/disabling the button (command in ExecuteAddNewEntry() is not needed as click event handler
        // in Network.xaml.cs used because of access to shell for navigation needed
        Mvvm.Command _addNewEntryCommand;
        public Mvvm.Command AddNewEntryCommand { get { return _addNewEntryCommand ?? (_addNewEntryCommand = new Mvvm.Command(ExecuteAddNewEntry, CanAddNewEntry)); } }
        private bool CanAddNewEntry()
        {
            if (IsLoadingInitial || IsRefreshing || IsLoadingNextEntries)
                return false;
            else if (App.IsLoadedContacts)
                return true;
            else
                return false;
            
        }

        private void ExecuteAddNewEntry()
        {
        }


        // Chronological button - User can switch to chronological order of entries
        Mvvm.Command _chronologicalModeCommand;
        public Mvvm.Command ChronologicalModeCommand { get { return _chronologicalModeCommand ?? (_chronologicalModeCommand = new Mvvm.Command(ExecuteChronologicalMode, CanChronologicalMode)); } }
        private bool CanChronologicalMode()
        {
            if (IsVisibleChronological)
                return false;
            else
                return true;
        }

        private void ExecuteChronologicalMode()
        {
            IsVisibleChronological = true;
            App.IsVisibleChronological = true;
            OnPropertyChanged("IsVisibleChronological");
            ThreadsModeCommand.RaiseCanExecuteChanged();
            ChronologicalModeCommand.RaiseCanExecuteChanged();
        }

        // Threads button - User can switch to threads order of entries
        Mvvm.Command _threadsModeCommand;
        public Mvvm.Command ThreadsModeCommand { get { return _threadsModeCommand ?? (_threadsModeCommand = new Mvvm.Command(ExecuteThreadsMode, CanThreadsMode)); } }
        private bool CanThreadsMode()
        {
            if (IsVisibleChronological)
                return true;
            else
                return false;
        }

        private void ExecuteThreadsMode()
        {
            IsVisibleChronological = false;
            App.IsVisibleChronological = false;
            OnPropertyChanged("IsVisibleChronological");
            ThreadsModeCommand.RaiseCanExecuteChanged();
            ChronologicalModeCommand.RaiseCanExecuteChanged();
        }


        public async void SetNewEntriesToSeen()
        {
            if (NetworkPosts != null)
            {
                // set all border back to zero
                foreach (var post in NetworkPosts)
                {
                    if (post.NewEntryIndicatorBorder.Bottom == 4)
                        post.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                }
            }

            if (NetworkPostsSample != null)
            {
                foreach (var post in NetworkPostsSample)
                {
                    if (post.NewEntryIndicatorBorder.Bottom == 4)
                        post.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                }
            }

            if (NetworkThreadsSample != null)
            {
                foreach (var thread in NetworkThreadsSample)
                {
                    foreach (var post in thread.Posts)
                    {
                        if (post.NewEntryIndicatorBorder.Bottom == 4)
                            post.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                    }
                }
            }

            // set counter in menu to zero and color of button to white
            App.TileCounter.CounterUnseenNetwork = 0;
            ScrollToTopColor = new SolidColorBrush(Colors.White);

            // set badge counter to zero
            await App.Badge.ClearBadgeNumber();
        }

        public void SaveToApp()
        {
            App.NetworkPosts = (NetworkPosts.Count != 0) ? NetworkPosts : NetworkPostsSample;
            App.NetworkThreads = (NetworkThreads.Count != 0) ? NetworkThreads : NetworkThreadsSample;
            foreach (var thread in App.NetworkThreads)
            {
                thread.ButtonAddCommentClicked -= Thread_ButtonAddCommentClicked;
                thread.ButtonShowProfileClicked -= Thread_ButtonShowProfileClicked;
            }
            App.IsVisibleChronological = IsVisibleChronological;
        }

        public void ReloadFromApp()
        {
            ThreadsModeCommand.RaiseCanExecuteChanged();
            ChronologicalModeCommand.RaiseCanExecuteChanged();

            foreach (var post in App.NetworkPosts)
            {
                // recreate transformed post entries
                post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                post.ButtonAddCommentClicked += Result_ButtonAddCommentClicked;
                post.ButtonShowThreadClicked += Result_ButtonShowThreadClicked;
                post.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                _networkPosts.Add(post);
            }
            foreach (var thread in App.NetworkThreads)
            {
                _networkThreads.Add(thread);
                var posts = new ObservableCollection<FriendicaPostExtended>();

                // crashes if null
                if (thread.Posts == null)
                {
                    thread.PostsLoaded += Thread_PostsLoaded;
                    thread.LoadThread();
                }
                else
                {
                    foreach (var post in thread.Posts)
                    {
                        post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                        post.ButtonAddCommentClicked += Result_ButtonAddCommentClicked;
                        post.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                        posts.Add(post);
                    }
                }
                thread.Posts = posts;
            }

            // load next entries only if we are not in test mode
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
            || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
                IsRefreshing = false;
            else
                LoadNewEntries();
        }

        public void ReloadFromAppSamples()
        {
            _noSettings = true;
            OnPropertyChanged("NoSettings");

            ThreadsModeCommand.RaiseCanExecuteChanged();
            ChronologicalModeCommand.RaiseCanExecuteChanged();

            foreach (var post in App.NetworkPosts)
            {
                // recreate transformed post entries
                post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                post.ButtonAddCommentClicked += Result_ButtonAddCommentClicked;
                post.ButtonShowThreadClicked += Result_ButtonShowThreadClicked;
                post.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                _networkPostsSample.Add(post);
            }
            foreach (var thread in App.NetworkThreads)
            {
                _networkThreadsSample.Add(thread);
                var posts = new ObservableCollection<FriendicaPostExtended>();
                foreach (var post in thread.Posts)
                {
                    post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                    posts.Add(post);
                }
                thread.ButtonAddCommentClicked += Thread_ButtonAddCommentClicked;
                thread.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                thread.Posts = posts;
                thread.ReduceComments();
            }
            IsRefreshing = false;
        }



        public NetworkViewmodel()
        {
            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;

            IsSendingNewPost = App.IsSendingNewPost;
            App.ContactsLoaded += App_ContactsLoaded;
            App.SendingNewPostChanged += App_SendingNewPostChanged;
            ScrollToTopColor = new SolidColorBrush(Colors.White);

            // load user setting for display mode from AppSettings
            if (App.Settings.NetworkMode == "Chronological" && App.IsVisibleChronological)
                IsVisibleChronological = true;
            else
                IsVisibleChronological = App.IsVisibleChronological;
            
            // initialize NetworkThreads
            NetworkThreads = new ObservableCollection<FriendicaThread>();
            NetworkThreadsSample = new ObservableCollection<FriendicaThread>();
            NetworkPostsSample = new ObservableCollection<FriendicaPostExtended>();

            SetShowDragArea();
        }

        private void App_SendingNewPostChanged(object sender, EventArgs e)
        {
            IsSendingNewPost = App.IsSendingNewPost;
            RefreshNetworkCommand.RaiseCanExecuteChanged();
            ExecuteRefresh();
        }

        private void App_ContactsLoaded(object sender, EventArgs e)
        {
            IsLoadedContacts = App.IsLoadedContacts;
            AddNewEntryCommand.RaiseCanExecuteChanged();
        }


        /// <summary>
        /// 
        /// </summary>
        public void LoadInitial()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                _noSettings = true;
                OnPropertyChanged("NoSettings");

                // prepare sample data source
                var sampleObscoll = new ObservableCollection<FriendicaPostExtended>();

                string sample = "{\"text\":\"* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\nLorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\\n\\n* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\n* Lorem ipsum dolor\\n\\nhttps://www.youtube.com/watch?v=OACu7zCagi8\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:54 +0000 2015\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"Friendica\",\"id\":4,\"id_str\":\"4\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":{\"type\":\"Point\",\"coordinates\":[40.7127837,-74.0059413]},\"location\":\"New York\",\"favorited\":false,\"user\":{\"id\":1,\"id_str\":\"1\",\"name\":\"Testuser #1\",\"screen_name\":\"test1\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":2,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":1,\"network\":\"dfrn\"},\"statusnet_html\":\"<ul class=\\\"listbullet\\\" style=\\\"list-style-type: circle;\\\"><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li></ul><br><br>Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.<br><br><ul class=\\\"listdecimal\\\" style=\\\"list-style-type: decimal;\\\"><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li><li>Lorem ipsum dolor</li></ul><br><br><a href=\\\"https://www.youtube.com/watch?v=OACu7zCagi8\\\" target=\\\"_blank\\\">https://www.youtube.com/watch?v=OACu7zCagi8</a>\",\"statusnet_conversation_id\":\"4\"}";
                var postExtended1 = new FriendicaPostExtended(sample);
                var postExtended1Thread = new FriendicaPostExtended(sample);
                sampleObscoll.Add(postExtended1);
                var userPetyr = new FriendicaUserExtended(9, "Petyr Baelish", "https://friendica.server.text/profile/petyr", "petyr", "https://upload.wikimedia.org/wikipedia/en/5/5e/Aidan_Gillen_as_Petyr_Baelish.jpg", ContactTypes.Friends);
                AddSampleUsers(postExtended1, postExtended1Thread, null, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended1, postExtended1Thread, userPetyr, PostFriendicaActivities.FriendicaActivity.dislike);

                sample = "{\"text\":\"#include <iostream>\\nusing namespace std;\\nvoid main()\\n{\\ncout << \\\"Hello World!\\\" << endl;\\ncout << \\\"Welcome to C++ Programming\\\" << endl;\\n}\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:40 +0000 2015\",\"in_reply_to_status_id\":1,\"in_reply_to_status_id_str\":\"1\",\"source\":\"Friendica\",\"id\":3,\"id_str\":\"3\",\"in_reply_to_user_id\":1,\"in_reply_to_user_id_str\":\"1\",\"in_reply_to_screen_name\":\"test1\",\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":3,\"id_str\":\"3\",\"name\":\"Testuser #3\",\"screen_name\":\"test3\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":1,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":3,\"network\":\"dfrn\"},\"statusnet_html\":\"<code>#include &lt;iostream&gt;<br>using namespace std;<br>void main()<br>{<br>     cout &lt;&lt; \\\"Hello World!\\\" &lt;&lt; endl;   <br>     cout &lt;&lt; \\\"Welcome to C++ Programming\\\" &lt;&lt; endl; <br>}</code>\",\"statusnet_conversation_id\":\"1\"}";
                var postExtended2 = new FriendicaPostExtended(sample);
                var postExtended2Thread = new FriendicaPostExtended(sample);
                sampleObscoll.Add(postExtended2);
                AddSampleUsers(postExtended2, postExtended2Thread, null, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended2, postExtended2Thread, null, PostFriendicaActivities.FriendicaActivity.dislike);

                sample = "{\"text\":\"Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. :coffee\\n\\nMax Mustermann wrote:\\n> Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. :-)\\n\\nhttps://de.wikipedia.org/wiki/Friendica\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:23 +0000 2015\",\"in_reply_to_status_id\":1,\"in_reply_to_status_id_str\":\"1\",\"source\":\"Friendica\",\"id\":2,\"id_str\":\"2\",\"in_reply_to_user_id\":1,\"in_reply_to_user_id_str\":\"1\",\"in_reply_to_screen_name\":\"test1\",\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":2,\"id_str\":\"2\",\"name\":\"Testuser #2\",\"screen_name\":\"test2\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":1,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":2,\"network\":\"dfrn\"},\"statusnet_html\":\"Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. :coffee<br><br><br><strong class=\\\"author\\\">Max Mustermann wrote:</strong><blockquote>Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. :-)</blockquote><br><br><a href=\\\"https://de.wikipedia.org/wiki/Friendica\\\" target=\\\"_blank\\\">https://de.wikipedia.org/wiki/Friendica</a>\",\"statusnet_conversation_id\":\"1\"}";
                var postExtended3 = new FriendicaPostExtended(sample);
                var postExtended3Thread = new FriendicaPostExtended(sample);
                postExtended3.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                postExtended3Thread.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                sampleObscoll.Add(postExtended3);
                var userJaime = new FriendicaUserExtended(8, "Jaime Lannister", "https://friendica.server.text/profile/jaimeL", "jaimeL", "https://upload.wikimedia.org/wikipedia/en/b/b5/JaimeLannister.jpg", ContactTypes.Friends);
                var userTyrion = new FriendicaUserExtended(1, "Tyrion Lannister", "https://friendica.server.test/profile/tyrion", "tyrion", "https://upload.wikimedia.org/wikipedia/en/5/50/Tyrion_Lannister-Peter_Dinklage.jpg", ContactTypes.Friends);
                AddSampleUsers(postExtended3, postExtended3Thread, userJaime, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended3, postExtended3Thread, userTyrion, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended3, postExtended3Thread, null, PostFriendicaActivities.FriendicaActivity.dislike);

                sample = "{\"text\":\"Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:45 +0000 2015\",\"in_reply_to_status_id\":1,\"in_reply_to_status_id_str\":\"1\",\"source\":\"Friendica\",\"id\":2,\"id_str\":\"2\",\"in_reply_to_user_id\":1,\"in_reply_to_user_id_str\":\"1\",\"in_reply_to_screen_name\":\"test1\",\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":4,\"id_str\":\"4\",\"name\":\"Testuser #4\",\"screen_name\":\"test4\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":1,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":4,\"network\":\"dfrn\"},\"statusnet_html\":\"Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua.\",\"statusnet_conversation_id\":\"1\"}";
                var postExtended4 = new FriendicaPostExtended(sample);
                var postExtended4Thread = new FriendicaPostExtended(sample);
                postExtended4.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                postExtended4Thread.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                sampleObscoll.Add(postExtended4);
                var userArya = new FriendicaUserExtended(6, "Arya Stark", "https://friendica.server.text/profile/aryastark", "aryastark", "https://upload.wikimedia.org/wikipedia/en/3/39/Arya_Stark-Maisie_Williams.jpg", ContactTypes.Friends);
                var userCatelyn = new FriendicaUserExtended(10, "Catelyn Stark", "https://friendica.server.text/profile/catelyn", "catelyn", "https://upload.wikimedia.org/wikipedia/en/1/1b/Catelyn_Stark_S3.jpg", ContactTypes.Friends);
                var userSansa = new FriendicaUserExtended(5, "Sansa Stark", "https://friendica.server.text/profile/sansa", "sansa", "https://upload.wikimedia.org/wikipedia/en/7/74/SophieTurnerasSansaStark.jpg", ContactTypes.Friends);
                var userDaenerys = new FriendicaUserExtended(3, "Daenerys Targaryen", "https://friendica.server.text/profile/daenerys", "daenerys", "https://upload.wikimedia.org/wikipedia/en/0/0d/Daenerys_Targaryen_with_Dragon-Emilia_Clarke.jpg", ContactTypes.Friends);
                var userMyself = new FriendicaUserExtended();
                userMyself.IsAuthenticatedUser = true;
                AddSampleUsers(postExtended4, postExtended4Thread, userArya, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended4, postExtended4Thread, userCatelyn, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended4, postExtended4Thread, userSansa, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended4, postExtended4Thread, userDaenerys, PostFriendicaActivities.FriendicaActivity.dislike);
                AddSampleUsers(postExtended4, postExtended4Thread, userMyself, PostFriendicaActivities.FriendicaActivity.like);

                sample = "{\"text\":\"Heading\\n\\nLorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. :-)\\n\\nhttps://upload.wikimedia.org/wikipedia/commons/thumb/1/1c/FuBK_testcard_vectorized.svg/1200px-FuBK_testcard_vectorized.svg.png\\n\\nUt enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure eprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\\n\\nhttp://europa.eu/about-eu/basic-information/symbols/anthem/european-anthem-2012.mp3\",\"truncated\":false,\"created_at\":\"Wed Nov 04 20:03:00 +0000 2015\",\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"source\":\"Friendica\",\"id\":1,\"id_str\":\"1\",\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"geo\":null,\"location\":\"\",\"favorited\":false,\"user\":{\"id\":1,\"id_str\":\"1\",\"name\":\"Testuser #1\",\"screen_name\":\"test1\",\"location\":\"Friendica\",\"description\":null,\"profile_image_url\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"profile_image_url_https\":\"http://www.clker.com/cliparts/5/7/4/8/13099629981030824019profile.svg.med.png\",\"url\":\"SAMPLE\",\"protected\":true,\"followers_count\":0,\"friends_count\":0,\"created_at\":\"Thu Jan 22 18:57:04 +0000 2015\",\"favourites_count\":0,\"utc_offset\":\"0\",\"time_zone\":\"UTC\",\"statuses_count\":2,\"following\":true,\"verified\":true,\"statusnet_blocking\":false,\"notifications\":false,\"statusnet_profile_url\":\"\",\"cid\":1,\"network\":\"dfrn\"},\"statusnet_html\":\"<h4>\\nHeading\\n</h4>\\nLorem ipsum dolor sit amet, <u>consectetur</u> adipisici elit, sed eiusmod <strong>tempor incidunt</strong> ut labore et dolore magna aliqua. :-)<br><br><img src=\\\"https://upload.wikimedia.org/wikipedia/commons/thumb/1/1c/FuBK_testcard_vectorized.svg/1200px-FuBK_testcard_vectorized.svg.png\\\" alt=\\\"Bild/Foto\\\"><br><br>Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure <em>eprehenderit</em> in voluptate velit esse <span style=\\\"color: red;\\\">cillum dolore eu fugiat nulla pariatur.</span> Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.<br><br><a href=\\\"http://europa.eu/about-eu/basic-information/symbols/anthem/european-anthem-2012.mp3\\\" target=\\\"_blank\\\">http://europa.eu/about-eu/basic-information/symbols/anthem/european-anthem-2012.mp3</a>\",\"statusnet_conversation_id\":\"1\"}";
                var postExtended5 = new FriendicaPostExtended(sample);
                var postExtended5Thread = new FriendicaPostExtended(sample);
                postExtended5.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                postExtended5Thread.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0); 
                sampleObscoll.Add(postExtended5);
                AddSampleUsers(postExtended5, postExtended5Thread, null, PostFriendicaActivities.FriendicaActivity.like);
                AddSampleUsers(postExtended5, postExtended5Thread, null, PostFriendicaActivities.FriendicaActivity.dislike);

                foreach (var post in sampleObscoll)
                {
                    post.ButtonAddCommentClicked += Result_ButtonAddCommentClicked;
                    post.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                    post.ButtonShowThreadClicked += Result_ButtonShowThreadClicked;
                }

                NetworkPostsSample = sampleObscoll;
                OnPropertyChanged("NetworkPostsSample");

                var thread1 = new FriendicaThread();
                thread1.Posts = new ObservableCollection<FriendicaPostExtended>();
                thread1.ButtonAddCommentClicked += Thread_ButtonAddCommentClicked;
                thread1.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                thread1.ThreadId = 1;
                var thread2 = new FriendicaThread();
                thread2.ButtonAddCommentClicked += Thread_ButtonAddCommentClicked;
                thread2.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                thread2.Posts = new ObservableCollection<FriendicaPostExtended>();
                thread2.ThreadId = 4;

                postExtended5Thread.IsPostWithComments = true;
                thread1.Posts.Add(postExtended5Thread);
                thread1.Posts.Add(postExtended4Thread);
                thread1.Posts.Add(postExtended3Thread);
                thread1.Posts.Add(postExtended2Thread);
                thread1.LoadSampleData();
                thread1.NewPosts = NetworkPostsSample.Where(m => m.NewEntryIndicatorBorder.Bottom == 4);
                thread1.ReduceComments();
                thread1.IsLoaded = true;

                thread2.Posts.Add(postExtended1Thread);
                thread2.LoadSampleData();
                thread2.NewPosts = NetworkPostsSample.Where(m => m.NewEntryIndicatorBorder.Bottom == 4);
                thread2.ReduceComments();
                thread2.IsLoaded = true;

                NetworkThreadsSample.Add(thread2);
                NetworkThreadsSample.Add(thread1);
                OnPropertyChanged("NetworkThreadsSample");
                App.NetworkPosts = NetworkPostsSample;
                App.NetworkThreads = NetworkThreadsSample;
                
                App.TileCounter.CounterUnseenNetwork = 2;
                ScrollToTopColor = new SolidColorBrush(Colors.Red);
            }
            else
            {
                _noSettings = false;
                // load data from server
                IsLoadingInitial = true;
                var getNetworkInitial = new GetFriendicaNetwork();
                getNetworkInitial.FriendicaNetworkLoaded += GetNetworkInitial_FriendicaNetworkLoaded;
                getNetworkInitial.GetFriendicaNetworkInitial(20);
            }

            // in loading page we are always on top, so hide scroll button
            ShowScrollToTop = false;
        }

        private void AddSampleUsers(FriendicaPostExtended post, FriendicaPostExtended threadPost, FriendicaUserExtended user, PostFriendicaActivities.FriendicaActivity activity)
        {
            if (post.Post.PostFriendicaActivities == null)
                post.Post.PostFriendicaActivities = new FriendicaActivities();
            if (threadPost.Post.PostFriendicaActivities == null)
                threadPost.Post.PostFriendicaActivities = new FriendicaActivities();

            if (user != null)
            {
                if (activity == PostFriendicaActivities.FriendicaActivity.like)
                {
                    post.Post.PostFriendicaActivities.ActivitiesLike.Add(user);
                    threadPost.Post.PostFriendicaActivities.ActivitiesLike.Add(user);
                }
                else if (activity == PostFriendicaActivities.FriendicaActivity.dislike)
                {
                    post.Post.PostFriendicaActivities.ActivitiesDislike.Add(user);
                    threadPost.Post.PostFriendicaActivities.ActivitiesDislike.Add(user);
                }
            }

            post.SetActivitiesParameters();
            threadPost.SetActivitiesParameters();
        }

        private void Thread_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            var thread = sender as FriendicaThread;
            PostToShowThread = thread.SelectedItem;
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
        }

        private void Thread_ButtonAddCommentClicked(object sender, EventArgs e)
        {
            var thread = sender as FriendicaThread;
            PostToShowThread = thread.SelectedItem;
            if (ButtonAddCommentClicked != null)
                ButtonAddCommentClicked(this, EventArgs.Empty);
        }

        private async void GetNetworkInitial_FriendicaNetworkLoaded(object sender, EventArgs e)
        {
            var getNetworkInitial = sender as GetFriendicaNetwork;

            if (getNetworkInitial.StatusCode == HttpStatusCode.Ok)
            {
                // convert result into an observableCollection
                var obscoll = ConvertJsonToObsColl(getNetworkInitial);

                // insert result into displayed list of posts
                _networkPosts = obscoll;
                OnPropertyChanged("NetworkPosts");
                IsLoadingInitial = false;
                
                // retrieve maximum PostId value to store it in App.Settings for determining new posts
                SaveSettingMaximumNetworkPostId(_networkPosts);
                
                // set counter on menu item to show the new entries
                var countNewEntries = _networkPosts.Count(m => m.NewEntryIndicatorBorder.Bottom == 4);
                App.TileCounter.CounterUnseenNetwork = countNewEntries;

                // Anzeige einer Info triggern, die dem User anzeigt, dass die Daten geladen wurden, aber nichts gekommen ist
                if (_networkPosts == null || _networkPosts.Count == 0)
                    NoDataAvailable = true;
                else
                    NoDataAvailable = false;

                // load all Users in App.AllFriendicaUser if not already there
                UpdateTblAllKnownUsers(obscoll);

                // background loading for threads and display them
                LoadThreadData(obscoll, false);
            }
            else
            {
                IsLoadingInitial = false;
                // es gab einen Fehler beim Abrufen der Daten
                // "Fehler beim Laden der Daten. (Fehlermeldung: xxx) \n\n Möchten Sie es erneut versuchen?"
                string errorMsg;
                if (getNetworkInitial.StatusCode == HttpStatusCode.None)
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), loader.GetString("messageDialogHttpStatusNone"));
                else
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), getNetworkInitial.StatusCode.ToString());
                var dialog = new MessageDialog(errorMsg);
                //var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                    LoadInitial();
                else
                {
                    IsRefreshing = false;
                    NoDataAvailable = true;
                }
            }
        }


        public void LoadNextEntries()
        {
            if (!IsLoadingNextEntries)
            {
                var getNetworkNextEntries = new GetFriendicaNetwork();
                getNetworkNextEntries.FriendicaNetworkLoaded += GetNetworkNextEntries_FriendicaNetworkLoaded;
                // reduce minimum ID by 1 to avoid retrieving the oldest post again
                var oldestId = _networkPosts.Min(m => m.Post.PostId) - 1;
                // oldestId may not be negative or zero, otherwise API returns the newest posts again
                if (oldestId > 0)
                {
                    IsLoadingNextEntries = true;
                    getNetworkNextEntries.GetFriendicaNetworkNextEntries(oldestId, 20);
                }
            }
        }

        private async void GetNetworkNextEntries_FriendicaNetworkLoaded(object sender, EventArgs e)
        {
            var getNetworkNextEntries = sender as GetFriendicaNetwork;

            if (getNetworkNextEntries.StatusCode == HttpStatusCode.Ok)
            {
                // convert result into an observableCollection
                var obscoll = ConvertJsonToObsColl(getNetworkNextEntries);

                // insert result into displayed list of posts
                foreach (var post in obscoll)
                {
                    _networkPosts.Add(post);
                }
                OnPropertyChanged("NetworkPosts");
                IsLoadingNextEntries = false;

                // retrieve maximum PostId value to store it in App.Settings for determining new posts
                SaveSettingMaximumNetworkPostId(_networkPosts);
                
                // set counter on menu item to show the new entries
                var countNewEntries = _networkPosts.Count(m => m.NewEntryIndicatorBorder.Bottom == 4);
                App.TileCounter.CounterUnseenNetwork = countNewEntries;

                // Anzeige einer Info triggern, die dem User anzeigt, dass die Daten geladen wurden, aber nichts gekommen ist
                if (_networkPosts == null || _networkPosts.Count == 0)
                    NoDataAvailable = true;
                else
                    NoDataAvailable = false;

                // load all Users in App.AllFriendicaUser if not already there
                UpdateTblAllKnownUsers(obscoll);

                // background loading for threads and display them
                LoadThreadData(obscoll, false);
            }
            else
            {
                IsLoadingNextEntries = false;
                // es gab einen Fehler beim Abrufen der Daten
                // "Fehler beim Laden der Daten. (Fehlermeldung: xxx) \n\n Möchten Sie es erneut versuchen?"
                string errorMsg;
                if (getNetworkNextEntries.StatusCode == HttpStatusCode.None)
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), loader.GetString("messageDialogHttpStatusNone"));
                else
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), getNetworkNextEntries.StatusCode.ToString());
                var dialog = new MessageDialog(errorMsg);
                //var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                    LoadNextEntries();
                else
                {
                    IsRefreshing = false;
                }
            }
        }


        public void LoadNewEntries()
        {
            var getNetworkNewEntries = new GetFriendicaNetwork();
            getNetworkNewEntries.FriendicaNetworkLoaded += GetNetworkNewEntries_FriendicaNetworkLoaded;
            double newestId = 0;
            if (_networkPosts.Count != 0)
                newestId = _networkPosts.Max(m => m.Post.PostId);
            IsRefreshing = true;
            getNetworkNewEntries.GetFriendicaNetworkNewEntries(newestId, 20);
        }


        private async void GetNetworkNewEntries_FriendicaNetworkLoaded(object sender, EventArgs e)
        {
            var getNetworkNewEntries = sender as GetFriendicaNetwork;

            if (getNetworkNewEntries.StatusCode == HttpStatusCode.Ok)
            {
                // convert result into an observableCollection
                var obscoll = ConvertJsonToObsColl(getNetworkNewEntries);

                // insert result into displayed list of posts
                int position = 0;
                foreach (var post in obscoll)
                {
                    NetworkPosts.Insert(position, post);
                    position += 1;
                }
                OnPropertyChanged("NetworkPosts");

                // retrieve maximum PostId value to store it in App.Settings for determining new posts
                SaveSettingMaximumNetworkPostId(_networkPosts);
                
                // set counter on menu item to show the new entries
                var countNewEntries = _networkPosts.Count(m => m.NewEntryIndicatorBorder.Bottom == 4);
                App.TileCounter.CounterUnseenNetwork = countNewEntries;

                // set scrollToTopColor to red if new elements loaded
                if (App.TileCounter.CounterUnseenNetwork != 0)
                    ScrollToTopColor = new SolidColorBrush(Colors.Red);

                // Anzeige einer Info triggern, die dem User anzeigt, dass die Daten geladen wurden, aber nichts gekommen ist
                if (_networkPosts == null || _networkPosts.Count == 0)
                    NoDataAvailable = true;
                else
                    NoDataAvailable = false;

                // load all Users in App.AllFriendicaUser if not already there
                UpdateTblAllKnownUsers(obscoll);
               
                // background loading for threads and display them
                LoadThreadData(obscoll, true);

                //IsRefreshing = false;
            }
            else
            {
                IsRefreshing = false;
                // es gab einen Fehler beim Abrufen der Daten
                // "Fehler beim Laden der Daten. (Fehlermeldung: xxx) \n\n Möchten Sie es erneut versuchen?"
                string errorMsg;
                if (getNetworkNewEntries.StatusCode == HttpStatusCode.None)
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), loader.GetString("messageDialogHttpStatusNone"));
                else
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), getNetworkNewEntries.StatusCode.ToString());
                var dialog = new MessageDialog(errorMsg);
                //var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                    LoadNewEntries();
                else
                {
                    if (NetworkPosts.Count == 0)
                        NoDataAvailable = true;
                }
            }
        }


        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth")
                OnPropertyChanged("ListViewWidth");
            if (e.PropertyName == "OrientationDevice")
            {
                OnPropertyChanged("ListViewWidth");
                SetShowDragArea();
            }
        }

        private void SetShowDragArea()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                        || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                ShowDragArea = false;
            }
            else
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.DesktopLandscape || App.Settings.OrientationDevice == OrientationDeviceFamily.DesktopPortrait)
                    ShowDragArea = true;
                else
                    ShowDragArea = false;
            }

        }

        private ObservableCollection<FriendicaPostExtended> ConvertJsonToObsColl(GetFriendicaNetwork httpResult)
        {
            var obscoll = new ObservableCollection<FriendicaPostExtended>();
            JsonArray resultArray = JsonArray.Parse(httpResult.ReturnString);
            int arraySize = resultArray.Count;
            for (int i = 0; i < arraySize; i++)
            {
                IJsonValue element = resultArray.GetArray()[i];
                switch (element.ValueType)
                {
                    case JsonValueType.Object:
                        var result = new FriendicaPostExtended(element.ToString());
                        result.ButtonShowThreadClicked += Result_ButtonShowThreadClicked;
                        result.ButtonAddCommentClicked += Result_ButtonAddCommentClicked;
                        result.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                        obscoll.Add(result);
                        break;
                }
            }
            return obscoll;
        }

        private void Result_ButtonAddCommentClicked(object sender, EventArgs e)
        {
            PostToShowThread = sender as FriendicaPostExtended;
            if (ButtonAddCommentClicked != null)
                ButtonAddCommentClicked(this, EventArgs.Empty);
        }

        private void Result_ButtonShowThreadClicked(object sender, EventArgs e)
        {
            PostToShowThread = sender as FriendicaPostExtended;
            if (ButtonShowThreadClicked != null)
                ButtonShowThreadClicked(this, EventArgs.Empty);
        }

        private void Result_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            PostToShowThread = sender as FriendicaPostExtended;
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
        }

        private void SaveSettingMaximumNetworkPostId(ObservableCollection<FriendicaPostExtended> obscoll)
        {
            if (obscoll != null && obscoll.Count != 0)
            {
                // retrieve maximum PostId value to store it in App.Settings for determining new posts
                var maxId = obscoll.Max(m => m.Post.PostId);
                var minId = obscoll.Min(m => m.Post.PostId);
                // on new installations, saved id is still zero
                if (maxId > App.Settings.LastReadNetworkPost && minId > App.Settings.LastReadNetworkPost && App.Settings.LastReadNetworkPost != 0 && App.TileCounter.CounterUnseenNetwork < 99)
                    LoadNextEntries();
                else if (maxId > App.Settings.LastReadNetworkPost)
                    App.Settings.LastReadNetworkPost = maxId;
            }
        }


        private void UpdateTblAllKnownUsers(ObservableCollection<FriendicaPostExtended> obscoll)
        {
            if (App.Settings.SaveLocalAllowed == "true")
            {
                foreach (var friendicaPostExtended in obscoll)
                {
                    if (friendicaPostExtended.Post.PostUser != null)
                    {
                        var user = App.sqliteConnection.SelectSingleFromtblAllKnownUsers(friendicaPostExtended.Post.PostUser.User.UserUrl);
                        if (user == null)
                        {
                            var isSucceeded = App.sqliteConnection.InsertIntotblAllKnownUsers(friendicaPostExtended.Post.PostUser.User);
                        }
                    }

                    if (friendicaPostExtended.Post.PostRetweetedStatus != null)
                    {
                        if (friendicaPostExtended.Post.PostRetweetedStatus.PostUser != null)
                        {
                            var user = App.sqliteConnection.SelectSingleFromtblAllKnownUsers(friendicaPostExtended.Post.PostRetweetedStatus.PostUser.User.UserUrl);
                            if (user == null)
                            {
                                var isSucceeded = App.sqliteConnection.InsertIntotblAllKnownUsers(friendicaPostExtended.Post.PostRetweetedStatus.PostUser.User);
                            }
                        }
                    }
                }
            }
        }


        private void LoadThreadData(ObservableCollection<FriendicaPostExtended> obscollPosts, bool isRefresh)
        {
            int position = 0;
            foreach (var post in obscollPosts)
            {
                // retrieve PostInReplyToStatusIdStr as the PostInReplyToStatusId might link to different id's which are not in database
                double id = 0;
                if (post.Post.PostInReplyToStatusId == 0)
                    id = post.Post.PostId;
                else if (post.Post.PostInReplyToStatusId != 0)
                    id = Convert.ToDouble(post.Post.PostInReplyToStatusIdStr);

                if (id != 0)
                {
                    if (NetworkThreads != null)
                    {
                        var result = NetworkThreads.Count(w => w.ThreadId == id);
                        if (result > 0)
                            continue;
                    }

                    var thread = new FriendicaThread();
                    thread.ThreadId = id;
                    thread.IsLoaded = false;
                    if (isRefresh)
                    {
                        NetworkThreads.Insert(position, thread);
                        position += 1;
                    }
                    else
                        NetworkThreads.Add(thread);
                }
            }

            var newPosts = NetworkPosts.Where(m => m.NewEntryIndicatorBorder.Bottom == 4);

            foreach (var thread in NetworkThreads)
            {
                thread.NewPosts = newPosts;
                thread.PostsLoaded += Thread_PostsLoaded;
                thread.LoadThread();
            }
        }

        private void Thread_PostsLoaded(object sender, EventArgs e)
        {
            var allLoaded = CheckIfAllThreadsAreLoaded();
            if (allLoaded)
                IsRefreshing = false;

            var thread = sender as FriendicaThread;
            // mark all new posts with border
            var newPosts = NetworkPosts.Where(m => m.NewEntryIndicatorBorder.Bottom == 4);
            foreach (var post in thread.Posts)
            {
                post.ButtonAddCommentClicked += Result_ButtonAddCommentClicked;
                post.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                foreach (var newPost in newPosts)
                {
                    if (post.Post.PostIdStr == newPost.Post.PostIdStr)
                    {
                        post.NewEntryIndicatorBorder = new Thickness(4, 4, 4, 4);
                        post.IsVisible = true;
                    }
                }
            }
        }

        private bool CheckIfAllThreadsAreLoaded()
        {
            bool _isLoaded = true;
            foreach (var thread in NetworkThreads)
                _isLoaded &= thread.IsLoaded;
            return _isLoaded;
        }

    }
}