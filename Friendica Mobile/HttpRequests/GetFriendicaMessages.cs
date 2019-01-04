using Friendica_Mobile.Models;
using Friendica_Mobile.Viewmodels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    public class GetFriendicaMessages : HttpRequestsBase
    {
        // containing the raw returns from server converted into classes
        private List<JsonFriendicaMessage> _messagesRaw;
        // containing the extended classes (commands, events, derived from BindableClass to enable XAML binding)
        public List<FriendicaMessage> Messages;
        // containing the extended classes for Search results
        public List<FriendicaMessage> SearchResults;
        // indicator that search has returned results
        public bool NoSearchResultsReturned { get; set; }
        // indicator if error has occurred
        public bool IsErrorOccurred { get; set; }
        // store error type
        public MessageErrors ErrorMessageFriendica { get; set; }

        public GetFriendicaMessages()
        {
            _messagesRaw = new List<JsonFriendicaMessage>();
            Messages = new List<FriendicaMessage>();
            SearchResults = new List<FriendicaMessage>();
        }

        
        //method to text whether the server understands the API commands (Friendica 3.5 or higher)
        public async Task<bool> CheckServerSupportMessages()
        {
            var url = String.Format("{0}/api/friendica/direct_messages_setseen.json?id={1}&timestamp={2}",
                Settings.FriendicaServer,
                0,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            return (this.StatusCode == System.Net.HttpStatusCode.OK);
        }
        

        // method to retrieve the first bunch of messages
        public async Task LoadMessagesInitialAsync(int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&timestamp={2}&friendica_verbose=true",
                Settings.FriendicaServer,
                count,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            foreach (var message in ConvertReturnString())
                Messages.Add(new FriendicaMessage(message));
            return;
        }


        // method to retrieve the next batch of messages
        public async void LoadMessagesNextAsync(double maxId, int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&max_id={2}&timestamp={3}&friendica_verbose=true",
                Settings.FriendicaServer,
                count,
                maxId,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            foreach (var message in ConvertReturnString())
                Messages.Add(new FriendicaMessage(message));
            return;
        }


        // method to retrieve newly arrived messages from server
        public async Task LoadMessagesNewAsync(double minId, int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&since_id={2}&timestamp={3}&friendica_verbose=true",
                Settings.FriendicaServer,
                count,
                minId,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            var messages = ConvertReturnString();
            if (messages != null)
            {
                foreach (var message in messages)
                    Messages.Add(new FriendicaMessage(message));
            }
            return;
        }


        // load all messages of the specified conversation uri
        public async void LoadConversation(string uri)
        {
            var url = String.Format("{0}/api/direct_messages/conversation.json?getText=html&count=999&uri={1}&timestamp={2}&friendica_verbose=true",
                Settings.FriendicaServer,
                uri,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            foreach (var message in ConvertReturnString())
                Messages.Add(new FriendicaMessage(message));
            return;
        }


        // method to set the seen flag of a message
        public async Task SetSeenMessageAsync(string id)
        {
            var url = String.Format("{0}/api/friendica/direct_messages_setseen.json?id={1}&timestamp={2}",
                Settings.FriendicaServer,
                id,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            foreach (var message in ConvertReturnString())
                Messages.Add(new FriendicaMessage(message));
            return;
        }


        // method to delete a message from the server
        public async void DeleteMessage(string id, string parenturi)
        {
            var url = String.Format("{0}/api/direct_messages/destroy.json?id={1}&friendica_parenturi={2}&friendica_verbose=true&timestamp={3}",
                Settings.FriendicaServer,
                id,
                parenturi,
                DateTime.Now.ToString());
            await this.DeleteStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            foreach (var message in ConvertReturnString())
                Messages.Add(new FriendicaMessage(message));
            return;
        }


        // method to search for messages containing the specified string
        public async void SearchMessage(string searchstring)
        {
            var url = String.Format("{0}/api/friendica/direct_messages_search.json?getText=html&searchstring={1}&timestamp={2}",
                Settings.FriendicaServer,
                searchstring,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            foreach (var message in ConvertReturnString())
                SearchResults.Add(new FriendicaMessage(message));
            return;
        }


        // TODO: the following is not yet converted to the PCL class
        /*
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
        */

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

        /*
        private List<FriendicaMessage> ConvertJsonToObjects(JsonArray array = null)
        {
            JsonArray resultArray;
            bool testArray = false;
            var list = new List<FriendicaMessage>();

            if (array != null)
                resultArray = array;
            else
                testArray = JsonArray.TryParse(this.ReturnString, out resultArray);

            if (testArray)
            {
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
            }
            return list;
        }
        */


        // Convert the returned text into json objects
        private List<JsonFriendicaMessage> ConvertReturnString()
        {
            if (ReturnString != null)
            {
                try
                {
                    var errorResult = JsonConvert.DeserializeObject<JsonFriendicaError>(ReturnString);
                    if (errorResult != null && errorResult.ErrorResult != null)
                    {
                        ErrorMessageFriendica = SetErrorMessage(errorResult.ErrorMessage);
                        if (errorResult.ErrorResult == "error")
                            IsErrorOccurred = true;
                        else if (errorResult.ErrorResult == "ok")
                            IsErrorOccurred = false;
                        return null;
                    }
                    else
                    {
                        // result from search api call 
                        try
                        {
                            return JsonConvert.DeserializeObject<List<JsonFriendicaMessage>>(ReturnString);
                        }
                        catch { return new List<JsonFriendicaMessage>(); }
                    }
                }
                catch
                {
                    // convert the returned string into a list of objects
                    try
                    {
                        return JsonConvert.DeserializeObject<List<JsonFriendicaMessage>>(ReturnString);
                    }
                    catch { return new List<JsonFriendicaMessage>(); }
                }
            }
            else
                return new List<JsonFriendicaMessage>();
        }


    }
}
