using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using Friendica_Mobile.Strings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace Friendica_Mobile.Viewmodels
{
    public class FriendicaThread : BindableClass
    {
        // Id of conversation starting post to indicate the conversation
        private double _threadId;
        public double ThreadId
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
            set { _mainPost = value;
                OnPropertyChanged("MainPost"); }
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
            set { _showCommentsToggle = value;
                OnPropertyChanged("ShowCommentsToggle"); }
        }

        // bool indicating to show all comments
        private bool _showAllComments;
        public bool ShowAllComments
        {
            get { return _showAllComments; }
            set { _showAllComments = value;
                if (value)
                    ExpandComments();
                else
                    CollapseComments();
            }
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
        public async void LoadThreadData()
        {
            if (ThreadId == 0)
                return;

            var getThread = new GetFriendicaNetwork();
            await getThread.GetFriendicaThreadByIdAsync(ThreadId);

            if (getThread.StatusCode == HttpStatusCode.OK)
            {
                var listComments = new List<FriendicaPost>();
                foreach (var post in getThread.Posts)
                {
                    if (post.IsComment)
                        listComments.Add(post);
                    else
                    {
                        post.IsVisible = true;
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
            else
            {
                IsThreadLoaded = false;
                // we had an error on loading the data, ask user for new try or cancel
                var errorMsg = String.Format(AppResources.messageDialogErrorOnLoadingData, getThread.StatusCode.ToString());
                var result = await StaticMessageDialog.ShowDialogAsync(null, errorMsg, AppResources.buttonYes, AppResources.buttonNo, null, 0, 1);

                if (result == 0)
                    LoadThreadData();
                else
                {
                    return;
                }
            }
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
        }
       
    }
}
