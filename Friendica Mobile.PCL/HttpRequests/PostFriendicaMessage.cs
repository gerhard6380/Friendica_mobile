using Friendica_Mobile.PCL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.HttpRequests
{
    public class PostFriendicaMessage : HttpRequestsBase
    {
        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public FriendicaMessageNew NewMessage;


        public PostFriendicaMessage()
        {
        }


        public async Task PostFriendicaMessageNewAsync(FriendicaMessageNew newMessage)
        {
            NewMessage = newMessage;
            var data = PrepareData();

            var url = String.Format("{0}/api/direct_messages/new",
                Settings.FriendicaServer);
            await this.PostMultipartAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword, data);

        }


        private Dictionary<string, object> PrepareData()
        {
            var Parameters = new Dictionary<string, object>
            {
                { "user_id", NewMessage.NewMessageUserUrl },
                { "text", NewMessage.NewMessageText },
                { "replyto", NewMessage.NewMessageReplyTo },
                { "title", NewMessage.NewMessageTitle }
            };
            return Parameters;
        }

    }
}
