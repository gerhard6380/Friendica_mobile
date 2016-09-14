using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BackgroundTasks
{
    class GetFriendicaMessages
    {
        public enum MessageErrors { OK, NoMailsAvailable, MessageIdNotSpecified, MessageSetToSeen, MessageIdOrParentUriNotSpecified, MessageDeleted, MessageIdNotInDatabase, ParentUriNotInDatabase, SearchstringNotSpecified, UnknownError };

        AppSettings appSettings = new AppSettings();
        private string _url;
        private string _username;
        private string _password;

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

        // event showing the finalization of the request
        public event EventHandler RequestFinished;

        protected virtual void OnRequestFinishedChanged()
        {
            if (RequestFinished != null)
                RequestFinished(this, EventArgs.Empty);
        }

        // Output values
        private Windows.Web.Http.HttpStatusCode _statusCode;
        public Windows.Web.Http.HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        private string _returnString;
        public string ReturnString
        {
            get { return _returnString; }
        }

        private bool _isSuccessStatusCode;
        public bool IsSuccessStateCode
        {
            get { return _isSuccessStatusCode; }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        private int _errorHresult;
        public int ErrorHResult
        {
            get { return _errorHresult; }
        }



        public GetFriendicaMessages()
        {
        }


        public void LoadMessagesInitial(int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&timestamp={2}&friendica_verbose=true",
                appSettings.FriendicaServer,
                count,
                DateTime.Now.ToString());
            RequestFinished += GetFriendicaMessages_RequestFinished;
            GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public void LoadMessagesNext(double maxId, int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&max_id={2}&timestamp={3}&friendica_verbose=true",
                appSettings.FriendicaServer,
                count,
                maxId,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public void LoadMessagesNew(double minId, int count)
        {
            var url = String.Format("{0}/api/direct_messages/all.json?getText=html&count={1}&since_id={2}&timestamp={3}&friendica_verbose=true",
                appSettings.FriendicaServer,
                count,
                minId,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public void LoadConversation(string uri)
        {
            ConversationUri = uri;
            var url = String.Format("{0}/api/direct_messages/conversation.json?getText=html&count=999&uri={1}&timestamp={2}&friendica_verbose=true",
                appSettings.FriendicaServer,
                uri,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public void SetSeenMessage(string id)
        {
            MessageId = id;
            var url = String.Format("{0}/api/friendica/direct_messages_setseen.json?id={1}&timestamp={2}",
                appSettings.FriendicaServer,
                id,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public void DeleteMessage(string id, string parenturi)
        {
            MessageId = id;
            var url = String.Format("{0}/api/friendica/direct_messages_delete.json?id={1}&parenturi={2}&timestamp={3}",
                appSettings.FriendicaServer,
                id,
                parenturi,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        public void SearchMessage(string searchstring)
        {
            var url = String.Format("{0}/api/friendica/direct_messages_search.json?getText=html&searchstring={1}&timestamp={2}",
                appSettings.FriendicaServer,
                searchstring,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaMessages_RequestFinished;
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        //public List<FriendicaConversation> RetrieveConversations()
        //{
        //    var parenturi = new List<FriendicaConversation>();
        //    // gets all distinct conversations from the returned messages
        //    foreach (var message in MessagesReturned)
        //    {
        //        var findResult = parenturi.Find(c => c.ConversationUri == message.MessageParentUri);
        //        if (findResult == null)
        //        {
        //            var conv = new FriendicaConversation();
        //            conv.ConversationUri = message.MessageParentUri;
        //            conv.Title = message.MessageTitle;
        //            parenturi.Add(conv);
        //        }
        //    }
        //    return parenturi;
        //}

        //public List<FriendicaConversation> RetrieveSearchConversations()
        //{
        //    var parenturi = new List<FriendicaConversation>();
        //    // gets all distinct conversations from the returned messages
        //    foreach (var message in SearchResults)
        //    {
        //        var findResult = parenturi.Find(c => c.ConversationUri == message.MessageParentUri);
        //        if (findResult == null)
        //        {
        //            var conv = new FriendicaConversation();
        //            conv.ConversationUri = message.MessageParentUri;
        //            conv.Title = message.MessageTitle;
        //            parenturi.Add(conv);
        //        }
        //    }
        //    return parenturi;
        //}


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

        public async void GetString(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;


            var filter = new HttpBaseProtocolFilter();
            filter.ServerCredential = new Windows.Security.Credentials.PasswordCredential(_url, _username, _password);
            filter.AllowUI = false;

            var httpClient = new HttpClient(filter);
            var headers = httpClient.DefaultRequestHeaders;
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var response = new HttpResponseMessage();
            var uri = new Uri(_url);
            try
            {
                response = await httpClient.GetAsync(uri);
                _statusCode = response.StatusCode;
                _returnString = await response.Content.ReadAsStringAsync();
                _isSuccessStatusCode = response.IsSuccessStatusCode;
                OnRequestFinishedChanged();
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                _errorHresult = ex.HResult;
                OnRequestFinishedChanged();
            }
        }


    }
}
