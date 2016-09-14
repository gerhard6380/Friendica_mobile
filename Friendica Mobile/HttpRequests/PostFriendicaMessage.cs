using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Friendica_Mobile.HttpRequests
{
    public class PostFriendicaMessage : clsHttpRequests
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public event EventHandler FriendicaMessageSent;
        protected virtual void OnFriendicaMessageSent()
        {
            if (FriendicaMessageSent != null)
                FriendicaMessageSent(this, EventArgs.Empty);
        }

        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public FriendicaMessageNew NewMessage;


        public PostFriendicaMessage()
        {
        }


        public void PostFriendicaMessageNew(FriendicaMessageNew newMessage)
        {
            NewMessage = newMessage;
            var data = PrepareData();

            var url = String.Format("{0}/api/direct_messages/new",
                App.Settings.FriendicaServer);
            this.RequestFinished += PostFriendicaMessage_RequestFinished;
            this.PostMultipart(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword, data);
        }


        private void PostFriendicaMessage_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaMessageSent();
        }


        private Dictionary<string, object> PrepareData()
        {
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("user_id", NewMessage.NewMessageUserUrl);
            Parameters.Add("text", NewMessage.NewMessageText);
            Parameters.Add("replyto", NewMessage.NewMessageReplyTo);
            Parameters.Add("title", NewMessage.NewMessageTitle);
            return Parameters;
        }

    }
}
