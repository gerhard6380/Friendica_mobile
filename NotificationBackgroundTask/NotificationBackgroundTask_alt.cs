using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using System;
using Windows.Web.Http;
using Windows.Data.Json;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using System.Threading.Tasks;
using Windows.Storage;


namespace BackgroundTasks
{
    public sealed class NotificationBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        AppSettings appSettings = new AppSettings();
        clsManageBadgeStatus badge = new clsManageBadgeStatus();

        //private bool _loadingPostsFinished = false;
        //private bool _loadingMessagesFinished = false;
        private bool _firstLoopDonePosts = false;
        private bool _firstLoopDoneMessages = false;

        // containing data which will be retrieved from Server
        private IList<FriendicaPost> _unseenPostsFromServer;
        public IList<FriendicaPost> UnseenPostsFromServer
        {
            get { return _unseenPostsFromServer; }
            set { _unseenPostsFromServer = value; }
        }

        // containing private messages which will be retrieved from server
        private IList<FriendicaMessage> _unseenMessagesFromServer;

        // event used for detecting when both data retrieves are finished
        //private event EventHandler FinalizeTask;

        ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //this.FinalizeTask += NotificationBackgroundTask_FinalizeTask;

            // get deferral for async operations
            _deferral = taskInstance.GetDeferral();

            // Load new posts (newer than last read one because we want update the badge/lockscreen with the total no. of unread elements
            if (appSettings.FriendicaServer != "" && appSettings.FriendicaServer != "http://"
                            && appSettings.FriendicaServer != "https://" && appSettings.FriendicaServer != null)
            {
                var getNetworkNewEntries = new GetFriendicaNetwork(appSettings);
                getNetworkNewEntries.FriendicaNetworkLoaded += GetNetworkNewEntries_FriendicaNetworkLoaded;
                double newestId = appSettings.LastReadNetworkPost;
                getNetworkNewEntries.GetFriendicaNetworkNewEntries(newestId, 999);
            }
            else
            {
                // update badge
                await Task.Delay(500);
                var count = ToastNotificationManager.History.GetHistory().Count;
                await badge.SetNewBadgeNumberAsync(count);
                _deferral.Complete();

                //_loadingPostsFinished = true;
                //_loadingMessagesFinished = true;
                //if (FinalizeTask != null)
                //    FinalizeTask.Invoke(this, EventArgs.Empty);
            }
        }


        private async void GetNetworkNewEntries_FriendicaNetworkLoaded(object sender, EventArgs e)
        {
            var getNetworkNewEntries = sender as GetFriendicaNetwork;

            if (getNetworkNewEntries.StatusCode == HttpStatusCode.Ok)
            {
                // convert result into a list
                var unseenPosts = ConvertJsonToObsColl(getNetworkNewEntries);
                UnseenPostsFromServer = unseenPosts.OrderBy(p => p.PostId).ToList();

                // loop through unread posts and create toast for each new post (save id of toasted post to appSettings)
                foreach (var post in UnseenPostsFromServer)
                {
                    var lastNotified = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["LastNotifiedNetworkPost"]);
                    if (post.PostId > lastNotified)
                    {
                        await CreateToasts(post);
                        appSettings.LastNotifiedNetworkPost = post.PostId;
                    }
                    _firstLoopDonePosts = true;
                }
            }
            else
            {
                // es gab einen Fehler beim Abrufen der Daten
                // keine Fehlerberichterstattung eingebaut
            }

            // first we need to test if the server has the new api calls 
            var getMessagesTestServer = new GetFriendicaMessages();
            getMessagesTestServer.FriendicaMessagesLoaded += GetMessagesTestServer_FriendicaMessagesLoaded;
            getMessagesTestServer.SetSeenMessage("0");
        }

        private async void GetMessagesTestServer_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesTestServer = sender as GetFriendicaMessages;

            if (getMessagesTestServer.StatusCode == HttpStatusCode.NotImplemented || getMessagesTestServer.StatusCode == HttpStatusCode.NotFound)
            {
                // if api call is not implemented we exit the background task after updating the badge
                await Task.Delay(500);
                var count = ToastNotificationManager.History.GetHistory().Count;
                await badge.SetNewBadgeNumberAsync(count);
                _deferral.Complete();
            }
            else
            {
                // server has the new api calls (Friendica 3.5 and higher)
                var getMessagesNewEntries = new GetFriendicaMessages();
                getMessagesNewEntries.FriendicaMessagesLoaded += GetMessagesNewEntries_FriendicaMessagesLoaded;
                var lastNotifiedMessage = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["LastNotifiedMessage"]);
                if (lastNotifiedMessage == 0)
                    getMessagesNewEntries.LoadMessagesInitial(20);
                else
                    getMessagesNewEntries.LoadMessagesNew(lastNotifiedMessage, 999);
            }
        }

        private async void GetMessagesNewEntries_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesNew = sender as GetFriendicaMessages;

            if (!getMessagesNew.IsErrorOccurred)
            {
                // convert result into a list
                _unseenMessagesFromServer = getMessagesNew.MessagesReturned.Where(m => m.MessageSeen == "0").OrderBy(m => m.MessageIdInt).ToList();

                if (_unseenMessagesFromServer.Count == 0)
                    appSettings.LastNotifiedMessage = getMessagesNew.MessagesReturned.Max(m => m.MessageIdInt);
                else
                {
                    // loop through unread posts and create toast for each new post (save id of toasted post to appSettings)
                    foreach (var message in _unseenMessagesFromServer)
                    {
                        var lastNotifiedMessage = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["LastNotifiedMessage"]);
                        if (message.MessageIdInt > lastNotifiedMessage)
                        {
                            CreateToastsMessages(message);
                            appSettings.LastNotifiedMessage = message.MessageIdInt;
                        }
                        _firstLoopDoneMessages = true;
                    }
                }
            }
            else
            {
                // es gab einen Fehler beim Abrufen der Daten
                // keine Fehlerberichterstattung eingebaut
            }

            await Task.Delay(500);
            var count = ToastNotificationManager.History.GetHistory().Count;
            await badge.SetNewBadgeNumberAsync(count);
            _deferral.Complete();
        }

        //private async void NotificationBackgroundTask_FinalizeTask(object sender, EventArgs e)
        //{
        //    if (_loadingMessagesFinished && _loadingPostsFinished)
        //    {
        //        // update badge
        //        await Task.Delay(500);
        //        var count = ToastNotificationManager.History.GetHistory().Count;
        //        await badge.SetNewBadgeNumberAsync(count);
        //        _deferral.Complete();
        //    }
        //}


        private async Task CreateToasts(FriendicaPost post)
        {
            var xml = new XmlDocument();
            string xmlBaseString = "<toast launch=\"general|{0}|{1}\"><visual><binding template=\"ToastGeneric\">"
                + "<image placement =\"appLogoOverride\" src=\"{2}\" hint-crop=\"circle\"/>"
                + "<text>{3}</text>"
                + "{4}</binding></visual>";
            // make notifications silent after 1st notification
            if (_firstLoopDonePosts)
                xmlBaseString += "<audio silent=\"true\" />";
            xmlBaseString += "<actions>"
                + "<action activationType=\"background\" content=\"\" arguments=\"post_like|{0}\" imageUri=\"Assets/like.png\" />"
                + "<action activationType=\"background\" content=\"\" arguments=\"post_dislike|{0}\" imageUri=\"Assets/dislike.png\" />"
                + "<action activationType=\"foreground\" content=\"" + loader.GetString("toastPostButtonCaption") + "\" arguments=\"reply|{0}|{1}\" />"
                + "</actions></toast>";

            // avoid showing own posts as toast notifications
            var ownUserProfile = appSettings.FriendicaServer + "/profile/" + appSettings.FriendicaUsername;
            if (post.PostUser.UserUrl.ToLower() == ownUserProfile.ToLower())
                return;

            // create timestamp for showing it in the headline of the toast
            DateTime createdAtDate = DateTime.ParseExact(post.PostCreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var timeString = "";
            if (createdAtDate.Date == DateTime.Now.Date) // show date only if post is not of today
                timeString = loader.GetString("stringAtTime") + " " + createdAtDate.ToString("t");
            else
                timeString = loader.GetString("stringOnDate") + " " + createdAtDate.ToString("d") + " " + loader.GetString("stringAtTime") + " " + createdAtDate.ToString("t");
            var userWithDate = "";
            if (post.PostRetweetedStatus != null)
                userWithDate = post.PostRetweetedStatus.PostUser.UserName + "   (" + timeString + ")";
            else
                userWithDate = post.PostUser.UserName + "   (" + timeString + ")";

            // retrieve conversation if available
            string conversation = "0";
            if (post.PostInReplyToStatusIdStr != null)
                conversation = post.PostInReplyToStatusIdStr;

            // take the correct image of the user
            string imageUrl = "";
            if (post.PostRetweetedStatus != null)
                imageUrl = post.PostRetweetedStatus.PostUser.UserProfileImageUrl;
            else
                imageUrl = post.PostUser.UserProfileImageUrl;

            // transform html from body text to xml for toast
            var htmlToXml = new clsHtmlToXml();
            var xmlStringMessage = await htmlToXml.TransformHtmlToXml(post.PostStatusnetHtml);

            // build final xml for toast submission, catch exceptions and post instead a simple info to user
            var xmlAdapted = String.Format(xmlBaseString, post.PostId, conversation, imageUrl, userWithDate, xmlStringMessage);
            try
            {
                xml.LoadXml(xmlAdapted);
            }
            catch
            {
                xmlAdapted = String.Format(xmlBaseString, post.PostId, post.PostUser.UserProfileImageUrl, userWithDate, "<Text>Messagetext with unrecognized content...</text>");
                xml.LoadXml(xmlAdapted);
            }
            // send the toast
            var toast = new ToastNotification(xml);
            toast.Tag = post.PostIdStr;
            toast.Group = "Post";
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }


        private void CreateToastsMessages(FriendicaMessage message)
        {
            var xml = new XmlDocument();
            string xmlBaseString = "<toast launch=\"pm_direct|{0}|{1}|{2}\"><visual><binding template=\"ToastGeneric\">"
                + "<image placement =\"appLogoOverride\" src=\"{3}\" hint-crop=\"circle\"/>"
                + "<text>{4}</text>"
                + "{5}</binding></visual>";
            // make notifications silent after 1st notification
            if (_firstLoopDoneMessages)
                xmlBaseString += "<audio silent=\"true\" />";
            // add actions only if user has activated the respective Setting
            if (appSettings.NotificationShowMessageContent == true)
            {
                xmlBaseString += "<actions>"
                    + "<input id=\"pm_message\" type=\"text\" placeHolderContent=\"" + loader.GetString("toastMessageInputPlaceholder") + "\" />"
                    + "<action activationType=\"background\" content=\"" + loader.GetString("toastMessageInputSendButton") + "\" arguments=\"pm_instantreply|{0}|{1}|{2}\" imageUri=\"Assets/send.png\" hint-inputId=\"pm_message\" />"
                    + "</actions>";
            }
            xmlBaseString += "</toast>";

            // avoid showing own posts as toast notifications
            var ownUserProfile = appSettings.FriendicaServer + "/profile/" + appSettings.FriendicaUsername;
            if (message.MessageSender.UserUrl.ToLower() == ownUserProfile.ToLower())
                return;

            // create timestamp for showing it in the headline of the toast
            DateTime createdAtDate = DateTime.ParseExact(message.MessageCreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
            var timeString = "";
            if (createdAtDate.Date == DateTime.Now.Date) // show date only if post is not of today
                timeString = loader.GetString("stringAtTime") + " " + createdAtDate.ToString("t");
            else
                timeString = loader.GetString("stringOnDate") + " " + createdAtDate.ToString("d") + " " + loader.GetString("stringAtTime") + " " + createdAtDate.ToString("t");
            var userWithDate = "";
            userWithDate = loader.GetString("toastMessageStartOfMessage") + " " + message.MessageTitle + "   (" + message.MessageSender.UserName + " " + timeString + ")";

            // take the correct image of the user
            string imageUrl = "";
            imageUrl = message.MessageSender.UserProfileImageUrl;

            // prepare text only if user wishes to send the content in the notification
            string xmlStringMessage;
            if (appSettings.NotificationShowMessageContent)
            {
                xmlStringMessage = message.MessageTextNotification;
            }
            else
                xmlStringMessage = "<text>" + loader.GetString("toastMessageAlternativeContent") + " </text>";

            string recipient = "";
            if (message.MessageSenderScreenName.ToLower() == appSettings.FriendicaUsername.ToLower())
                recipient = message.MessageRecipient.UserUrl;
            else if (message.MessageRecipientScreenName.ToLower() == appSettings.FriendicaUsername.ToLower())
                recipient = message.MessageSender.UserUrl;

            string conversation = message.MessageParentUri;
            // build final xml for toast submission, catch exceptions and post instead a simple info to user
            var xmlAdapted = String.Format(xmlBaseString, message.MessageId, conversation, recipient, imageUrl, userWithDate, xmlStringMessage);
            try
            {
                xml.LoadXml(xmlAdapted);
            }
            catch
            {
                xmlAdapted = String.Format(xmlBaseString, message.MessageId, conversation, recipient, message.MessageSender.UserProfileImageUrl, userWithDate, "<Text>Messagetext with unrecognized content...</text>");
                xml.LoadXml(xmlAdapted);
            }
            // send the toast
            var toast = new ToastNotification(xml);
            toast.Tag = message.MessageId;
            toast.Group = "Message";
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }


        private IList<FriendicaPost> ConvertJsonToObsColl(GetFriendicaNetwork httpResult)
        {
            var obscoll = new List<FriendicaPost>();
            JsonArray resultArray = JsonArray.Parse(httpResult.ReturnString);
            int arraySize = resultArray.Count;
            for (int i = 0; i < arraySize; i++)
            {
                IJsonValue element = resultArray.GetArray()[i];
                switch (element.ValueType)
                {
                    case JsonValueType.Object:
                        var result = new FriendicaPost(element.ToString());
                        obscoll.Add(result);
                        break;
                }
            }
            return obscoll;
        }


    }
}
