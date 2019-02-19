using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;

namespace Friendica_Mobile.UWP.HttpRequests
{
    public class PostFriendicaNewPost : clsHttpRequests
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public event EventHandler FriendicaNewPostSent;
        protected virtual void OnFriendicaNewPostSent()
        {
            if (FriendicaNewPostSent != null)
                FriendicaNewPostSent(this, EventArgs.Empty);
        }

        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public FriendicaNewPost NewPost;

        public PostFriendicaNewPost()
        {
        }

        public void PostFriendicaStatus(FriendicaNewPost newPost)
        {
            // set indicator that app starts to send the post
            App.IsSendingNewPost = true;

            NewPost = newPost;
            var data = PrepareData();

            var url = String.Format("{0}/api/statuses/update",
                App.Settings.FriendicaServer);
            this.RequestFinished += PostFriendicaStatus_RequestFinished;
            this.PostMultipart(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword, data, newPost);
        }

        private async void PostFriendicaStatus_RequestFinished(object sender, EventArgs e)
        {
            App.IsSendingNewPost = false;
            OnFriendicaNewPostSent();

            // if image is too big for server due to upload_max_filesize setting in php.ini; server returns OK but an empty string, this is the only
            // indicator to see this as we cannot retrieve the setting of the server. If server returns an empty string and OK, we inform user about
            // the php.ini setting
            if (ReturnString == "" && StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
            {
                if (NewPost.NewPostMedia != null)
                {
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnSendingNewPostImage"));
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                }
            }
        }

        public async Task<string> PostFriendicaStatusAsync(FriendicaNewPost newPost)
        {
            // set indicator that app starts to send the post
            App.IsSendingNewPost = true;

            NewPost = newPost;
            var data = PrepareData();

            var url = String.Format("{0}/api/statuses/update", App.Settings.FriendicaServer);
            var result = await this.PostMultipartAsync(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword, data);

            App.IsSendingNewPost = false;
            return result;
        }



        private Dictionary<string, object> PrepareData()
        {
            var Parameters = new Dictionary<string, object>();

            Parameters.Add("title", NewPost.NewPostTitle);
            Parameters.Add("status", NewPost.NewPostStatus);
            Parameters.Add("htmlstatus", NewPost.NewPostHtmlStatus);
            Parameters.Add("in_reply_to_status_id", NewPost.NewPostInReplyToStatusId.ToString()); // auf string umstellen, da sonst nicht an Server übertragen
            Parameters.Add("lat", NewPost.NewPostLatitude);
            Parameters.Add("long", NewPost.NewPostLongitude);
            Parameters.Add("media", NewPost.NewPostMedia);
            Parameters.Add("source", NewPost.NewPostSource);
            Parameters.Add("group_allow", NewPost.NewPostGroupAllow);
            Parameters.Add("contact_allow", NewPost.NewPostContactAllow);
            Parameters.Add("group_deny", NewPost.NewPostGroupDeny);
            Parameters.Add("contact_deny", NewPost.NewPostContactDeny);

            return Parameters;
        }

    }
}
