using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using System;
using Windows.ApplicationModel.Resources;


namespace BackgroundTasks
{
    public sealed class NotificationReactionBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        clsManageBadgeStatus badge = new clsManageBadgeStatus();

        private ToastNotificationActionTriggerDetail _action;
        private string[] _parameter;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // get deferral for async operations
            _deferral = taskInstance.GetDeferral();

            // load trigger and supplied parameters and message text
            _action = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (_action != null)
            {
                var argument = _action.Argument;
                _parameter = argument.Split(new char[] { '|' });

                if (_parameter[0] == "pm_instantreply")
                    ReplyMessage();
                else if (_parameter[0] == "post_like")
                    ActivityPost(PostFriendicaActivities.FriendicaActivity.like);
                else if (_parameter[0] == "post_dislike")
                    ActivityPost(PostFriendicaActivities.FriendicaActivity.dislike);
            }
        }

        private void ReplyMessage()
        {
            string messageText = "";
            try { messageText = _action.UserInput["pm_message"].ToString(); } catch { }

            var messageId = _parameter[1];
            var conversation = _parameter[2];
            var recipient = _parameter[3];

            // set seen for the message to which user replied
            var getMessages = new GetFriendicaMessages();
            getMessages.SetSeenMessage(messageId);

            // create new message for sending
            var newMessage = new FriendicaMessageNew();
            newMessage.NewMessageUserUrl = recipient;
            newMessage.NewMessageReplyTo = messageId;
            newMessage.NewMessageText = messageText;

            // sending new message to server
            var postMessage = new PostFriendicaMessage();
            postMessage.FriendicaMessageSent += PostMessage_FriendicaMessageSent;
            postMessage.PostFriendicaMessageNew(newMessage);
        }

        private void PostMessage_FriendicaMessageSent(object sender, EventArgs e)
        {
            var postMessage = sender as PostFriendicaMessage;

            // if server returns an error we will post a toast to user to indicate the error
            if (!postMessage.IsSuccessStateCode)
            {
                var loader = ResourceLoader.GetForViewIndependentUse();
                
                // The getTemplateContent method returns a Windows.Data.Xml.Dom.XmlDocument object
                // that contains the toast notification XML content.
                var template = ToastTemplateType.ToastText02;
                var toastXml = ToastNotificationManager.GetTemplateContent(template);
                var textNodes = toastXml.GetElementsByTagName("text");

                // "Fehler beim Senden der Nachricht"
                textNodes[0].InnerText = loader.GetString("toastErrorPostMessageHeader");
                var baseString = loader.GetString("toastErrorPostMessageText");
                var message = String.Format(baseString, postMessage.NewMessage.NewMessageText, postMessage.NewMessage.NewMessageTitle);
                textNodes[1].InnerText = message;
                // to send the toast.
                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            _deferral.Complete();
        }


        private void ActivityPost(PostFriendicaActivities.FriendicaActivity activity)
        {
            var postId = Convert.ToDouble(_parameter[1]);

            // sending activity to server
            var postActivity = new PostFriendicaActivities();
            postActivity.FriendicaActivitySent += PostActivity_FriendicaActivitySent;
            postActivity.PostFriendicaActivity(postId, activity);
        }

        private void PostActivity_FriendicaActivitySent(object sender, EventArgs e)
        {
            var postActivity = sender as PostFriendicaActivities;

            // if server returns an error we will post a toast to user to indicate the error
            if (!postActivity.IsSuccessStateCode)
            {
                var loader = ResourceLoader.GetForViewIndependentUse();

                // The getTemplateContent method returns a Windows.Data.Xml.Dom.XmlDocument object
                // that contains the toast notification XML content.
                var template = ToastTemplateType.ToastText02;
                var toastXml = ToastNotificationManager.GetTemplateContent(template);
                var textNodes = toastXml.GetElementsByTagName("text");

                // "Fehler beim Senden der Nachricht"
                textNodes[0].InnerText = loader.GetString("toastErrorPostActivityHeader");
                var baseString = loader.GetString("toastErrorPostActivityText");
                var message = String.Format(baseString, postActivity.PostId.ToString());
                textNodes[1].InnerText = message;
                // to send the toast.
                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            _deferral.Complete();
        }
    }
}
