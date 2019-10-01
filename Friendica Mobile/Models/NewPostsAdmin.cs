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
    public class NewPostsAdmin : BindableClass
    {
        // collection to add new posts for sending to server (we could have more than one in parallel, probably on sending big images taking some time)
        private ObservableCollection<NewPostModel> _newPosts;
        public ObservableCollection<NewPostModel> NewPosts
        {
            get { return _newPosts; }
            set { SetProperty(ref _newPosts, value); }
        }


        // constructor
        public NewPostsAdmin()
        {
            NewPosts = new ObservableCollection<NewPostModel>();
            NewPosts.CollectionChanged += NewPosts_CollectionChanged;
        }


        private async void NewPosts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // start sending the post to server as soon as added
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var list = e.NewItems;
                var newPost = list[0] as NewPostModel;
                newPost.SendNewPostFinished += NewPost_SendNewPostFinished;
                await newPost.SendNewPost();
            }
        }


        private async void NewPost_SendNewPostFinished(object sender, EventArgs e)
        {
            // we are finished with this current post, remove it from the list
            var newPost = sender as NewPostModel;
            newPost.SendNewPostFinished -= NewPost_SendNewPostFinished;
            NewPosts.Remove(newPost);

            // reload home and network items in background
            if (App.Posts != null)
                await App.Posts.LoadNewPostsAsync();
        }
    }
}
