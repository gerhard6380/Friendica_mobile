using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Strings;
using Xamarin.Forms;
using static Friendica_Mobile.Models.FriendicaPost;

namespace Friendica_Mobile.Models
{
    public class PostsModel : BindableClass
    {
        // indicator that first posts have been loaded, now different API call used for next entries
        private bool _initialLoadDone;
        public bool IsLoadingInitial { get; set; }
        public bool IsRefreshing { get; set; }

        // indicator that we are loading the next portion of posts, avoids double calling before running call is finished
        private bool _isLoadingNext;
        public bool IsLoadingNext
        {
            get { return _isLoadingNext; }
            set { SetProperty(ref _isLoadingNext, value); }
        }

        // indicator that we have no settings defined as display sample data instead
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set { SetProperty(ref _noSettings, value); }
        }

        // indicator that server has not returned data for user's timeline
        private bool _noDataAvailableHome;
        public bool NoDataAvailableHome
        {
            get { return _noDataAvailableHome; }
            set { SetProperty(ref _noDataAvailableHome, value); }
        }

        // indicator that server has not returned data for network
        private bool _noDataAvailableNetwork;
        public bool NoDataAvailableNetwork
        { 
            get { return _noDataAvailableNetwork; }
            set { SetProperty(ref _noDataAvailableNetwork, value); }
        }

        // indicator that server has not returned data for newsfeed
        private bool _noDataAvailableNewsfeed;
        public bool NoDataAvailableNewsfeed
        {
            get { return _noDataAvailableNewsfeed; }
            set { SetProperty(ref _noDataAvailableNewsfeed, value); }
        }

        // list of returned posts (used for selecting min/max, search etc.)
        // extended class incl. commands, events, etc. (JsonFriendicaPost used only in HttpFriendicaPost
        private List<FriendicaPost> _homePosts { get; set; }
        private List<FriendicaPost> _networkPosts { get; set; }

        // containing the threads for displaying in the listviews
        private ObservableCollection<FriendicaThread> _homeThreads;
        public ObservableCollection<FriendicaThread> HomeThreads
        {
            get { return _homeThreads; }
            set { _homeThreads = value; }
        }
        private ObservableCollection<FriendicaThread> _networkThreads;
        public ObservableCollection<FriendicaThread> NetworkThreads
        {
            get { return _networkThreads; }
            set { _networkThreads = value; }
        }
        private ObservableCollection<FriendicaThread> _newsfeedThreads;
        public ObservableCollection<FriendicaThread> NewsfeedThreads
        {
            get { return _newsfeedThreads; }
            set { _newsfeedThreads = value; }
        }

        // counter of unread elements in Home
        private int _counterUnseenHome;
        public int CounterUnseenHome
        {
            get { return _counterUnseenHome; }
            set { SetProperty(ref _counterUnseenHome, value);
                MessagingCenter.Send<PostsModel>(this, "CounterUpdated");
            }
        }

        // counter of unread elements in Network
        private int _counterUnseenNetwork;
        public int CounterUnseenNetwork
        {
            get { return _counterUnseenNetwork; }
            set { SetProperty(ref _counterUnseenNetwork, value);
                MessagingCenter.Send<PostsModel>(this, "CounterUpdated");
            }
        }

        // counter of unread elements in Newsfeed
        private int _counterUnseenNewsfeed;
        public int CounterUnseenNewsfeed
        {
            get { return _counterUnseenNewsfeed; }
            set { SetProperty(ref _counterUnseenNewsfeed, value);
                MessagingCenter.Send<PostsModel>(this, "CounterUpdated");
            }
        }

        // indicator to avoid showing hint on loading next in demo mode more than one
        public bool NoSettingsHintAlreadyShown { get; set; }

        // enum 
        public enum ThreadType { Home, Network, Newsfeed }


        // constructor - initializing the lists
        public PostsModel()
        {
            NoSettings = (!Settings.IsFriendicaLoginDefined());
            _homePosts = new List<FriendicaPost>();
            _networkPosts = new List<FriendicaPost>();
            HomeThreads = new ObservableCollection<FriendicaThread>();
            NetworkThreads = new ObservableCollection<FriendicaThread>();
            NewsfeedThreads = new ObservableCollection<FriendicaThread>();
        }


        public async Task LoadNewPostsAsync()
        {
            IsLoadingInitial = true;
            if (!_initialLoadDone)
            {
                await LoadInitialHomeAsync();
                await LoadInitialNetworkAsync();
                _initialLoadDone = true;
            }
            else
            {
                await LoadNewHomeAsync();
                await LoadNewNetworkAsync();
            }
            IsLoadingInitial = false;
        }


        private async Task LoadInitialHomeAsync()
        {

            // TODO: load first 20 entries for home
            // TODO: working on the entries, load threads where needed
            // TODO: demo mode = sample data


        }

        private async Task LoadInitialNetworkAsync()
        {
            var getData = new HttpFriendicaPosts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            var result = await getData.GetNetworkInitialAsync();

            switch (result)
            {
                case HttpFriendicaPosts.GetFriendicaPostsResults.SampleMode:
                    // get SampleData
                    foreach (var post in SampleData.PostsNetworkSamples())
                    {
                        _networkPosts.Add(post);
                        var type = (post.PostType == PostTypes.UserGenerated) ? ThreadType.Network : ThreadType.Newsfeed;
                        WorkOnEachPost(post, type);
                    }
                    UpdateCounters();

                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.OK:
                    if (getData.Posts == null || getData.Posts.Count == 0)
                        NoDataAvailableNetwork = NoDataAvailableNewsfeed = true;
                    else
                    {
                        foreach (var post in getData.Posts)
                        {
                            var extendedPost = new FriendicaPost(post);
                            _networkPosts.Add(extendedPost);
                            var type = (extendedPost.PostType == PostTypes.UserGenerated) ? ThreadType.Network : ThreadType.Newsfeed;
                            WorkOnEachPost(extendedPost, type);
                        }
                        UpdateCounters();

                        // load next elements if needed
                        if (ShallWeContinue())
                            await LoadNextNetworkAsync();
                    }
                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAnswered:
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAuthenticated:
                case HttpFriendicaPosts.GetFriendicaPostsResults.SerializationError:
                case HttpFriendicaPosts.GetFriendicaPostsResults.UnexpectedError:
                default:
                    var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getData.StatusCode.ToString());
                    var answer = await Application.Current.MainPage.DisplayAlert("", errorMsg, AppResources.buttonYes, AppResources.buttonNo);
                    if (answer)
                        await LoadInitialNetworkAsync();
                    else
                    {
                        if (NetworkThreads.Count == 0)
                            NoDataAvailableNetwork = true;
                        if (NewsfeedThreads.Count == 0)
                            NoDataAvailableNewsfeed = true;
                    }
                    break;
            }
        }

        private async Task LoadNewHomeAsync()
        {


        }

        private async Task LoadNewNetworkAsync()
        {
            IsRefreshing = true;

            // hint to user on NoSetings and then abort
            if (NoSettings)
            {
                await Task.Delay(3000);
                IsRefreshing = false;
                return;
            }

            // get highest displayed id
            double newestId = 0;
            if (_networkPosts.Count != 0)
                newestId = _networkPosts.Max(m => m.Post.PostId);
                
            var getData = new HttpFriendicaPosts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            var result = await getData.GetNetworkNewAsync(newestId);

            switch (result)
            {
                case HttpFriendicaPosts.GetFriendicaPostsResults.SampleMode:
                    // get SampleData - should not be reached
                    foreach (var post in SampleData.PostsNetworkSamples())
                    {
                        _networkPosts.Add(post);
                        var type = (post.PostType == PostTypes.UserGenerated) ? ThreadType.Network : ThreadType.Newsfeed;
                        WorkOnEachPost(post, type);
                    }
                    UpdateCounters();
                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.OK:
                    if (getData.Posts == null || getData.Posts.Count == 0)
                    {
                        NoDataAvailableNetwork = (NetworkThreads.Count == 0);
                        NoDataAvailableNewsfeed = (NewsfeedThreads.Count == 0);
                    }
                    else
                    {
                        var posts = new List<FriendicaPost>();
                        foreach (var post in getData.Posts)
                        {
                            var extendedPost = new FriendicaPost(post);
                            posts.Add(extendedPost);
                        }

                        // order by date to add them always on top of the list (oldest first, newest last)
                        foreach (var extPost in posts.OrderBy(p => p.CreatedAtDateTime))
                        {
                            _networkPosts.Add(extPost);
                            var type = (extPost.PostType == PostTypes.UserGenerated) ? ThreadType.Network : ThreadType.Newsfeed;
                            WorkOnEachPost(extPost, type, true);
                        }
                        UpdateCounters();
                    }
                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAnswered:
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAuthenticated:
                case HttpFriendicaPosts.GetFriendicaPostsResults.SerializationError:
                case HttpFriendicaPosts.GetFriendicaPostsResults.UnexpectedError:
                default:
                    var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getData.StatusCode.ToString());
                    var answer = await Application.Current.MainPage.DisplayAlert("", errorMsg, AppResources.buttonYes, AppResources.buttonNo);
                    if (answer)
                        await LoadInitialNetworkAsync();
                    else
                    {
                        if (NetworkThreads.Count == 0)
                            NoDataAvailableNetwork = true;
                        if (NewsfeedThreads.Count == 0)
                            NoDataAvailableNewsfeed = true;
                    }
                    break;
            }
            IsRefreshing = false;
        }

        public async Task LoadNextHomeAsync()
        {
            // TODO: perform load of next entries for home
            // TODO: work on the entries, load threads where needed
            // TODO: demo mode
        }

        public async Task LoadNextNetworkAsync()
        {
            if (IsLoadingNext)
                return;

            IsLoadingNext = true;

            // hint to user on NoSetings and then abort
            if (NoSettings)
            {
                if (!NoSettingsHintAlreadyShown)
                {
                    await Application.Current.MainPage.DisplayAlert("", AppResources.messageDialogNetworkNoSettings, "OK");
                    NoSettingsHintAlreadyShown = true;
                }
                IsLoadingNext = false;
                return;
            }

            // reduce minimum id by 1 to avoid retrieving the oldest post again
            var oldestId = _networkPosts.Min(m => m.Post.PostId) - 1;
            // oldestId my not be negative or zero, otherwise API returns the newest posts again
            if (oldestId <= 0)
                return;

            var getData = new HttpFriendicaPosts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            var result = await getData.GetNetworkNextAsync(oldestId);

            switch (result)
            {
                case HttpFriendicaPosts.GetFriendicaPostsResults.SampleMode:
                    // get SampleData
                    foreach (var post in SampleData.PostsNetworkSamples())
                    {
                        _networkPosts.Add(post);
                        var type = (post.PostType == PostTypes.UserGenerated) ? ThreadType.Network : ThreadType.Newsfeed;
                        WorkOnEachPost(post, type);
                    }
                    UpdateCounters();
                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.OK:
                    if (getData.Posts == null || getData.Posts.Count == 0)
                        NoDataAvailableNetwork = NoDataAvailableNewsfeed = true;
                    else
                    {
                        foreach (var post in getData.Posts)
                        {
                            var extendedPost = new FriendicaPost(post);
                            _networkPosts.Add(extendedPost);
                            var type = (extendedPost.PostType == PostTypes.UserGenerated) ? ThreadType.Network : ThreadType.Newsfeed;
                            WorkOnEachPost(extendedPost, type);
                        }
                        UpdateCounters();

                        // load next elements if needed
                        if (ShallWeContinue())
                            await LoadNextNetworkAsync();
                    }
                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAnswered:
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAuthenticated:
                case HttpFriendicaPosts.GetFriendicaPostsResults.SerializationError:
                case HttpFriendicaPosts.GetFriendicaPostsResults.UnexpectedError:
                default:
                    var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getData.StatusCode.ToString());
                    var answer = await Application.Current.MainPage.DisplayAlert("", errorMsg, AppResources.buttonYes, AppResources.buttonNo);
                    if (answer)
                        await LoadInitialNetworkAsync();
                    else
                    {
                        if (NetworkThreads.Count == 0)
                            NoDataAvailableNetwork = true;
                        if (NewsfeedThreads.Count == 0)
                           NoDataAvailableNewsfeed = true;
                    }
                    break;
            }
            IsLoadingNext = false;
        }

        /// <summary>
        /// Set all newsfeed posts to seen. Go through all newsfeed items and clear settings with the id's
        /// </summary>
        public void ExecuteSetAllSeen(ThreadType type)
        {
            ObservableCollection<FriendicaThread> threads = null;
            switch (type)
            {
                case ThreadType.Home:
                    threads = NewsfeedThreads;
                    break;
                case ThreadType.Network:
                    threads = NetworkThreads;
                    break;
                case ThreadType.Newsfeed:
                    threads = NewsfeedThreads;
                    break;
            }
            // set all items to seen
            if (threads != null)
            {
                foreach (var thread in threads)
                {
                    foreach (var post in thread.MainPost)
                        post.IsNewEntry = false;
                    foreach (var post in thread.Comments)
                        post.IsNewEntry = false;
                    foreach (var post in thread.CommentsDisplay)
                        post.IsNewEntry = false;
                }
            }

            // clear counter badges
            UpdateCounters();

            // update setting with id's of unseen posts to empty list
            if (type == ThreadType.Newsfeed)
                Settings.UnseenNewsfeedItems = new List<int>();

            // clear live tiles
            // TODO: implement in UWP to clear live tiles 
        }

        private void WorkOnEachPost(FriendicaPost post, ThreadType type, bool isRefresh = false)
        {
            // add item to Settings.UnseenNewsfeedItems if needed
            UpdateUnseenNewsfeedItems(post);

            // get thread id
            var id = post.GetThreadId();

            // check if we do have the id already in our lists
            var thread = HomeThreads.SingleOrDefault(th => th.ThreadId == id) 
                                    ?? NetworkThreads.SingleOrDefault(th => th.ThreadId == id) 
                                    ?? NewsfeedThreads.SingleOrDefault(th => th.ThreadId == id);

            if (thread == null)
            {
                ObservableCollection<FriendicaThread> threads = new ObservableCollection<FriendicaThread>();
                switch (type)
                {
                    case ThreadType.Home:
                        threads = HomeThreads;
                        break;
                    case ThreadType.Network:
                        threads = NetworkThreads;
                        break;
                    case ThreadType.Newsfeed:
                        threads = NewsfeedThreads;
                        break;
                };

                // not yet available in thread lists, let's create a new thread
                var threadNew = new FriendicaThread() { ThreadId = id };
                threadNew.ThreadIsLoaded += ThreadNew_ThreadIsLoaded;
                threadNew.LoadThreadDataAsync();
                if (isRefresh)
                    threads.Insert(0, threadNew);
                else
                    threads.Add(threadNew);
            }
            else
            {
                if (isRefresh)
                {
                    //thread already in lists, so we will update it in refresh mode
                    thread.ThreadIsLoaded += ThreadNew_ThreadIsLoaded;
                    thread.LoadThreadDataAsync();
                }
            }
        }

        private void UpdateCounters()
        {
            CounterUnseenHome = (from th in HomeThreads
                                    select new
                                    {
                                        counter = th.Comments.Count(p => p.IsNewEntry)
                                        + th.MainPost.Count(p => p.IsNewEntry)
                                    }).Sum(a => a.counter);
            CounterUnseenNetwork = (from th in NetworkThreads
                                    select new
                                    {
                                        counter = th.Comments.Count(p => p.IsNewEntry)
                                        + th.MainPost.Count(p => p.IsNewEntry)
                                    }).Sum(a => a.counter);
            CounterUnseenNewsfeed = (from th in NewsfeedThreads
                                     select new
                                     {
                                         counter = th.Comments.Count(p => p.IsNewEntry)
                                         + th.MainPost.Count(p => p.IsNewEntry)
                                     }).Sum(a => a.counter);
        }

        /// <summary>
        /// Checks if we want to automatically load the next portion of posts as we haven't reached the last read post or 
        /// do not yet show a network posts
        /// </summary>
        /// <returns><c>true</c> if there was no post <c>false</c> if there was a post and we can finalize</returns>
        private bool ShallWeContinue()
        {
            var maxId = _networkPosts.Max(p => p.Post.PostId);
            var minId = _networkPosts.Min(p => p.Post.PostId);

            // check for network posts, currently retrieved posts are higher than lastly retrieved ones
            // on new installations, saved id is still zero
            // stop if we reach 99 elements
            if (maxId > Settings.LastReadNetworkPost
                && minId > Settings.LastReadNetworkPost
                && Settings.LastReadNetworkPost != 0
                && CounterUnseenNetwork < 99)
                return true;

            // check for newsfeed posts, stop if 99 elements is reached
            if (Settings.UnseenNewsfeedItems.Count > 0
                && minId > Settings.UnseenNewsfeedItems.Min()
                && CounterUnseenNewsfeed < 99)
                return true;

            // if no network post shown, but newsfeed items are retrieved we will continue until we have at least one network post
            if (NetworkThreads != null && NetworkThreads.Count == 0 && _networkPosts.Count != 0)
                return true;

            return false;
        }

        async void ThreadNew_ThreadIsLoaded(object sender, EventArgs e)
        {
            // TODO: implement code after loading
            // update counters
            UpdateCounters();

            // check if we are now done with all threads, if yes let's do some work to clean up
            var allLoaded = CheckIfAllThreadsAreLoaded();
            if (!allLoaded)
                return;

            // set LastReadNetworkPost in settings
            var maxId = _networkPosts.Max(p => p.Post.PostId);
            if (maxId > Settings.LastReadNetworkPost)
                Settings.LastReadNetworkPost = (int)maxId;

            // set this after all threads have been loaded
            IsLoadingNext = false;
            IsRefreshing = false;
        }

        private bool CheckIfAllThreadsAreLoaded()
        {
            bool _isLoaded = true;
            if (NetworkThreads != null)
            {
                foreach (var thread in NetworkThreads)
                    _isLoaded &= thread.IsThreadLoaded;
            }
            if (NewsfeedThreads != null)
            {
                foreach (var thread in NewsfeedThreads)
                    _isLoaded &= thread.IsThreadLoaded;
            }
            return _isLoaded;
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

    }
}
