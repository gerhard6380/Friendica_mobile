using Friendica_Mobile.HttpRequests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.Web.Http;

namespace Friendica_Mobile.Models
{
    // result of api call //server/api/statuses/show.json?i=xxx&conversation=1
    public class FriendicaThread : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        // keeps all entries from this conversation
        private ObservableCollection<FriendicaPostExtended> _posts;
        public ObservableCollection<FriendicaPostExtended> Posts
        {
            get { return _posts; }
            set { _posts = value; }
        }

        // keeps all entries from this conversation
        private ObservableCollection<FriendicaPostExtended> _postsDisplay;
        public ObservableCollection<FriendicaPostExtended> PostsDisplay
        {
            get { return _postsDisplay; }
            set { _postsDisplay = value; }
        }

        private FriendicaPostExtended _selectedItem;
        public FriendicaPostExtended SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value;
                OnPropertyChanged("SelectedItem"); }
        }


        // Id of conversation starting post to indicate the conversation
        private double _threadId;
        public double ThreadId
        {
            get { return _threadId; }
            set { _threadId = value; }
        }

        // indicator for building data store (loop through all id's and load posts)
        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value;
                OnPropertyChanged("IsLoaded"); }
        }

        public event EventHandler PostsLoaded;
        public event EventHandler ButtonAddCommentClicked;
        public event EventHandler ButtonShowProfileClicked;

        // bool to decide if showing the button with "show all" or "collapse"
        private bool _isVisibleShowAllButton;
        public bool IsVisibleShowAllButton
        {
            get { return _isVisibleShowAllButton; }
            set { _isVisibleShowAllButton = value;
                OnPropertyChanged("IsVisibleShowAllButton"); }
        }

        private IEnumerable<FriendicaPostExtended> _newPosts;
        public IEnumerable<FriendicaPostExtended> NewPosts
        {
            get { return _newPosts; }
            set { _newPosts = value; }
        }


        public FriendicaThread()
        {
        }

        public void LoadThread()
        {
            var getThread = new GetFriendicaThread();
            getThread.FriendicaThreadLoaded += GetThread_FriendicaThreadLoaded;
            getThread.GetFriendicaThreadById(ThreadId);
        }


        private async void GetThread_FriendicaThreadLoaded(object sender, EventArgs e)
        {
            var getThread = sender as GetFriendicaThread;

            if (getThread.StatusCode == HttpStatusCode.Ok)
            {
                // convert result into an observableCollection
                var obscoll = new ObservableCollection<FriendicaPostExtended>();
                try
                {
                    JsonArray resultArray = JsonArray.Parse(getThread.ReturnString);
                    int arraySize = resultArray.Count;
                    for (int i = 0; i < arraySize; i++)
                    {
                        IJsonValue element = resultArray.GetArray()[i];
                        switch (element.ValueType)
                        {
                            case JsonValueType.Object:
                                var result = new FriendicaPostExtended(element.ToString());
                                obscoll.Add(result);
                                break;
                        }
                    }
                }
                catch
                { }
                // insert result into Posts
                Posts = obscoll;
                ReduceComments();

                // Check if button for more or less comments should be displayed
                SetIsVisibleShowAllButton();
                foreach (var post in Posts)
                {
                    post.ButtonShowMorePostsClicked += Post_ButtonShowMorePostsClicked;
                }

                IsLoaded = true;
                if (PostsLoaded != null)
                    PostsLoaded(this, EventArgs.Empty);
                OnPropertyChanged("Posts");
            }
            else
            {
                IsLoaded = false;
                // es gab einen Fehler beim Abrufen der Daten
                // "Fehler beim Laden der Daten. (Fehlermeldung: xxx) \n\n Möchten Sie es erneut versuchen?"
                string errorMsg;
                if (getThread.StatusCode == HttpStatusCode.None)
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), loader.GetString("messageDialogHttpStatusNone"));
                else
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), getThread.StatusCode.ToString());
                var dialog = new MessageDialog(errorMsg);
                //var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                    LoadThread();
                else
                {
                }
            }

        }

        public void LoadSampleData()
        {
            SetIsVisibleShowAllButton();
            foreach (var post in Posts)
            {
                post.ButtonShowMorePostsClicked += Post_ButtonShowMorePostsClicked;
                post.ButtonAddCommentClicked += Post_ButtonAddCommentClicked;
                post.ButtonShowProfileClicked += Post_ButtonShowProfileClicked;
            }
        }

        private void Post_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            this.SelectedItem = sender as FriendicaPostExtended;
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
        }

        private void Post_ButtonAddCommentClicked(object sender, EventArgs e)
        {
            this.SelectedItem = sender as FriendicaPostExtended;
            if (ButtonAddCommentClicked != null)
                ButtonAddCommentClicked(this, EventArgs.Empty);
        }

        private void SetIsVisibleShowAllButton()
        {
            var count = Posts.Count(m => m.IsComment);
            if (count > 2)
            {
                IsVisibleShowAllButton = true;
                var firstPost = Posts.Single(m => m.IsComment == false);
                firstPost.IsPostWithComments = true;
            }
            else
                IsVisibleShowAllButton = false;
        }


        private void Post_ButtonShowMorePostsClicked(object sender, EventArgs e)
        {
            if (Posts.Count == PostsDisplay.Count)
                ReduceComments();
            else
                ShowAllComments();
        }


        public void ShowAllComments()
        {
            PostsDisplay.Clear();
            foreach (var post in Posts)
            {
                post.IsVisible = true;
                post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                PostsDisplay.Add(post);
            }
        }

        
        public void ReduceComments()
        {
            if (PostsDisplay != null)
                PostsDisplay.Clear();
            else
                PostsDisplay = new ObservableCollection<FriendicaPostExtended>();
            // show 2 comments plus initial post plus all new comments
            int countNew = 0;
            foreach (var post in NewPosts)
            {
                double id = 0;
                if (post.Post.PostInReplyToStatusId == 0)
                    id = post.Post.PostId;
                else if (post.Post.PostInReplyToStatusId != 0)
                    id = Convert.ToDouble(post.Post.PostInReplyToStatusIdStr);

                if (id == ThreadId)
                    countNew += 1;
            }
            
            var count = Posts.Count - 3 - countNew;
            foreach (var post in Posts)
            {
                var position = Posts.IndexOf(post);
                if (!post.IsComment)
                {
                    post.IsVisible = true;
                    post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                    PostsDisplay.Add(post);
                }
                else if (post.IsComment && position > count)
                {
                    post.IsVisible = true;
                    post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                    PostsDisplay.Add(post);
                }
                else
                    post.IsVisible = false;
            }
            OnPropertyChanged("PostsDisplay");
        }
    }
}
