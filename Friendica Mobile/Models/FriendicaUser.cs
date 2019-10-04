using System.Collections.Generic;
using System.Windows.Input;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Friendica_Mobile.Models
{
    public class FriendicaUser
    {
        [JsonProperty(PropertyName = "id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "id_str")]
        public string UserIdStr { get; set; }

        private string _username;
        [JsonProperty(PropertyName = "name")]
        public string UserName 
        { 
            get { return _username; } 
            set
            {
                _username = value;
                IsAuthenticatedUser = SetIsAuthenticatedUser();
            } 
        }

        [JsonProperty(PropertyName = "screen_name")]
        public string UserScreenName { get; set; }

        [JsonProperty(PropertyName = "location")]
        public string UserLocation { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string UserDescription { get; set; }

        [JsonProperty(PropertyName = "profile_image_url")]
        public string UserProfileImageUrl { get; set; }

        [JsonProperty(PropertyName = "profile_image_url_https")]
        public string UserProfileImageUrlHttps { get; set; }

        [JsonProperty(PropertyName = "profile_image_url_large")]
        public string UserProfileImageUrlLarge { get; set; }

        private string _userUrl;
        [JsonProperty(PropertyName = "url")]
        public string UserUrl 
        { 
            get { return _userUrl; } 
            set
            {
                _userUrl = value;
                IsAuthenticatedUser = SetIsAuthenticatedUser();
            } 
        }

        [JsonProperty(PropertyName = "protected")]
        public bool UserProtected { get; set; }

        [JsonProperty(PropertyName = "followers_count")]
        public int UserFollowersCount { get; set; }

        [JsonProperty(PropertyName = "friends_count")]
        public int UserFriendsCounts { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string UserCreatedAt { get; set; }

        [JsonProperty(PropertyName = "favourites_count")]
        public int UserFavouritesCount { get; set; }

        [JsonProperty(PropertyName = "utc_offset")]
        public int UserUtcOffset { get; set; }

        [JsonProperty(PropertyName = "time_zone")]
        public string UserTimeZone { get; set; }

        [JsonProperty(PropertyName = "statuses_count")]
        public int UserStatusesCount { get; set; }

        [JsonProperty(PropertyName = "following")]
        public bool UserFollowing { get; set; }

        [JsonProperty(PropertyName = "verified")]
        public bool UserVerified { get; set; }

        [JsonProperty(PropertyName = "statusnet_blocking")]
        public bool UserStatusnetBlocking { get; set; }

        [JsonProperty(PropertyName = "notifications")]
        public bool UserNotifications { get; set; }

        [JsonProperty(PropertyName = "statusnet_profile_url")]
        public string UserStatusnetProfileUrl { get; set; }

        [JsonProperty(PropertyName = "cid")]
        public int UserCid { get; set; }

        [JsonProperty(PropertyName = "network")]
        public string UserNetwork { get; set; }


        // property to identify if a user dataset is referring to the user himself (in activities used)
        [JsonIgnore]
        public bool IsAuthenticatedUser { get; set; }


        private ICommand _openProfileCommand;
        public ICommand OpenProfileCommand => _openProfileCommand ?? (_openProfileCommand = new Command(OpenProfile));
        private void OpenProfile()
        {
            Launchers.OpenUrlWithZrl(UserStatusnetProfileUrl, true);
        }


        // method to check if user is the authenticated user
        private bool SetIsAuthenticatedUser()
        {
            // no authenticated user if there is no server set so far (sample mode)
            if (Settings.IsFriendicaLoginDefined())
            {
                // check url as well as there could be users with identic screen names on different servers
                return (UserScreenName != null && UserUrl != null 
                    && Settings.FriendicaUsername.ToLower() == UserScreenName.ToLower() 
                    && UserUrl.Contains(Settings.FriendicaServer));
            }
            return false;
        }
    }
}