using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using System;
using Windows.ApplicationModel.Resources;
using Friendica_Mobile.Models;
using Friendica_Mobile.HttpRequests;
using System.Threading.Tasks;
using Friendica_Mobile.Viewmodels;

namespace BackgroundTasks
{
    public sealed class NotificationReactionBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        clsManageBadgeStatus badge = new clsManageBadgeStatus();

        private ToastNotificationActionTriggerDetail _action;
        private string[] _parameter;

        public async void Run(IBackgroundTaskInstance taskInstance)
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
                    await ReplyMessageAsync();
                else if (_parameter[0] == "post_like")
                    ActivityPost(FriendicaActivity.like);
                else if (_parameter[0] == "post_dislike")
                    ActivityPost(FriendicaActivity.dislike);
            }
        }

        private async Task ReplyMessageAsync()
        {
            string messageText = "";
            try { messageText = _action.UserInput["pm_message"].ToString(); } catch { }

            var messageId = _parameter[1];
            var conversation = _parameter[2];
            var recipient = _parameter[3];

            // set seen for the message to which user replied
            var getMessages = new GetFriendicaMessages();
            await getMessages.SetSeenMessageAsync(messageId);

            // create new message for sending
            var newMessage = new FriendicaMessageNew()
            {
                NewMessageUserUrl = recipient,
                NewMessageReplyTo = messageId,
                NewMessageText = messageText
            };

            // sending new message to server
            var postMessage = new PostFriendicaMessage();
            await postMessage.PostFriendicaMessageNewAsync(newMessage);

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


        private async void ActivityPost(FriendicaActivity activity)
        {
            var postId = Convert.ToDouble(_parameter[1]);

            // sending activity to server
            var postActivity = new PostFriendicaActivities();
            await postActivity.PostFriendicaActivityAsync(postId, activity);

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
