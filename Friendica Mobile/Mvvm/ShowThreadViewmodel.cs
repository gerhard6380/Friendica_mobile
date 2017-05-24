using Friendica_Mobile.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Friendica_Mobile.Mvvm
{
    class ShowThreadViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public Thickness BottomAppBarMargin { get { return App.Settings.BottomAppBarMargin; } }
        public double ListViewWidth { get { return App.Settings.ShellWidth; } }

        public event EventHandler ButtonShowProfileClicked;

        // containing data which will be shown in listview
        private ObservableCollection<FriendicaThread> _showThread;
        public ObservableCollection<FriendicaThread> ShowThread
        {
            get { return _showThread; }
            set { _showThread = value;
                OnPropertyChanged("ShowThread"); }
        }

        private FriendicaPostExtended _postToShow;
        public FriendicaPostExtended PostToShow
        {
            get { return _postToShow; }
            set { _postToShow = value; }
        }

        private FriendicaPostExtended _selectedPostForAction;
        public FriendicaPostExtended SelectedPostForAction
        {
            get { return _selectedPostForAction; }
            set { _selectedPostForAction = value; }
        }


        private string _navigationSourcePage;
        public string NavigationSourcePage
        {
            get { return _navigationSourcePage; }
            set { _navigationSourcePage = value; }
        }

        // show or hide loading indicator and change status of buttons
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value;
                RefreshShowThreadCommand.RaiseCanExecuteChanged();
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoading"); }
        }


        // Indicator for sending new posts 
        private bool _isSendingNewPost;
        public bool IsSendingNewPost
        {
            get { return _isSendingNewPost; }
            set
            {
                _isSendingNewPost = value;
                RefreshShowThreadCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsSendingNewPost");
            }
        }


        // show or hide error message on server problems
        private bool _noDataAvailable;
        public bool NoDataAvailable
        {
            get { return _noDataAvailable; }
            set { _noDataAvailable = value;
                OnPropertyChanged("NoDataAvailable"); }
        }

        // show or hide info text if photo has no comments yet
        private bool _noDataAvailablePhoto;
        public bool NoDataAvailablePhoto
        {
            get { return _noDataAvailablePhoto; }
            set { _noDataAvailablePhoto = value;
                AddNewEntryCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("NoDataAvailablePhoto");
            }
        }

        // internal indicator when photo comments were loaded (no add or refresh button in this case available)
        private bool _photoCommentsLoaded;

        // show or hide button to scroll back to the top
        private bool _showScrollToTop;
        public bool ShowScrollToTop
        {
            get { return _showScrollToTop; }
            set { _showScrollToTop = value;
                OnPropertyChanged("ShowScrollToTop"); }
        }


        // define color of the button for scrolling back to top
        private Brush _scrollToTopColor;
        public Brush ScrollToTopColor
        {
            get { return _scrollToTopColor; }
            set { _scrollToTopColor = value;
                OnPropertyChanged("ScrollToTopColor"); }
        }


        // refresh button
        Mvvm.Command _refreshShowThreadCommand;
        public Mvvm.Command RefreshShowThreadCommand { get { return _refreshShowThreadCommand ?? (_refreshShowThreadCommand = new Mvvm.Command(ExecuteRefresh, CanRefresh)); } }
        private bool CanRefresh()
        {
            // at the moment we will not allow commenting on photos where no comment is yet available (NewPost.xaml needs an id which we do not have at this moment)
            if (_isLoading || IsSendingNewPost || NoDataAvailablePhoto || _photoCommentsLoaded)
                return false;
            else
                return true;
        }

        private void ExecuteRefresh()
        {
            // remove borders indicating new entries and set counter to zero
            SetNewEntriesToSeen();

            // Load data newer than current max from server
            if (App.Settings.FriendicaServer != "" && App.Settings.FriendicaServer != "http://"
                && App.Settings.FriendicaServer != "https://" && App.Settings.FriendicaServer != null)
            {
                ReloadThread();
            }
        }


        // add button - needed to react with enabling/disabling the button (command in ExecuteAddNewEntry() is not needed as click event handler
        // in ShowThread.xaml.cs used because of access to shell for navigation needed
        Mvvm.Command _addNewEntryCommand;
        public Mvvm.Command AddNewEntryCommand { get { return _addNewEntryCommand ?? (_addNewEntryCommand = new Mvvm.Command(ExecuteAddNewEntry, CanAddNewEntry)); } }
        private bool CanAddNewEntry()
        {
            // at the moment we will not allow commenting on photos where no comment is yet available (NewPost.xaml needs an id which we do not have at this moment)
            if (_isLoading || NoDataAvailablePhoto || _photoCommentsLoaded)
                return false;
            else
                return true;
        }

        private void ExecuteAddNewEntry()
        {
        }


        public ShowThreadViewmodel()
        {
            IsSendingNewPost = App.IsSendingNewPost;

            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;
            App.SendingNewPostChanged += App_SendingNewPostChanged;
            ScrollToTopColor = new SolidColorBrush(Colors.White);

            // initialize observable collection
            ShowThread = new ObservableCollection<FriendicaThread>();
        }

        private void App_SendingNewPostChanged(object sender, EventArgs e)
        {
            IsSendingNewPost = App.IsSendingNewPost;
            RefreshShowThreadCommand.RaiseCanExecuteChanged();
            ExecuteRefresh();
        }

        public void SetNewEntriesToSeen()
        {
            // set all border back to zero
            foreach (var thread in ShowThread)
            {
                foreach (var post in thread.Posts)
                {
                    if (post.NewEntryIndicatorBorder.Bottom == 4)
                        post.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                }
                foreach (var post in thread.PostsDisplay)
                {
                    if (post.NewEntryIndicatorBorder.Bottom == 4)
                        post.NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
                }
            }
            ScrollToTopColor = new SolidColorBrush(Colors.White);
        }


        public void InitialLoad()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                // show sample data
                // load data from App.NetworkThreads
                IsLoading = true;
                var threadId = GetThreadId();

                if (NavigationSourcePage == "Network")
                {
                    foreach (var thread in App.NetworkThreads)
                    {
                        if (thread.ThreadId == threadId)
                        {
                            foreach (var post in thread.Posts)
                            {
                                if (!post.IsComment)
                                    post.ToggleShowCommentsState = true;
                            }
                            thread.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                            ShowThread.Add(thread);
                        }
                        thread.ShowAllComments();
                    }
                }
                else if (NavigationSourcePage == "Home")
                {
                    foreach (var thread in App.HomeThreads)
                    {
                        if (thread.ThreadId == threadId)
                        {
                            foreach (var post in thread.Posts)
                            {
                                if (!post.IsComment)
                                    post.ToggleShowCommentsState = true;
                            }
                            thread.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                            ShowThread.Add(thread);
                        }
                        thread.ShowAllComments();
                    }
                }
                else if (NavigationSourcePage == "Photos")
                {
                    // load posts for this thread from App.PhotosVm if possible, we do not need to reload from server again
                    if (App.PhotosVm != null && App.PhotosVm.SelectedPhotoalbum != null && App.PhotosVm.SelectedPhotoalbum.SelectedPhoto != null)
                    {
                        var thread = new FriendicaThread();
                        thread.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                        thread.PostsDisplay = new ObservableCollection<FriendicaPostExtended>(App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.Photo.PhotoComments);
                        // show information if there are no elements (no comments or likes yet posted)
                        if (thread.PostsDisplay.Count == 0)
                            NoDataAvailablePhoto = true;
                        else
                        {
                            NoDataAvailablePhoto = false;
                            _photoCommentsLoaded = true;
                            thread.Posts = thread.PostsDisplay;
                            foreach (var post in thread.PostsDisplay)
                            {
                                post.ButtonShowProfileClicked += Post_ButtonShowProfileClicked;
                                post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                            }
                        }
                        thread.IsLoaded = true;
                        ShowThread.Add(thread);
                    }
                }
            }
            else
            {
                // load data from App.NetworkThreads
                IsLoading = true;

                if (NavigationSourcePage == "Network")
                {
                    var threadId = GetThreadId();
                    foreach (var thread in App.NetworkThreads)
                    {
                        if (thread.ThreadId == threadId)
                        {
                            foreach (var post in thread.Posts)
                            {
                                if (!post.IsComment)
                                    post.ToggleShowCommentsState = true;
                            }
                            thread.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                            ShowThread.Add(thread);
                        }
                        thread.ShowAllComments();
                    }
                }
                else if (NavigationSourcePage == "Home")
                {
                    var threadId = GetThreadId();
                    foreach (var thread in App.HomeThreads)
                    {
                        if (thread.ThreadId == threadId)
                        {
                            foreach (var post in thread.Posts)
                            {
                                if (!post.IsComment)
                                    post.ToggleShowCommentsState = true;
                            }
                            thread.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                            ShowThread.Add(thread);
                        }
                        thread.ShowAllComments();
                    }
                }
                else if (NavigationSourcePage == "Photos")
                {
                    // load posts for this thread from App.PhotosVm if possible, we do not need to reload from server again
                    if (App.PhotosVm != null && App.PhotosVm.SelectedPhotoalbum != null && App.PhotosVm.SelectedPhotoalbum.SelectedPhoto != null)
                    {
                        var thread = new FriendicaThread();
                        thread.ButtonShowProfileClicked += Thread_ButtonShowProfileClicked;
                        thread.PostsDisplay = new ObservableCollection<FriendicaPostExtended>(App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.Photo.PhotoComments);
                        // show information if there are no elements (no comments or likes yet posted)
                        if (thread.PostsDisplay.Count == 0)
                            NoDataAvailablePhoto = true;
                        else
                        {
                            NoDataAvailablePhoto = false;
                            _photoCommentsLoaded = true;
                            thread.Posts = thread.PostsDisplay;
                            foreach (var post in thread.PostsDisplay)
                            {
                                post.ButtonShowProfileClicked += Post_ButtonShowProfileClicked;
                                post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                            }
                        }
                        thread.IsLoaded = true;
                        ShowThread.Add(thread);
                    }
                }
            }

            // in loading page we are always on top, so hide scroll button
            ShowScrollToTop = false;

            IsLoading = false;
        }

        private void Post_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            SelectedPostForAction = sender as FriendicaPostExtended;
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
        }

        private void Thread_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            var thread = sender as FriendicaThread;
            SelectedPostForAction = thread.SelectedItem;
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
        }

        public void ReloadThread()
        {
            IsLoading = true;
            var thread = new FriendicaThread();
            thread.PostsLoaded += Thread_PostsLoaded;
            thread.ThreadId = GetThreadId();
            thread.NewPosts = App.NetworkPosts.Where(m => m.NewEntryIndicatorBorder.Bottom == 4);
            thread.IsLoaded = false;
            thread.LoadThread();
            ShowThread = new ObservableCollection<FriendicaThread>();
            ShowThread.Add(thread);
        }

        private double GetThreadId()
        {
            double id = 0;
            if (PostToShow == null)
                return id;
            if (PostToShow.Post.PostInReplyToStatusId == 0)
                id = PostToShow.Post.PostId;
            else if (PostToShow.Post.PostInReplyToStatusId != 0)
                id = Convert.ToDouble(PostToShow.Post.PostInReplyToStatusIdStr);
            return id;
        }

        private void Thread_PostsLoaded(object sender, EventArgs e)
        {
            var thread = sender as FriendicaThread;
            foreach (var post in thread.Posts)
            {
                if (!post.IsComment)
                    post.ToggleShowCommentsState = true;
            }
            IsLoading = false;
        }


        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        { 
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth")
                OnPropertyChanged("ListViewWidth");
        }

        

    }
}