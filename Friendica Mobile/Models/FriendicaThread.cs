using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.PCL.HttpRequests;
using Friendica_Mobile.PCL.Models;
using Friendica_Mobile.PCL.Strings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Xamarin.Forms;

namespace Friendica_Mobile.Models
{
    public class FriendicaThread : BindableClass
    {
        // Id of conversation starting post to indicate the conversation
        private int _threadId;
        public int ThreadId
        {
            get { return _threadId; }
            set { _threadId = value; }
        }

        // indicator for building data store (loop through all id's and load posts)
        private bool _isThreadLoaded;
        public bool IsThreadLoaded
        {
            get { return _isThreadLoaded; }
            set
            {
                _isThreadLoaded = value;
                OnPropertyChanged("IsThreadLoaded");
            }
        }

        // property for keeping the main post
        private ObservableCollection<FriendicaPost> _mainPost;
        public ObservableCollection<FriendicaPost> MainPost
        {
            get { return _mainPost; }
            set { SetProperty(ref _mainPost, value); }
        }

        // keeps all entries from this conversation
        private List<FriendicaPost> _comments;
        public List<FriendicaPost> Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }

        // keeps all comments to be displayed (button modifies this obscoll)
        private ObservableCollection<FriendicaPost> _commentsDisplay;
        public ObservableCollection<FriendicaPost> CommentsDisplay
        {
            get { return _commentsDisplay; }
            set { _commentsDisplay = value; }
        }

        // bool to show the button with "show all" or "collapse"
        private bool _showCommentsToggle;
        public bool ShowCommentsToggle
        {
            get { return _showCommentsToggle; }
            set { SetProperty(ref _showCommentsToggle, value); }
        }

        // bool indicating to show all comments
        private bool _showAllComments;
        public bool ShowAllComments
        {
            get { return _showAllComments; }
            set { SetProperty(ref _showAllComments, value);
                if (value)
                    ExpandComments();
                else
                    CollapseComments();
                CommentsToggleText = (value) ? AppResources.SliderShowMorePostsOn : AppResources.SliderShowMorePostsOff;
            }
        }

        private string _commentsToggleText = AppResources.SliderShowMorePostsOff;
        public string CommentsToggleText
        {
            get { return _commentsToggleText; }
            set { SetProperty(ref _commentsToggleText, value); }
        }


        // eventhandlers
        public event EventHandler ThreadIsLoaded;
        

        // constructor 
        public FriendicaThread()
        {
            MainPost = new ObservableCollection<FriendicaPost>();
            Comments = new List<FriendicaPost>();
            CommentsDisplay = new ObservableCollection<FriendicaPost>();
        }


        // method for loading the thread (id must be inserted into class before)
        public async void LoadThreadDataAsync()
        {
            if (ThreadId == 0)
                return;

            var getThread = new HttpRequests.HttpFriendicaPosts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            var result = await getThread.GetFriendicaThreadByIdAsync(ThreadId);

            switch (result)
            {
                case HttpFriendicaPosts.GetFriendicaPostsResults.SampleMode:
                    // get SampleData
                    WorkOnThreadItems(SampleData.PostsThreadSamples(ThreadId));
                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.OK:
                    var posts = new List<FriendicaPost>();
                    foreach (var post in getThread.Posts)
                        posts.Add(new FriendicaPost(post));
                    WorkOnThreadItems(posts);
                    break;
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAnswered:
                case HttpFriendicaPosts.GetFriendicaPostsResults.NotAuthenticated:
                case HttpFriendicaPosts.GetFriendicaPostsResults.SerializationError:
                case HttpFriendicaPosts.GetFriendicaPostsResults.UnexpectedError:
                default:
                    IsThreadLoaded = false;
                    // we had an error on loading the data, ask user for new try or cancel
                    var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getThread.StatusCode.ToString());
                    var dialogResult = await Application.Current.MainPage.DisplayAlert(result.ToString(), errorMsg, AppResources.buttonYes, AppResources.buttonNo);

                    if (dialogResult)
                        LoadThreadDataAsync();
                    else
                    {
                        return;
                    }
                    break;
            }
        }


        private void WorkOnThreadItems(List<FriendicaPost> posts)
        {
            var listComments = new List<FriendicaPost>();
            foreach (var post in posts)
            {
                if (post.IsComment)
                    listComments.Add(post);
                else
                {
                    post.IsVisible = true;
                    if (MainPost == null)
                        MainPost = new ObservableCollection<FriendicaPost>();
                    MainPost.Add(post);
                }

                // set IsNewEntry on newsfeed when id is containing in the list stored in the settings
                if (Settings.UnseenNewsfeedItems.Contains((int)post.Post.PostId))
                    post.IsNewEntry = true;
            }
            // order comments by date
            Comments = listComments.OrderBy(p => p.CreatedAtDateTime).ToList();

            CollapseComments();

            // Check if button for more or less comments should be displayed
            SetIsVisibleShowAllButton();

            IsThreadLoaded = true;
            ThreadIsLoaded?.Invoke(this, EventArgs.Empty);
        }


        public void CollapseComments()
        {
            if (CommentsDisplay != null)
                CommentsDisplay.Clear();
            else
                CommentsDisplay = new ObservableCollection<FriendicaPost>();
            // show 2 comments plus all new comments
            var countNew = Comments.Count(p => p.IsNewEntry);

            var count = Comments.Count - 2 - countNew;
            foreach (var post in Comments)
            {
                if (Comments.IndexOf(post) >= count)
                {
                    post.IsVisible = true;
                    CommentsDisplay.Add(post);
                }
                else
                    post.IsVisible = false;
            }
        }


        public void ExpandComments()
        {
            CommentsDisplay.Clear();
            foreach (var post in Comments)
            {
                post.IsVisible = true;
                CommentsDisplay.Add(post);
            }
        }


        private void SetIsVisibleShowAllButton()
        {
            var count = Comments.Count(m => m.IsComment);
            if (count > 2)
            {
                ShowCommentsToggle = true;
            }
            else
                ShowCommentsToggle = false;
            ShowAllComments = (Comments.Count == CommentsDisplay.Count);
        }
       
    }
}
