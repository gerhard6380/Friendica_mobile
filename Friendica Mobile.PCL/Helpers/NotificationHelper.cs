using Friendica_Mobile.PCL.HttpRequests;
using Friendica_Mobile.PCL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL
{
    // this class is used to retrieve the new posts and messages platform-independent 
    // and prepare them for creating the notifications
    public class NotificationHelper
    {
        public enum StatusCode { Ok, NoAuthentication, NoResponse, OtherError }
        public List<NotificationElement> UnseenPostsFromServer = new List<NotificationElement>();
        public List<NotificationElement> UnseenNewsfeedsFromServer = new List<NotificationElement>();
        public List<NotificationElement> UnseenMessagesFromServer = new List<NotificationElement>();
        private string _authenticatedUser = Settings.FriendicaServer + "/profile/" + Settings.FriendicaUsername;
        private GetFriendicaMessages _getMessages;

        public async Task<StatusCode> GetNotificationContentAsync()
        {
            // loading new posts/newsfeed entries from the network page and prepare the result for the notifications
            var status = await LoadNetworkContentAsync();

            // now let's check if the server supports the new API calls for private messages (Friendica 3.5 or higher)
            _getMessages = new GetFriendicaMessages();
            var messagesSupported = await _getMessages.CheckServerSupportMessages();

            if (messagesSupported)
            {
                // so we can load new private messages from the server and prepare them for the notifications
                status = await LoadMessagesContentAsync();
            }

            return status;
        }

        private async Task<StatusCode> LoadNetworkContentAsync()
        {
            var getNetworkNewEntries = new GetFriendicaNetwork();
            var newestId = Settings.LastReadNetworkPost;
            if (newestId == 0)
                await getNetworkNewEntries.GetNetworkInitialAsync(20);
            else
            {
                await getNetworkNewEntries.GetNetworkNewAsync(newestId, 999);
            }

            switch (getNetworkNewEntries.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    // split by type, keep only if not from user and not already notified
                    var networkPosts = getNetworkNewEntries.Posts.Where(p => p.PostType == Viewmodels.PostTypes.UserGenerated
                                                        && !p.User.IsAuthenticatedUser 
                                                        && p.Post.PostId > Settings.LastNotifiedNetworkPost).ToList();

                    var newsfeedPosts = getNetworkNewEntries.Posts.Where(p => p.PostType == Viewmodels.PostTypes.Newsfeed
                                                        && !p.User.IsAuthenticatedUser 
                                                        && p.Post.PostId > Settings.LastNotifiedNetworkPost).ToList();

                    foreach (var post in networkPosts.OrderByDescending(p => p.CreatedAtDateTime))
                    {
                        var notification = new NotificationElement(NotificationElement.NotificationType.Post, post);
                        if (notification.Post.User.User.UserUrl.ToLower() == _authenticatedUser.ToLower())
                            continue;
                        UnseenPostsFromServer.Add(notification);
                    }
                    
                    if (Settings.NotificationEachNewsfeedAllowed)
                    {
                        foreach (var post in newsfeedPosts.OrderByDescending(p => p.CreatedAtDateTime))
                        {
                            var notification = new NotificationElement(NotificationElement.NotificationType.Newsfeed, post);
                            if (notification.Post.User.User.UserUrl.ToLower() == _authenticatedUser.ToLower())
                                continue;
                            UnseenNewsfeedsFromServer.Add(notification);
                        }
                    }
                    else
                    {
                        // user wants only 1 general notification for all newsfeed items
                        if (newsfeedPosts.Count > 0)
                        {
                            var notification = new NotificationElement(NotificationElement.NotificationType.Newsfeed, newsfeedPosts.Count)
                            {
                                PostId = newsfeedPosts.Max(p => p.Post.PostId),
                                GeneralInfoNewsfeed = true
                            };
                            UnseenNewsfeedsFromServer.Add(notification);
                        }
                    }                    
                    return StatusCode.Ok;

                // there was an error when calling the data from the server
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return StatusCode.NoAuthentication;

                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return StatusCode.NoResponse;

                default:
                    return StatusCode.OtherError;
            }
        }


        private async Task<StatusCode> LoadMessagesContentAsync()
        {
            var lastNotifiedMessage = Settings.LastNotifiedMessage;
            if (lastNotifiedMessage == 0)
                await _getMessages.LoadMessagesInitialAsync(20);
            else
                await _getMessages.LoadMessagesNewAsync(lastNotifiedMessage, 999);

            switch (_getMessages.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    // keep only if not yet seen and order descending by date
                    var messages = _getMessages.Messages.Where(m => m.Message.MessageSeen == "0")
                                            .OrderByDescending(m => m.MessageCreatedAtDateTime)
                                            .ToList();

                    foreach (var message in messages)
                    {
                        var notification = new NotificationElement(NotificationElement.NotificationType.Message, message);
                        // ignore messages from myself (should not arrive as they have no seen=1 flag)
                        if (notification.Message.Sender.User.UserUrl.ToLower() == _authenticatedUser.ToLower())
                            continue;
                        UnseenMessagesFromServer.Add(notification);
                    }
                    return StatusCode.Ok;

                // there was an error when calling the data from the server
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return StatusCode.NoAuthentication;

                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return StatusCode.NoResponse;

                default:
                    return StatusCode.OtherError;
            }
        }


    }
}
