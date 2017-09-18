using Friendica_Mobile.PCL.HttpRequests;
using Friendica_Mobile.PCL.Models;
using Friendica_Mobile.PCL.Strings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.Viewmodels
{
    public class NetworkViewmodel : BindableClass
    {
        // indicator showing that we do not need to start initial load if it has already been done
        private bool _initialLoadDone;
        public bool InitialLoadDone
        {
            get { return _initialLoadDone; }
            set { _initialLoadDone = value; }
        }

        // indicator that we have no settings set, so we show an info to user
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set { _noSettings = value;
                OnPropertyChanged("NoSettings"); }
        }

        // indicator showing that we are loading the initial data from server
        private bool _isLoadingInitial;
        public bool IsLoadingInitial
        {
            get { return _isLoadingInitial; }
            set { _isLoadingInitial = value;
                RefreshCommand.RaiseCanExecuteChanged();
                SetAllSeenCommand.RaiseCanExecuteChanged();
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingInitial"); }
        }

        // indicator showing that we are loading the next portion of posts
        private bool _isLoadingNext;
        public bool IsLoadingNext
        {
            get { return _isLoadingNext; }
            set { _isLoadingNext = value;
                SetAllSeenCommand.RaiseCanExecuteChanged();
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingNext"); }
        }

        // indicator showing that we are refreshing (loading new entries from server)
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { _isRefreshing = value;
                RefreshCommand.RaiseCanExecuteChanged();
                SetAllSeenCommand.RaiseCanExecuteChanged();
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsRefreshing"); }
        }

        // indicator showing that we are currently sending a post to the server
        private bool _isSendingNewPost;
        public bool IsSendingNewPost
        {
            get { return _isSendingNewPost; }
            set { _isSendingNewPost = value;
                OnPropertyChanged("IsSendingNewPost"); }
        }

        // indicator showing that we have not get any datasets from server
        private bool _noDataAvailableNetwork;
        public bool NoDataAvailableNetwork
        {
            get { return _noDataAvailableNetwork; }
            set { _noDataAvailableNetwork = value;
                OnPropertyChanged("NoDataAvailableNetwork"); }
        }

        // indicator showing that we have not get any newsfeed items from server
        private bool _noDataAvailableNewsfeed;
        public bool NoDataAvailableNewsfeed
        {
            get { return _noDataAvailableNewsfeed; }
            set
            {
                _noDataAvailableNewsfeed = value;
                OnPropertyChanged("NoDataAvailableNewsfeed");
            }
        }

        // container saving the results from http requests
        private List<FriendicaPost> _posts;
        public List<FriendicaPost> Posts
        {
            get { return _posts; }
            set { _posts = value; }
        }

        // collection with all threads for network page
        private ObservableCollection<FriendicaThread> _networkThreads;
        public ObservableCollection<FriendicaThread> NetworkThreads
        {
            get { return _networkThreads; }
            set { _networkThreads = value; }
        }

        // collection with all threads for newsfeed page
        private ObservableCollection<FriendicaThread> _newsfeedThreads;
        public ObservableCollection<FriendicaThread> NewsfeedThreads
        {
            get { return _newsfeedThreads; }
            set { _newsfeedThreads = value;
                OnPropertyChanged("NewsfeedThreads");
            }
        }

        // list to save the network threads when moving away to another page (keeping would cause a layout loop)
        private List<FriendicaThread> _networkThreadsContainer;
        public List<FriendicaThread> NetworkThreadsContainer
        {
            get { return _networkThreadsContainer; }
            set { _networkThreadsContainer = value; }
        }

        // list to save the newsfeeds when moving away to another page (keeping would cause a layout loop)
        private List<FriendicaThread> _newsfeedThreadsContainer;
        public List<FriendicaThread> NewsfeedThreadsContainer
        {
            get { return _newsfeedThreadsContainer; }
            set { _newsfeedThreadsContainer = value; }
        }


        // eventhandlers
        public event EventHandler ButtonAddCommentClicked;
        public event EventHandler ButtonShowProfileClicked;
        public event EventHandler ButtonRetweetClicked;
        public event EventHandler LikeNewsfeedClicked;
        public event EventHandler ButtonAddNewEntryClicked;
        public event EventHandler ButtonShowMapClicked;
        public event EventHandler UserShowProfileClicked;


        // constructor
        public NetworkViewmodel()
        {
            Posts = new List<FriendicaPost>();
            NetworkThreadsContainer = new List<FriendicaThread>();
            NewsfeedThreadsContainer = new List<FriendicaThread>();
        }


        // commands

        // refresh button
        Command _refreshCommand;
        public Command RefreshCommand { get { return _refreshCommand ?? (_refreshCommand = new Command(ExecuteRefresh, CanRefresh)); } }
        private bool CanRefresh()
        {
            if (_isLoadingInitial || _isRefreshing)
                return false;
            else
                return true;
        }

        private async void ExecuteRefresh()
        {
            // don't start refreshing if we have already started it before
            if (IsRefreshing)
                return;

            // hint to user on NoSetting and then abort
            if (NoSettings)
            {
                // show info to the user once per session
                if (!StaticGlobalParameters.NetworkNoSettingsAlreadyShownRefresh)
                {
                    var result = await StaticMessageDialog.ShowDialogAsync("", AppResources.messageDialogNetworkNoSettings, "OK", null, null, 0, 0);
                    StaticGlobalParameters.NetworkNoSettingsAlreadyShownRefresh = true;
                }
                else
                    await Task.Delay(3000);
                IsRefreshing = false;
                return;
            }

            // remove borders indicating new entries in network and set counter to zero
            SetNewEntriesToSeen();

            await Refresh();
        }


        // command for setting all newsfeed threads to seen state
        Command _setAllSeenCommand;
        public Command SetAllSeenCommand { get { return _setAllSeenCommand ?? (_setAllSeenCommand = new Command(ExecuteSetAllSeen, CanSetAllSeen)); } }
        private bool CanSetAllSeen()
        {
            if (_isLoadingInitial || _isRefreshing || _isRefreshing)
                return false;
            else
                return true;
        }

        private void ExecuteSetAllSeen()
        {
            // set all Newsfeed-Threads to seen
            if (NewsfeedThreads != null)
            {
                foreach (var thread in NewsfeedThreads)
                {
                    if (thread.MainPost != null && thread.MainPost.Count != 0)
                        thread.MainPost[0].IsNewEntry = false;

                    foreach (var post in thread.Comments)
                        post.IsNewEntry = false;
                    foreach (var post in thread.CommentsDisplay)
                        post.IsNewEntry = false;
                }
            }
            UpdateCounters();

            // update setting with the id's of unseen posts to an empty string
            Settings.UnseenNewsfeedItems = new List<int>();
        }


        // command for button to start a new conversation
        Command _addNewEntryCommand;
        public Command AddNewEntryCommand { get { return _addNewEntryCommand ?? (_addNewEntryCommand = new Command(ExecuteAddNewEntry, CanAddNewEntry)); } }
        private bool CanAddNewEntry()
        {
            if (IsLoadingInitial || IsRefreshing || IsLoadingNext)
                return false;
            else
                return true;
        }

        private void ExecuteAddNewEntry()
        {
            // fire trigger to navigate to NewPost page
            ButtonAddNewEntryClicked?.Invoke(this, EventArgs.Empty);
        }

        
        // main functions 

        // load posts if nothing loaded so far
        public async Task LoadInitial()
        {
            if (InitialLoadDone)
            {
                // as the initial load has been performed before, we will now only perform a refresh
                Refresh();
            }
            else
            {
                if (CheckSettings.ServerSettingsAvailable())
                {
                    NoSettings = false;
                    IsLoadingInitial = true;

                    // start the initial load
                    var getNetworkInitial = new GetFriendicaNetwork();
                    await getNetworkInitial.GetNetworkInitialAsync(20);

                    switch (getNetworkInitial.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                            if (getNetworkInitial.Posts == null || getNetworkInitial.Posts.Count == 0)
                            {
                                NoDataAvailableNetwork = true;
                                NoDataAvailableNewsfeed = true;
                            }
                            else
                            {
                                // save results of http requests to a List<FriendicaPost> 
                                Posts = getNetworkInitial.Posts;

                                foreach (var post in getNetworkInitial.Posts)
                                {
                                    WorkOnEachPost(post);
                                }

                                // update NoDataAvailable indicators
                                UpdateNoDataAvailable();

                                // update counters for displaying numbers of new items on navigation bar
                                UpdateCounters();

                                // retrieve maximum PostId value to store it in App.Settings for determining new posts
                                var maxId = Posts.Max(p => p.Post.PostId);
                                var minId = Posts.Min(p => p.Post.PostId);

                                if (Settings.UnseenNewsfeedItems == null)
                                    Settings.UnseenNewsfeedItems = new List<int>();

                                // on new installations, saved id is still zero
                                if ((maxId > Settings.LastReadNetworkPost
                                    && minId > Settings.LastReadNetworkPost
                                    && Settings.LastReadNetworkPost != 0
                                    && StaticGlobalParameters.CounterUnseenNetwork < 99) ||
                                    (Settings.UnseenNewsfeedItems.Count > 0
                                    && minId > Settings.UnseenNewsfeedItems.Min()
                                    && StaticGlobalParameters.CounterUnseenNewsfeed < 99))
                                    await LoadNext();

                                // continue to load if we haven't yet received some network items
                                if ((NetworkThreads != null && NetworkThreads.Count == 0 && Posts.Count != 0) ||
                                    (NetworkThreads == null && NetworkThreadsContainer.Count == 0 && Posts.Count != 0))
                                    await LoadNext();
                            }
                            // set this after the initial load has been performed to avoid another initial load
                            InitialLoadDone = true;
                            break;
                        default:
                            // we had an error when loading the data, asking user to try it again or not
                            var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getNetworkInitial.StatusCode.ToString());
                            var result = await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonYes, AppResources.buttonNo, null, 0, 1);

                            if (result == 0)
                                await LoadInitial();
                            else
                            {
                                NoDataAvailableNetwork = true;
                                NoDataAvailableNewsfeed = true;
                            }
                            break;
                    }
                    IsLoadingInitial = false;
                }
                else
                {
                    // we are in samples mode
                    NoSettings = true;
                    var samples = new FriendicaSampleData();

                    // prepare samples for network page
                    var sampleThreads = samples.PrepareNetworkSamples();
                    foreach (var thread in sampleThreads)
                    {
                        if (NetworkThreads != null)
                            NetworkThreads.Add(thread);
                        else
                            NetworkThreadsContainer.Add(thread);

                        var mainPost = thread.MainPost[0];

                        thread.MainPost[0].ButtonAddCommentClicked += Post_ButtonAddCommentClicked;
                        thread.MainPost[0].ButtonShowProfileClicked += Post_ButtonShowProfileClicked;
                        thread.MainPost[0].ButtonRetweetClicked += Post_ButtonRetweetClicked;
                        thread.MainPost[0].LikeNewsfeedClicked += Post_LikeNewsfeedClicked;
                        thread.MainPost[0].ButtonShowMapClicked += Post_ButtonShowMapClicked;

                        foreach (var user in mainPost.LikesForDisplay)
                            user.ButtonShowProfileClicked += User_ButtonShowProfileClicked;
                        foreach (var user in mainPost.DislikesForDisplay)
                            user.ButtonShowProfileClicked += User_ButtonShowProfileClicked;

                        foreach (var post in thread.Comments)
                        {
                            post.ButtonAddCommentClicked += Post_ButtonAddCommentClicked;
                            post.ButtonShowProfileClicked += Post_ButtonShowProfileClicked;
                            post.ButtonRetweetClicked += Post_ButtonRetweetClicked;
                            post.ButtonShowProfileClicked += Post_ButtonShowMapClicked;

                            foreach (var user in post.LikesForDisplay)
                                user.ButtonShowProfileClicked += User_ButtonShowProfileClicked;
                            foreach (var user in post.DislikesForDisplay)
                                user.ButtonShowProfileClicked += User_ButtonShowProfileClicked;
                        }
                    }

                    // prepare samples for the newsfeed page
                    sampleThreads = samples.PrepareNewsfeedSamples();
                    foreach (var thread in sampleThreads)
                    {
                        if (NewsfeedThreads != null)
                            NewsfeedThreads.Add(thread);
                        else
                            NewsfeedThreadsContainer.Add(thread);

                        var mainPost = thread.MainPost[0];

                        thread.MainPost[0].ButtonRetweetClicked += Post_ButtonRetweetClicked;
                        thread.MainPost[0].LikeNewsfeedClicked += Post_LikeNewsfeedClicked;
                        thread.MainPost[0].ButtonShowMapClicked += Post_ButtonShowMapClicked;
                    }

                    // update counters for displaying numbers of new items on navigation bar
                    UpdateCounters();

                    InitialLoadDone = true;
                }
            }
        }


        private void User_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            UserShowProfileClicked?.Invoke(sender, e);
        }

        public async Task LoadNext()
        {
            // we are already on loading, abort
            //if (IsLoadingNext)
            //    return;

            // hint to user on NoSetting and then abort
            if (NoSettings)
            {
                if (!StaticGlobalParameters.NetworkNoSettingsAlreadyShownNext)
                {
                    IsLoadingNext = true;
                    // show info to the user
                    var result = await StaticMessageDialog.ShowDialogAsync("", AppResources.messageDialogNetworkNoSettings, "OK", null, null, 0, 0);
                    IsLoadingNext = false;
                    StaticGlobalParameters.NetworkNoSettingsAlreadyShownNext = true;
                }
                return;
            }

            var getNetworkNext = new GetFriendicaNetwork();
            // reduce minimum ID by 1 to avoid retrieving the oldest post again
            var oldestId = Posts.Min(m => m.Post.PostId) - 1;
            // oldestId may not be negative or zero, otherwise API returns the newest posts again
            if (oldestId <= 0)
                return;

            // now we can start with the retrieving from server
            IsLoadingNext = true;
            await getNetworkNext.GetNetworkNextAsync(oldestId, 20);

            switch (getNetworkNext.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    // save results of http requests to the Posts list and check the thread id
                    foreach (var post in getNetworkNext.Posts)
                    {
                        // set IsNewEntry on newsfeed when id is containing in the list stored in the settings
                        if (Settings.UnseenNewsfeedItems.Contains((int)post.Post.PostId))
                            post.IsNewEntry = true;

                        IsLoadingNext = true;
                        Posts.Add(post);
                        WorkOnEachPost(post);
                    }

                    // update NoDataAvailable indicators
                    UpdateNoDataAvailable();

                    // update counters for displaying numbers of new items on navigation bar
                    UpdateCounters();

                    // retrieve maximum PostId value to store it in App.Settings for determining new posts
                    var maxId = Posts.Max(p => p.Post.PostId);
                    var minId = Posts.Min(p => p.Post.PostId);

                    // on new installations, saved id is still zero
                    if ((maxId > Settings.LastReadNetworkPost 
                        && minId > Settings.LastReadNetworkPost 
                        && Settings.LastReadNetworkPost != 0 
                        && StaticGlobalParameters.CounterUnseenNetwork < 99) || 
                        (Settings.UnseenNewsfeedItems.Count > 0 
                        && minId > Settings.UnseenNewsfeedItems.Min() 
                        && StaticGlobalParameters.CounterUnseenNewsfeed < 99))
                        await LoadNext();

                    // continue to load if we haven't yet received some network items
                    if ((NetworkThreads != null && NetworkThreads.Count == 0 && Posts.Count != 0) ||
                        (NetworkThreads == null && NetworkThreadsContainer.Count == 0 && Posts.Count != 0))
                        await LoadNext();

                    //else
                    //    IsLoadingNext = false;
                    break;
                default:
                    // there was an error when calling the data from the server
                    var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getNetworkNext.StatusCode.ToString());
                    var result = await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonYes, AppResources.buttonNo, null, 0, 1);

                    if (result == 0)
                        await LoadNext();
                    else
                        IsLoadingNext = false;
                    break;
            }
        }


        public async Task Refresh()
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
                var getNetworkRefresh = new GetFriendicaNetwork();
                double newestId = 0;
                if (Posts.Count != 0)
                    newestId = Posts.Max(m => m.Post.PostId);
                await getNetworkRefresh.GetNetworkNewAsync(newestId, 20);

                switch (getNetworkRefresh.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        // save results of http requests to the Posts list and check the thread id
                        int position = 0;

                        if (getNetworkRefresh.Posts.Count == 0)
                            IsRefreshing = false;
                        else
                        {
                            foreach (var post in getNetworkRefresh.Posts)
                            {
                                Posts.Insert(position, post);
                                position += 1;
                                WorkOnEachPost(post, true);
                            }
                        }

                        // Anzeige einer Info triggern, die dem User anzeigt, dass die Daten geladen wurden, aber nichts gekommen ist
                        if (Posts == null || Posts.Count == 0)
                        {
                            NoDataAvailableNetwork = true;
                            NoDataAvailableNewsfeed = true;
                        }
                        else
                            UpdateNoDataAvailable();

                        // update counters for displaying numbers of new items on navigation bar
                        UpdateCounters();

                        // retrieve maximum PostId value to store it in App.Settings for determining new posts
                        var maxId = Posts.Max(p => p.Post.PostId);
                        var minId = Posts.Min(p => p.Post.PostId);

                        // on new installations, saved id is still zero
                        if ((maxId > Settings.LastReadNetworkPost
                            && minId > Settings.LastReadNetworkPost
                            && Settings.LastReadNetworkPost != 0
                            && StaticGlobalParameters.CounterUnseenNetwork < 99) ||
                            (Settings.UnseenNewsfeedItems.Count > 0
                            && minId > Settings.UnseenNewsfeedItems.Min()
                            && StaticGlobalParameters.CounterUnseenNewsfeed < 99))
                            await LoadNext();
                        break;
                    default:
                        // there was an error when calling the data from the server
                        var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getNetworkRefresh.StatusCode.ToString());
                        var result = await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonYes, AppResources.buttonNo, null, 0, 1);

                        if (result == 0)
                            Refresh();
                        else
                        {
                            IsRefreshing = false;
                            if (Posts.Count == 0)
                            {
                                NoDataAvailableNetwork = true;
                                NoDataAvailableNewsfeed = true;
                            }
                        }
                        break;
                }
            }
        }

        private void WorkOnEachPost(FriendicaPost post, bool isRefresh = false)
        {
            UpdateUnseenNewsfeedItems(post);

            // get thread id
            var id = post.GetThreadId();

            // check if we do have the id already in our lists
            var networkCount = (NetworkThreads == null) ? NetworkThreadsContainer.Count(th => th.ThreadId == id) : NetworkThreads.Count(th => th.ThreadId == id);
            var newsfeedCount = (NewsfeedThreads == null) ? NewsfeedThreadsContainer.Count(th => th.ThreadId == id) : NewsfeedThreads.Count(th => th.ThreadId == id);
            var idAlreadyUsed = networkCount + newsfeedCount;

            if (idAlreadyUsed == 0)
            {
                var thread = new FriendicaThread() { ThreadId = id };
                thread.ThreadIsLoaded += Thread_ThreadIsLoaded;
                thread.LoadThreadData();
                if (post.PostType == PostTypes.Newsfeed)
                {
                    if (NewsfeedThreads != null)
                    {
                        // insert as first element when we are in refresh mode
                        if (isRefresh)
                            NewsfeedThreads.Insert(0, thread);
                        else
                            NewsfeedThreads.Add(thread);
                    }
                    else
                    {
                        if (isRefresh)
                            NewsfeedThreadsContainer.Insert(0, thread);
                        else
                            NewsfeedThreadsContainer.Add(thread);
                    }
                }
                else
                {
                    if (NetworkThreads != null)
                    {
                        if (isRefresh)
                            NetworkThreads.Insert(0, thread);
                        else
                            NetworkThreads.Add(thread);
                    }
                    else
                    {
                        if (isRefresh)
                            NetworkThreadsContainer.Insert(0, thread);
                        else
                            NetworkThreadsContainer.Add(thread);
                    }
                }
            }
            else
            {
                // refresh the thread if there was a new post
                if (isRefresh)
                {
                    FriendicaThread thread;
                    if (post.PostType == PostTypes.Newsfeed)
                    {
                        if (NewsfeedThreads == null)
                            thread = NewsfeedThreadsContainer.SingleOrDefault(th => th.ThreadId == id);
                        else
                            thread = NewsfeedThreads.SingleOrDefault(th => th.ThreadId == id);
                    }
                    else
                    {
                        if (NetworkThreads == null)
                            thread = NetworkThreadsContainer.SingleOrDefault(th => th.ThreadId == id);
                        else
                            thread = NetworkThreads.SingleOrDefault(th => th.ThreadId == id);
                    }
                    thread.LoadThreadData();
                }
            }
        }

        private void UpdateNoDataAvailable()
        {
            int countNetwork = 0;
            int countNewsfeed = 0;

            countNetwork = (NetworkThreads == null) ? NetworkThreadsContainer.Count : NetworkThreads.Count;
            countNewsfeed = (NewsfeedThreads == null) ? NewsfeedThreadsContainer.Count : NewsfeedThreads.Count;

            NoDataAvailableNetwork = (countNetwork == 0);
            NoDataAvailableNewsfeed = (countNewsfeed == 0);
        }


        private void UpdateUnseenNewsfeedItems(FriendicaPost post)
        {
            if (post.PostType == PostTypes.Newsfeed)
            {
                // null will cause crashes
                if (Settings.UnseenNewsfeedItems == null)
                    Settings.UnseenNewsfeedItems = new List<int>();

                var id = (int)post.Post.PostId;
                var list = Settings.UnseenNewsfeedItems;
                // place the id in the list if it is an empty list so far
                if (list.Count == 0)
                {
                    if (id > Settings.LastReadNetworkPost)
                        list.Add(id);
                }
                else
                {
                    // add post to list only if higher than LastReadNetworkPost (to ensure that we get all elements when having an empty list) ...
                    if (!list.Contains(id) && id > Settings.LastReadNetworkPost)
                        list.Add(id);
                    // ... or if newer than previously added items (if there is a list available)
                    else if (!list.Contains(id) && id > list.Min())
                        list.Add(id);
                }
                Settings.UnseenNewsfeedItems = list;
            }
        }


        private async void Thread_ThreadIsLoaded(object sender, EventArgs e)
        {
            // working on the current thread
            var thread = sender as FriendicaThread;
            thread.MainPost[0].ButtonAddCommentClicked += Post_ButtonAddCommentClicked;
            thread.MainPost[0].ButtonShowProfileClicked += Post_ButtonShowProfileClicked;
            thread.MainPost[0].ButtonRetweetClicked += Post_ButtonRetweetClicked;
            thread.MainPost[0].LikeNewsfeedClicked += Post_LikeNewsfeedClicked;
            thread.MainPost[0].ButtonShowMapClicked += Post_ButtonShowMapClicked;
            foreach (var post in thread.Comments)
            {
                post.ButtonAddCommentClicked += Post_ButtonAddCommentClicked;
                post.ButtonShowProfileClicked += Post_ButtonShowProfileClicked;
                post.ButtonRetweetClicked += Post_ButtonRetweetClicked;
                post.ButtonShowProfileClicked += Post_ButtonShowMapClicked;
            }

            // update counters
            UpdateCounters();

            // check if we are now done with all threads 
            var allLoaded = CheckIfAllThreadsAreLoaded();
            if (!allLoaded)
                return;
            else
            {
                // working after last thread has been loaded

                // update table with the known users for the profile link in the text
                await UpdateTblAllKnownUsersAsync();

                // set LastReadNetworkPost in settings
                var maxId = Posts.Max(p => p.Post.PostId);
                if (maxId > Settings.LastReadNetworkPost)
                    Settings.LastReadNetworkPost = maxId;

                // set this after all thread have been loaded
                IsLoadingInitial = false;
                IsLoadingNext = false;
                IsRefreshing = false;
            }
        }

        private void Post_ButtonShowMapClicked(object sender, EventArgs e)
        {
            ButtonShowMapClicked?.Invoke(sender, e);
        }

        private void Post_LikeNewsfeedClicked(object sender, EventArgs e)
        {
            LikeNewsfeedClicked?.Invoke(sender, e);
        }

        private void Post_ButtonRetweetClicked(object sender, EventArgs e)
        {
            ButtonRetweetClicked?.Invoke(sender, EventArgs.Empty);
        }


        private void Post_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            ButtonShowProfileClicked?.Invoke(sender, EventArgs.Empty);
        }


        private void Post_ButtonAddCommentClicked(object sender, EventArgs e)
        {
            var post = sender as FriendicaPost;
            var thread = GetThreadForPost(post);
            if (thread != null)
                ButtonAddCommentClicked?.Invoke(thread, EventArgs.Empty);
        }

        private FriendicaThread GetThreadForPost(FriendicaPost post)
        {
            if (NetworkThreads == null)
            {
                foreach (var thread in NetworkThreadsContainer)
                {
                    if (thread.MainPost.Contains(post))
                        return thread;
                    if (thread.Comments.Contains(post))
                        return thread;
                }
                return null;
            }
            else
            {
                foreach (var thread in NetworkThreads)
                {
                    if (thread.MainPost.Contains(post))
                        return thread;
                    if (thread.Comments.Contains(post))
                        return thread;
                }
                return null;
            }
        }

        private bool CheckIfAllThreadsAreLoaded()
        {
            bool _isLoaded = true;
            if (NetworkThreads != null)
            {
                foreach (var thread in NetworkThreads)
                    _isLoaded &= thread.IsThreadLoaded;
            }
            if (NetworkThreadsContainer != null)
            {
                foreach (var thread in NetworkThreadsContainer)
                    _isLoaded &= thread.IsThreadLoaded;
            }
            if (NewsfeedThreads != null)
            {
                foreach (var thread in NewsfeedThreads)
                    _isLoaded &= thread.IsThreadLoaded;
            }
            if (NewsfeedThreadsContainer != null)
            {
                foreach (var thread in NewsfeedThreadsContainer)
                    _isLoaded &= thread.IsThreadLoaded;
            }
            return _isLoaded;
        }



        private void SaveSettingMaximumNetworkPostId()
        {
            if (Posts == null || Posts.Count == 0)
                return;

        }


        private async Task UpdateTblAllKnownUsersAsync()
        {
            if (Settings.SaveLocalAllowed)
            {
                foreach (var post in Posts)
                {
                    if (post.Post.PostUser != null)
                    {
                        var result = await StaticSQLiteConn.SelectSingleFromtblAllKnownUsersAsync(post.Post.PostUser.UserUrl);
                        if (result == null || result.Count == 0)
                        {
                            var isSucceeded = await StaticSQLiteConn.InsertIntotblAllKnownUsersAsync(post.Post.PostUser);
                        }
                    }

                    if (post.Post.PostRetweetedStatus != null)
                    {
                        if (post.Post.PostRetweetedStatus.PostUser != null)
                        {
                            var result = await StaticSQLiteConn.SelectSingleFromtblAllKnownUsersAsync(post.Post.PostUser.UserUrl);
                            if (result == null || result.Count == 0)
                            {
                                var isSucceeded = await StaticSQLiteConn.InsertIntotblAllKnownUsersAsync(post.Post.PostUser);
                            }
                        }
                    }
                }
            }
        }


        public void SetNewEntriesToSeen()
        {
            // perform this only on NetworkThreads as Newsfeed shall be set back manually by the user (probably he/she wants to read them later)
            if (NetworkThreads != null)
            {
                foreach (var thread in NetworkThreads)
                {
                    if (thread.MainPost != null && thread.MainPost.Count != 0)
                        thread.MainPost[0].IsNewEntry = false;

                    foreach (var post in thread.Comments)
                        post.IsNewEntry = false;
                    foreach (var post in thread.CommentsDisplay)
                        post.IsNewEntry = false;
                }
            }
            UpdateCounters();
        }




        public void UpdateCounters()
        {
            // set counter on menu item to show the new entries
            int countNewNetwork = 0;
            int countNewNewsfeed = 0;

            if (NetworkThreads == null)
                countNewNetwork = (from th in NetworkThreadsContainer
                                       select new { counter = th.Comments.Count(p => p.IsNewEntry) + th.MainPost.Count(p => p.IsNewEntry) }).Sum(a => a.counter);
            else
                countNewNetwork = (from th in NetworkThreads
                                       select new { counter = th.Comments.Count(p => p.IsNewEntry) + th.MainPost.Count(p => p.IsNewEntry) }).Sum(a => a.counter);
            StaticGlobalParameters.CounterUnseenNetwork = countNewNetwork;

            if (NewsfeedThreads == null)
                countNewNewsfeed = (from th in NewsfeedThreadsContainer
                                    select new { counter = th.Comments.Count(p => p.IsNewEntry) + th.MainPost.Count(p => p.IsNewEntry) }).Sum(a => a.counter);
            else
                countNewNewsfeed = (from th in NewsfeedThreads
                                    select new { counter = th.Comments.Count(p => p.IsNewEntry) + th.MainPost.Count(p => p.IsNewEntry) }).Sum(a => a.counter);
            StaticGlobalParameters.CounterUnseenNewsfeed = countNewNewsfeed;
        }

    }
}
