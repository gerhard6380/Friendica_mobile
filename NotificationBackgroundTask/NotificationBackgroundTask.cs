using Friendica_Mobile.PCL;
using Friendica_Mobile.PCL.Models;
using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class NotificationBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        clsManageBadgeStatus badge = new clsManageBadgeStatus();
        NotificationHelper _notificationHelper = null;
        //clsHtmlToXml htmlToXml = new clsHtmlToXml();

        // indicators for silent audio after first post
        private bool _firstLoopDonePosts = false;
        private bool _firstLoopDoneMessages = false;

        ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // get deferral for async operations
            _deferral = taskInstance.GetDeferral();

            // check if we have defined user settings, otherwise we can cancel the task silently
            if (!CheckSettings.ServerSettingsAvailable())
            {
                _deferral.Complete();
                return;
            }

            // now load the data from the server
            _notificationHelper = new NotificationHelper();
            var errorCode = await _notificationHelper.GetNotificationContentAsync();

            switch (errorCode)
            {
                case NotificationHelper.StatusCode.Ok:
                    // send out toast notifications for private messages
                    if (_notificationHelper.UnseenMessagesFromServer.Count > 0)
                    {
                        foreach (var notification in _notificationHelper.UnseenMessagesFromServer)
                        {
                            //if (notification.Message != null)
                            //    notification.BodyTextXml = await StaticHtmlToXml.TransformHtmlToXml(notification.Message.Message.MessageText);
                            CreateToastsMessages(notification);
                            _firstLoopDoneMessages = true;
                        }
                        var maxId = _notificationHelper.UnseenMessagesFromServer.Max(m => m.PostId);
                        //if (maxId > Settings.LastNotifiedMessage)
                            //Settings.LastNotifiedMessage = maxId;
                    }
                    var count = ToastNotificationManager.History.GetHistory().Count;
                    await badge.SetNewBadgeNumberAsync(count);

                    // send out toast notifications for network posts
                    if (_notificationHelper.UnseenPostsFromServer.Count > 0)
                    {
                        foreach (var notification in _notificationHelper.UnseenPostsFromServer)
                        {
                            //if (notification.Post != null)
                            //    notification.BodyTextXml = await StaticHtmlToXml.TransformHtmlToXml(notification.Post.Post.PostStatusnetHtml);
                            CreateToasts(notification);
                            _firstLoopDonePosts = true;
                        }
                        var maxId = _notificationHelper.UnseenPostsFromServer.Max(m => m.PostId);
                        if (maxId > Settings.LastNotifiedNetworkPost)
                            Settings.LastNotifiedNetworkPost = maxId;
                    }
                    count = ToastNotificationManager.History.GetHistory().Count;
                    await badge.SetNewBadgeNumberAsync(count);

                    // send out toast notifications for newsfeed posts
                    if (_notificationHelper.UnseenNewsfeedsFromServer.Count > 0)
                    {
                        foreach (var notification in _notificationHelper.UnseenNewsfeedsFromServer)
                        {
                            //if (notification.Post != null)
                            //    notification.BodyTextXml = await StaticHtmlToXml.TransformHtmlToXml(notification.Post.Post.PostStatusnetHtml);
                            CreateToasts(notification);
                            _firstLoopDonePosts = true;
                        }
                        var maxId = _notificationHelper.UnseenNewsfeedsFromServer.Max(m => m.PostId);
                        if (maxId > Settings.LastNotifiedNetworkPost)
                            Settings.LastNotifiedNetworkPost = maxId;
                    }
                    count = ToastNotificationManager.History.GetHistory().Count;
                    await badge.SetNewBadgeNumberAsync(count);

                    _deferral.Complete();
                    return;
                case NotificationHelper.StatusCode.NoResponse:
                case NotificationHelper.StatusCode.NoAuthentication:
                case NotificationHelper.StatusCode.OtherError:
                    _deferral.Complete();
                    return;
            }
        }


        private void CreateToasts(NotificationElement notification)
        {
            var xml = new XmlDocument();
            string xmlBaseString = "<toast launch=\"general|{0}|{1}\" " +
                "displayTimestamp=\"{2}\"><visual><binding template=\"ToastGeneric\">"
                + "<image placement =\"appLogoOverride\" src=\"{3}\" hint-crop=\"None\"/>"
                + "<text>{4}</text>"
                + "{5}</binding></visual>";
            // make notifications silent after 1st notification
            if (_firstLoopDonePosts)
                xmlBaseString += "<audio silent=\"true\" />";
            // don't add the buttons if we are only telling a general info on new rss feed elements (we cannot like or answer here)
            if (!notification.GeneralInfoNewsfeed)
            {
                if (notification.Post != null && notification.Post.PostType == Friendica_Mobile.PCL.Viewmodels.PostTypes.UserGenerated)
                {
                    xmlBaseString += "<actions>"
                        + "<action activationType=\"background\" content=\"\" arguments=\"post_like|{0}\" imageUri=\"Assets/like.png\" />"
                        + "<action activationType=\"background\" content=\"\" arguments=\"post_dislike|{0}\" imageUri=\"Assets/dislike.png\" />"
                        + "<action activationType=\"foreground\" content=\"" + loader.GetString("toastPostButtonCaption") + "\" arguments=\"reply|{0}|{1}\" />"
                        + "</actions>";
                }
            }
            xmlBaseString += "</toast>";

            // build final xml for toast submission, catch exceptions and post instead a simple info to user
            var xmlAdapted = String.Format(xmlBaseString, 
                                    notification.PostId, 
                                    notification.ConversationId, 
                                    notification.DisplayTimestamp.ToString("yyyy-MM-ddTHH:MM:ssZ"), 
                                    notification.ImageUrl, 
                                    notification.Title, 
                                    notification.BodyTextXml);
            try
            {
                xml.LoadXml(xmlAdapted);
            }
            catch
            {
                xmlAdapted = String.Format(xmlBaseString,
                                    notification.PostId, 
                                    notification.ConversationId, 
                                    notification.DisplayTimestamp.ToString("yyyy-MM-ddTHH:MM:ssZ"), 
                                    notification.ImageUrl, 
                                    notification.Title, 
                                    "<text>Messagetext with unrecognized content...</text>");
                xml.LoadXml(xmlAdapted);
            }
            // send the toast
            var toast = new ToastNotification(xml)
            {
                Tag = notification.PostId.ToString(),
                Group = "Post"
            };
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }


        private void CreateToastsMessages(NotificationElement notification)
        {
            var xml = new XmlDocument();
            string xmlBaseString = "<toast launch=\"pm_direct|{0}|{1}|{2}\" " +
                "displayTimestamp=\"{3}\"><visual><binding template=\"ToastGeneric\">"
                + "<image placement =\"appLogoOverride\" src=\"{4}\" hint-crop=\"circle\"/>"
                + "<text>{5}</text>"
                + "{6}</binding></visual>";
            // make notifications silent after 1st notification
            if (_firstLoopDoneMessages)
                xmlBaseString += "<audio silent=\"true\" />";
            // add actions only if user has activated the respective Setting
            if (Settings.NotificationShowMessageContent == true)
            {
                xmlBaseString += "<actions>"
                    + "<input id=\"pm_message\" type=\"text\" placeHolderContent=\"" + loader.GetString("toastMessageInputPlaceholder") + "\" />"
                    + "<action activationType=\"background\" content=\"" + loader.GetString("toastMessageInputSendButton") + "\" arguments=\"pm_instantreply|{0}|{1}|{2}\" imageUri=\"Assets/send.png\" hint-inputId=\"pm_message\" />"
                    + "</actions>";
            }
            xmlBaseString += "</toast>";

            // build final xml for toast submission, catch exceptions and post instead a simple info to user
            var xmlAdapted = String.Format(xmlBaseString, 
                                notification.PostId, 
                                notification.ConversationId, 
                                notification.Recipient, 
                                notification.DisplayTimestamp.ToString("yyyy-MM-ddTHH:MM:ssZ"), 
                                notification.ImageUrl, 
                                notification.Title, 
                                notification.BodyTextXml);
            try
            {
                xml.LoadXml(xmlAdapted);
            }
            catch
            {
                xmlAdapted = String.Format(xmlBaseString, 
                                notification.PostId,
                                notification.ConversationId,
                                notification.Recipient,
                                notification.DisplayTimestamp.ToString("yyyy-MM-ddTHH:MM:ssZ"),
                                notification.ImageUrl,
                                notification.Title, 
                                "<Text>Messagetext with unrecognized content...</text>");
                xml.LoadXml(xmlAdapted);
            }
            // send the toast
            var toast = new ToastNotification(xml)
            {
                Tag = notification.PostId.ToString(),
                Group = "Message"
            };
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

    }
}
