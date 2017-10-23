using Friendica_Mobile.PCL.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.Models
{
    public class NotificationElement
    {
        public enum NotificationType { Post, Newsfeed, Message }
        public FriendicaPost Post;
        public FriendicaMessage Message;
        public double PostId;
        public string ConversationId;
        public string Recipient; // nur bei Message
        public string ImageUrl;
        public string Title; // is showing the name of the sender in the first line
        public string BodyTextXml;
        public DateTime DisplayTimestamp;
        public bool GeneralInfoNewsfeed = false;


        public NotificationElement(NotificationType type, int count)
        {
            // generic creator used for general newsfeed item
            if (type == NotificationType.Newsfeed)
            {
                PostId = 0;
                ConversationId = "";
                ImageUrl = "ms-appx:///BackgroundTasks/Assets/rssfeed.png";
                Title = "RSS feeds";
                DisplayTimestamp = DateTime.Now;
                BodyTextXml = "<text>" + String.Format(AppResources.toastNewsfeedGeneralNotification, count) + "</text>";
            }
        }

        public NotificationElement(NotificationType type, object element)
        {
            if (type == NotificationType.Post || type == NotificationType.Newsfeed)
            {
                if (element.GetType() == typeof(FriendicaPost))
                {
                    Post = (FriendicaPost)element;
                    PostId = Post.Post.PostId;
                    ConversationId = Post.GetThreadId().ToString();
                    ImageUrl = Post.User.User.UserProfileImageUrl;
                    Title = Post.User.User.UserName;
                    DisplayTimestamp = Post.CreatedAtDateTime;
                    CreateXml(type);
                }

            }
            else if (type == NotificationType.Message)
            {
                if (element.GetType() == typeof(FriendicaMessage))
                {
                    Message = (FriendicaMessage)element;
                    PostId = Message.MessageIdInt;
                    ConversationId = Message.MessageParentUri;
                    Recipient = Message.CounterpartUrl;
                    ImageUrl = Message.Sender.User.UserProfileImageUrl;
                    Title = Message.Message.MessageTitle;
                    DisplayTimestamp = Message.MessageCreatedAtDateTime;
                    CreateXml(type);
                }
            }
        }

        private async void CreateXml(NotificationType type)
        {
            if (Post != null && (type == NotificationType.Post || type == NotificationType.Newsfeed))
            {
                BodyTextXml = await StaticHtmlToXml.TransformHtmlToXml(Post.Post.PostStatusnetHtml);
            }
            else if (Message != null && type == NotificationType.Message)
            {
                // prepare text only if user wishes to send the contect in the notification
                if (Settings.NotificationShowMessageContent)
                    BodyTextXml = await StaticHtmlToXml.TransformHtmlToXml(Message.Message.MessageText);
                else
                    BodyTextXml = "<text>" + AppResources.toastMessageAlternativeContent + "</text";
            }
        }
    }
}
