using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.HttpRequests
{
    class GetFriendicaMessages : clsHttpRequests
    {
        public enum MessageErrors { OK, NoMailsAvailable, MessageIdNotSpecified, MessageSetToSeen, MessageIdOrParentUriNotSpecified, MessageDeleted, MessageIdNotInDatabase, ParentUriNotInDatabase, SearchstringNotSpecified, UnknownError };

        AppSettings appSettings = new AppSettings();

        public string MessageId { get; set; }
        public string ConversationUri { get; set; }
        public List<FriendicaMessage> MessagesReturned { get; set; }
        public List<FriendicaMessage> SearchResults { get; set; }
        public bool NoSearchResultsReturned { get; set; }
        public bool IsErrorOccurred { get; set; }
        public MessageErrors ErrorMessageFriendica { get; set; }

        public event EventHandler FriendicaMessagesLoaded;
        protected virtual void OnFriendicaMessagesLoaded()
        {
            if (FriendicaMessagesLoaded != null)
                FriendicaMessagesLoaded(this, EventArgs.Empty);
        }
        

        public GetFriendicaMessages()
        {
        }


        public async void LoadMessagesInitial(int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&timestamp={2}&friendica_verbose=true",
                appSettings.FriendicaServer,
                count,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public async void LoadMessagesNext(double maxId, int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&max_id={2}&timestamp={3}&friendica_verbose=true",
                appSettings.FriendicaServer,
                count,
                maxId,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public async void LoadMessagesNew(double minId, int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&since_id={2}&timestamp={3}&friendica_verbose=true",
                appSettings.FriendicaServer,
                count,
                minId,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public async void LoadConversation(string uri)
        {
            ConversationUri = uri;
            var url = String.Format("{0}/api/direct_messages/conversation.json?getText=html&count=999&uri={1}&timestamp={2}&friendica_verbose=true",
                appSettings.FriendicaServer,
                uri,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public async void SetSeenMessage(string id)
        {
            MessageId = id;
            var url = String.Format("{0}/api/friendica/direct_messages_setseen.json?id={1}&timestamp={2}",
                appSettings.FriendicaServer,
                id,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public async void DeleteMessage(string id, string parenturi)
        {
            MessageId = id;
            var url = String.Format("{0}/api/direct_messages/destroy.json?id={1}&friendica_parenturi={2}&friendica_verbose=true&timestamp={3}",
                appSettings.FriendicaServer,
                id,
                parenturi,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            await this.DeleteString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public async void SearchMessage(string searchstring)
        {
            var url = String.Format("{0}/api/friendica/direct_messages_search.json?getText=html&searchstring={1}&timestamp={2}",
                appSettings.FriendicaServer,
                searchstring,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public List<FriendicaConversation> RetrieveConversations()
        {
            var parenturi = new List<FriendicaConversation>();
            // gets all distinct conversations from the returned messages
            foreach (var message in MessagesReturned)
            {
                var findResult = parenturi.Find(c => c.ConversationUri == message.MessageParentUri);
                if (findResult == null)
                {
                    var conv = new FriendicaConversation();
                    conv.ConversationUri = message.MessageParentUri;
                    conv.Title = message.MessageTitle;
                    parenturi.Add(conv);
                }
            }
            return parenturi;
        }

        public List<FriendicaConversation> RetrieveSearchConversations()
        {
            var parenturi = new List<FriendicaConversation>();
            // gets all distinct conversations from the returned messages
            foreach (var message in SearchResults)
            {
                var findResult = parenturi.Find(c => c.ConversationUri == message.MessageParentUri);
                if (findResult == null)
                {
                    var conv = new FriendicaConversation();
                    conv.ConversationUri = message.MessageParentUri;
                    conv.Title = message.MessageTitle;
                    parenturi.Add(conv);
                }
            }
            return parenturi;
        }


        private void GetFriendicaMessages_RequestFinished(object sender, EventArgs e)
        {
            if (this.StatusCode == Windows.Web.Http.HttpStatusCode.Ok)
            {
                try
                {
                    var errorResult = new FriendicaErrorResult(this.ReturnString);
                    if (errorResult != null && errorResult.ErrorResult != null)
                    {
                        ErrorMessageFriendica = SetErrorMessage(errorResult.ErrorMessage);
                        if (errorResult.ErrorResult == "error")
                            IsErrorOccurred = true;
                        else if (errorResult.ErrorResult == "ok")
                            IsErrorOccurred = false;
                    }
                    else
                    {
                        // result from search api call 
                        JsonObject jsonObject = JsonObject.Parse(this.ReturnString);
                        try
                        {
                            var arraySearchResults = jsonObject.GetNamedArray("search_results");
                            SearchResults = ConvertJsonToObjects(arraySearchResults);
                            NoSearchResultsReturned = false;
                        }
                        catch
                        {
                            var stringSearchResults = jsonObject.GetNamedString("search_results");
                            if (stringSearchResults == "nothing found")
                                NoSearchResultsReturned = true;
                        }
                    }
                }
                catch
                {
                    MessagesReturned = ConvertJsonToObjects();
                    if (MessagesReturned.Count == 0)
                    {
                        IsErrorOccurred = true;
                        ErrorMessageFriendica = MessageErrors.ParentUriNotInDatabase;
                    }
                }
            }
            else
                IsErrorOccurred = true; 

            OnFriendicaMessagesLoaded();
        }

        private MessageErrors SetErrorMessage(string errorMessage)
        {
            switch (errorMessage)
            {
                case "message deleted":
                    return MessageErrors.MessageDeleted;
                case "message id not in database":
                    return MessageErrors.MessageIdNotInDatabase;
                case "message id not specified":
                    return MessageErrors.MessageIdNotSpecified;
                case "message id or parenturi not specified":
                    return MessageErrors.MessageIdOrParentUriNotSpecified;
                case "message set to seen":
                    return MessageErrors.MessageSetToSeen;
                case "no mails available":
                    return MessageErrors.NoMailsAvailable;
                case "searchstring not specified":
                    return MessageErrors.SearchstringNotSpecified;
                case "unknown error":
                    return MessageErrors.UnknownError;
                default:
                    return MessageErrors.UnknownError; 
            }
        }

        private List<FriendicaMessage> ConvertJsonToObjects(JsonArray array = null)
        {
            JsonArray resultArray;

            if (array != null)
                resultArray = array;
            else
                resultArray = JsonArray.Parse(this.ReturnString);

            var list = new List<FriendicaMessage>();
            int arraySize = resultArray.Count;
            for (int i = 0; i < arraySize; i++)
            {
                IJsonValue element = resultArray.GetArray()[i];
                switch (element.ValueType)
                {
                    case JsonValueType.Object:
                        var result = new FriendicaMessage(element.ToString());
                        list.Add(result);
                        break;
                }
            }
            return list;
        }

    }
}
