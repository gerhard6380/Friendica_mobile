using Newtonsoft.Json;
using SQLite;

namespace Friendica_Mobile.Models
{
    public class JsonFriendicaUser
    {
        private string _userId;
        private string _userIdStr;
        private string _userName;
        private string _userScreenName;
        private string _userLocation;
        private string _userDescription;
        private string _userProfileImageUrl;
        private string _userProfileImageUrlHttps;
        private string _userUrl;
        private bool _userProtected;
        private double _userFollowersCount;
        private double _userFriendsCount;
        private string _userCreatedAt;
        private double _userFavouritesCount;
        private double _userUtcOffset;
        private string _userTimeZone;
        private double _userStatusesCount;
        private bool _userFollowing;
        private bool _userVerified;
        private bool _userStatusnetBlocking;
        private bool _userNotifications;
        private string _userStatusnetProfileUrl;
        private double _userCid;
        private string _userNetwork;


        [JsonProperty("id")]
        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        [JsonProperty("id_str")]
        public string UserIdStr
        {
            get { return _userIdStr; }
            set { _userIdStr = value; }
        }

        [JsonProperty("name")]
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        [JsonProperty("screen_name")]
        public string UserScreenName
        {
            get { return _userScreenName; }
            set { _userScreenName = value; }
        }

        [JsonProperty("location")]
        public string UserLocation
        {
            get { return _userLocation; }
            set { _userLocation = value; }
        }

        [JsonProperty("description")]
        public string UserDescription
        {
            get { return _userDescription; }
            set { _userDescription = value; }
        }

        [JsonProperty("profile_image_url")]
        public string UserProfileImageUrl
        {
            get { return _userProfileImageUrl; }
            set { _userProfileImageUrl = value; }
        }

        [JsonProperty("profile_image_url_https")]
        public string UserProfileImageUrlHttps
        {
            get { return _userProfileImageUrlHttps; }
            set { _userProfileImageUrlHttps = value; }
        }

        [JsonProperty("url")]
        public string UserUrl
        {
            get { return _userUrl; }
            set { _userUrl = value; }
        }

        [JsonProperty("protected")]
        public bool UserProtected
        {
            get { return _userProtected; }
            set { _userProtected = value; }
        }

        [JsonProperty("followers_count")]
        public double UserFollowersCount
        {
            get { return _userFollowersCount; }
            set { _userFollowersCount = value; }
        }

        [JsonProperty("friends_count")]
        public double UserFriendsCount
        {
            get { return _userFriendsCount; }
            set { _userFriendsCount = value; }
        }

        [JsonProperty("created_at")]
        public string UserCreatedAt
        {
            get { return _userCreatedAt; }
            set { _userCreatedAt = value; }
        }

        [JsonProperty("favourites_count")]
        public double UserFavouritesCount
        {
            get { return _userFavouritesCount; }
            set { _userFavouritesCount = value; }
        }

        [JsonProperty("utc_offset")]
        public double UserUtcOffset
        {
            get { return _userUtcOffset; }
            set { _userUtcOffset = value; }
        }

        [JsonProperty("time_zone")]
        public string UserTimeZone
        {
            get { return _userTimeZone; }
            set { _userTimeZone = value; }
        }

        [JsonProperty("statusnet_count")]
        public double UserStatusesCount
        {
            get { return _userStatusesCount; }
            set { _userStatusesCount = value; }
        }

        [JsonProperty("following")]
        public bool UserFollowing
        {
            get { return _userFollowing; }
            set { _userFollowing = value; }
        }

        [JsonProperty("verified")]
        public bool UserVerified
        {
            get { return _userVerified; }
            set { _userVerified = value; }
        }

        [JsonProperty("statusnet_blocking")]
        public bool UserStatusnetBlocking
        {
            get { return _userStatusnetBlocking; }
            set { _userStatusnetBlocking = value; }
        }

        [JsonProperty("notifications")]
        public bool UserNotifications
        {
            get { return _userNotifications; }
            set { _userNotifications = value; }
        }

        [JsonProperty("statusnet_profile_url")]
        public string UserStatusnetProfileUrl
        {
            get { return _userStatusnetProfileUrl; }
            set { _userStatusnetProfileUrl = value; }
        }

        [JsonProperty("cid")]
        public double UserCid
        {
            get { return _userCid; }
            set { _userCid = value; }
        }

        [JsonProperty("network")]
        public string UserNetwork
        {
            get { return _userNetwork; }
            set { _userNetwork = value; }
        }

    }
}
