using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.UWP.Models
{
    public class FriendicaUser
    {
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser };

        private const string userIdKey = "id";
        private const string userIdStrKey = "id_str";
        private const string userNameKey = "name";
        private const string userScreenNameKey = "screen_name";
        private const string userLocationKey = "location";
        private const string userDescriptionKey = "description";
        private const string userProfileImageUrlKey = "profile_image_url";
        private const string userProfileImageUrlHttpsKey = "profile_image_url_https";
        private const string userUrlKey = "url";
        private const string userProtectedKey = "protected";
        private const string userFollowersCountKey = "followers_count";
        private const string userFriendsCountKey = "friends_count";
        private const string userCreatedAtKey = "created_at";
        private const string userFavouritesCountKey = "favourites_count";
        private const string userUtcOffsetKey = "utc_offset";
        private const string userTimeZoneKey = "time_zone";
        private const string userStatusesCountKey = "statuses_count";
        private const string userFollowingKey = "following";
        private const string userVerifiedKey = "verified";
        private const string userStatusnetBlockingKey = "statusnet_blocking";
        private const string userNotificationsKey = "notifications";
        private const string userStatusnetProfileUrlKey = "statusnet_profile_url";
        private const string userCidKey = "cid";
        private const string userNetworkKey = "network";

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

        public FriendicaUser()
        {
            UserId = "";
            UserIdStr = "";
            UserName = "";
            UserScreenName = "";
            UserLocation = "";
            UserDescription = "";
            UserProfileImageUrl = "";
            UserProfileImageUrlHttps = "";
            UserUrl = "";
            UserProtected = false;
            UserFollowersCount = 0;
            UserFriendsCount = 0;
            UserCreatedAt = "";
            UserFavouritesCount = 0;
            UserUtcOffset = 0;
            UserTimeZone = "";
            UserStatusesCount = 0;
            UserFollowing = false;
            UserVerified = false;
            UserStatusnetBlocking = false;
            UserNotifications = false;
            UserStatusnetProfileUrl = "";
            UserCid = 0;
            UserNetwork = "";
        }

        private JsonValue SetAttribute(object field, AttributeTypes type)
        {
            if (type == AttributeTypes.String)
            {
                if (field == null)
                    return JsonValue.CreateNullValue();
                else
                    return JsonValue.CreateStringValue((string)field);
            }
            else if (type == AttributeTypes.Number)
            {
                if (field == null)
                    return JsonValue.CreateNullValue();
                else
                    return JsonValue.CreateNumberValue((double)field);
            }
            else if (type == AttributeTypes.Boolean)
            {
                if (field == null)
                    return JsonValue.CreateNullValue();
                else
                    return JsonValue.CreateBooleanValue((bool)field);
            }
            else
                return JsonValue.CreateNullValue();
        }


        private object CheckAttribute(JsonObject jsonObject, string key, AttributeTypes type)
        {
            IJsonValue value;
            jsonObject.TryGetValue(key, out value);
            switch (type)
            {
                case AttributeTypes.String:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                    {
                        // es gibt Fälle, wo ein String-Feld als integer im Json-String enthalten ist (user/id), darauf reagieren
                        if (value.ValueType == JsonValueType.String)
                            return jsonObject.GetNamedString(key, "");
                        else if (value.ValueType == JsonValueType.Number)
                            return jsonObject.GetNamedNumber(key, 0.0).ToString();
                        else
                            return null;
                    }
                        
                case AttributeTypes.Number:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return 0.0;
                    else
                    {
                        if (value.ValueType == JsonValueType.Number)
                            return jsonObject.GetNamedNumber(key, 0);
                        else if (value.ValueType == JsonValueType.String)
                            return Double.Parse(jsonObject.GetNamedString(key, ""));
                        else
                            return 0.0;
                    }
                case AttributeTypes.Boolean:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedBoolean(key, false);
                case AttributeTypes.FriendicaPost:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaPost(jsonObject.GetNamedObject(key));
                default:
                    return null;
            }
        }

        public FriendicaUser(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            UserId = (string)CheckAttribute(jsonObject, userIdKey, AttributeTypes.String);
            UserIdStr = jsonObject.GetNamedString(userIdStrKey, "");
            UserName = jsonObject.GetNamedString(userNameKey, "");
            UserScreenName = jsonObject.GetNamedString(userScreenNameKey, "");
            UserLocation = (string)CheckAttribute(jsonObject, userLocationKey, AttributeTypes.String);
            UserDescription = (string)CheckAttribute(jsonObject, userDescriptionKey, AttributeTypes.String);
            UserProfileImageUrl = jsonObject.GetNamedString(userProfileImageUrlKey, "");
            UserProfileImageUrlHttps = jsonObject.GetNamedString(userProfileImageUrlHttpsKey, "");
            UserUrl = jsonObject.GetNamedString(userUrlKey, "");
            UserProtected = jsonObject.GetNamedBoolean(userProtectedKey, false);
            UserFollowersCount = jsonObject.GetNamedNumber(userFollowersCountKey, 0);
            UserFriendsCount = jsonObject.GetNamedNumber(userFriendsCountKey, 0);
            UserCreatedAt = jsonObject.GetNamedString(userCreatedAtKey, "");
            UserFavouritesCount = jsonObject.GetNamedNumber(userFavouritesCountKey, 0);
            UserUtcOffset = (double)CheckAttribute(jsonObject, userUtcOffsetKey, AttributeTypes.Number);
            UserTimeZone = jsonObject.GetNamedString(userTimeZoneKey, "");
            UserStatusesCount = jsonObject.GetNamedNumber(userStatusesCountKey, 0);
            UserFollowing = jsonObject.GetNamedBoolean(userFollowingKey, false);
            UserVerified = jsonObject.GetNamedBoolean(userVerifiedKey, false);
            UserStatusnetBlocking = jsonObject.GetNamedBoolean(userStatusnetBlockingKey, false);
            UserNotifications = jsonObject.GetNamedBoolean(userNotificationsKey, false);
            UserStatusnetProfileUrl = jsonObject.GetNamedString(userStatusnetProfileUrlKey, "");
            UserCid = jsonObject.GetNamedNumber(userCidKey, 0);
            UserNetwork = jsonObject.GetNamedString(userNetworkKey, "");
        }

        public FriendicaUser(JsonObject jsonObject) : this()
        {
            UserId = (string)CheckAttribute(jsonObject, userIdKey, AttributeTypes.String);
            UserIdStr = jsonObject.GetNamedString(userIdStrKey, "");
            UserName = jsonObject.GetNamedString(userNameKey, "");
            UserScreenName = jsonObject.GetNamedString(userScreenNameKey, "");
            UserLocation = (string)CheckAttribute(jsonObject, userLocationKey, AttributeTypes.String);
            UserDescription = (string)CheckAttribute(jsonObject, userDescriptionKey, AttributeTypes.String);
            UserProfileImageUrl = (string)CheckAttribute(jsonObject, userProfileImageUrlKey, AttributeTypes.String);
            UserProfileImageUrlHttps = (string)CheckAttribute(jsonObject, userProfileImageUrlHttpsKey, AttributeTypes.String);
            UserUrl = jsonObject.GetNamedString(userUrlKey, "");
            UserProtected = jsonObject.GetNamedBoolean(userProtectedKey, false);
            UserFollowersCount = jsonObject.GetNamedNumber(userFollowersCountKey, 0);
            UserFriendsCount = jsonObject.GetNamedNumber(userFriendsCountKey, 0);
            UserCreatedAt = jsonObject.GetNamedString(userCreatedAtKey, "");
            UserFavouritesCount = jsonObject.GetNamedNumber(userFavouritesCountKey, 0);
            UserUtcOffset = (double)CheckAttribute(jsonObject, userUtcOffsetKey, AttributeTypes.Number);
            UserTimeZone = jsonObject.GetNamedString(userTimeZoneKey, "");
            UserStatusesCount = jsonObject.GetNamedNumber(userStatusesCountKey, 0);
            UserFollowing = jsonObject.GetNamedBoolean(userFollowingKey, false);
            UserVerified = jsonObject.GetNamedBoolean(userVerifiedKey, false);
            UserStatusnetBlocking = jsonObject.GetNamedBoolean(userStatusnetBlockingKey, false);
            UserNotifications = jsonObject.GetNamedBoolean(userNotificationsKey, false);
            UserStatusnetProfileUrl = jsonObject.GetNamedString(userStatusnetProfileUrlKey, "");
            UserCid = jsonObject.GetNamedNumber(userCidKey, 0);
            UserNetwork = jsonObject.GetNamedString(userNetworkKey, "");
        }
        
        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject[userIdKey] = JsonValue.CreateStringValue(UserId);
            jsonObject[userIdStrKey] = JsonValue.CreateStringValue(UserIdStr);
            jsonObject[userNameKey] = JsonValue.CreateStringValue(UserName);
            jsonObject[userScreenNameKey] = JsonValue.CreateStringValue(UserScreenName);
            jsonObject[userLocationKey] = JsonValue.CreateStringValue(UserLocation);
            jsonObject[userDescriptionKey] = JsonValue.CreateStringValue(UserDescription);
            jsonObject[userProfileImageUrlKey] = JsonValue.CreateStringValue(UserProfileImageUrl);
            jsonObject[userProfileImageUrlHttpsKey] = JsonValue.CreateStringValue(UserProfileImageUrlHttps);
            jsonObject[userUrlKey] = JsonValue.CreateStringValue(UserUrl);
            jsonObject[userProtectedKey] = JsonValue.CreateBooleanValue(UserProtected);
            jsonObject[userFollowersCountKey] = JsonValue.CreateNumberValue(UserFollowersCount);
            jsonObject[userFriendsCountKey] = JsonValue.CreateNumberValue(UserFriendsCount);
            jsonObject[userCreatedAtKey] = JsonValue.CreateStringValue(UserCreatedAt);
            jsonObject[userFavouritesCountKey] = JsonValue.CreateNumberValue(UserFavouritesCount);
            jsonObject[userUtcOffsetKey] = JsonValue.CreateNumberValue(UserUtcOffset);
            jsonObject[userTimeZoneKey] = JsonValue.CreateStringValue(UserTimeZone);
            jsonObject[userStatusesCountKey] = JsonValue.CreateNumberValue(UserStatusesCount);
            jsonObject[userFollowingKey] = JsonValue.CreateBooleanValue(UserFollowing);
            jsonObject[userVerifiedKey] = JsonValue.CreateBooleanValue(UserVerified);
            jsonObject[userStatusnetBlockingKey] = JsonValue.CreateBooleanValue(UserStatusnetBlocking);
            jsonObject[userNotificationsKey] = JsonValue.CreateBooleanValue(UserNotifications);
            jsonObject[userStatusnetProfileUrlKey] = JsonValue.CreateStringValue(UserStatusnetProfileUrl);
            jsonObject[userCidKey] = JsonValue.CreateNumberValue(UserCid);
            jsonObject[userNetworkKey] = JsonValue.CreateStringValue(UserNetwork);

            return jsonObject.Stringify();
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaUserObject = new JsonObject();
            friendicaUserObject.SetNamedValue(userIdKey, JsonValue.CreateStringValue(UserId));
            friendicaUserObject.SetNamedValue(userIdStrKey, JsonValue.CreateStringValue(UserIdStr));
            friendicaUserObject.SetNamedValue(userNameKey, JsonValue.CreateStringValue(UserName));
            friendicaUserObject.SetNamedValue(userScreenNameKey, JsonValue.CreateStringValue(UserScreenName));
            friendicaUserObject.SetNamedValue(userLocationKey, JsonValue.CreateStringValue(UserLocation));
            friendicaUserObject.SetNamedValue(userDescriptionKey, SetAttribute(UserDescription, AttributeTypes.String));
            //friendicaUserObject.SetNamedValue(userDescriptionKey, JsonValue.CreateStringValue(UserDescription));
            friendicaUserObject.SetNamedValue(userProfileImageUrlKey, JsonValue.CreateStringValue(UserProfileImageUrl));
            friendicaUserObject.SetNamedValue(userProfileImageUrlHttpsKey, JsonValue.CreateStringValue(UserProfileImageUrlHttps));
            friendicaUserObject.SetNamedValue(userUrlKey, JsonValue.CreateStringValue(UserUrl));
            friendicaUserObject.SetNamedValue(userProtectedKey, JsonValue.CreateBooleanValue(UserProtected));
            friendicaUserObject.SetNamedValue(userFollowersCountKey, JsonValue.CreateNumberValue(UserFollowersCount));
            friendicaUserObject.SetNamedValue(userFriendsCountKey, JsonValue.CreateNumberValue(UserFriendsCount));
            friendicaUserObject.SetNamedValue(userCreatedAtKey, JsonValue.CreateStringValue(UserCreatedAt));
            friendicaUserObject.SetNamedValue(userFavouritesCountKey, JsonValue.CreateNumberValue(UserFavouritesCount));
            friendicaUserObject.SetNamedValue(userUtcOffsetKey, JsonValue.CreateNumberValue(UserUtcOffset));
            friendicaUserObject.SetNamedValue(userTimeZoneKey, JsonValue.CreateStringValue(UserTimeZone));
            friendicaUserObject.SetNamedValue(userStatusesCountKey, JsonValue.CreateNumberValue(UserStatusesCount));
            friendicaUserObject.SetNamedValue(userFollowingKey, JsonValue.CreateBooleanValue(UserFollowing));
            friendicaUserObject.SetNamedValue(userVerifiedKey, JsonValue.CreateBooleanValue(UserVerified));
            friendicaUserObject.SetNamedValue(userStatusnetBlockingKey, JsonValue.CreateBooleanValue(UserStatusnetBlocking));
            friendicaUserObject.SetNamedValue(userNotificationsKey, JsonValue.CreateBooleanValue(UserNotifications));
            friendicaUserObject.SetNamedValue(userStatusnetProfileUrlKey, JsonValue.CreateStringValue(UserStatusnetProfileUrl));
            friendicaUserObject.SetNamedValue(userCidKey, JsonValue.CreateNumberValue(UserCid));
            friendicaUserObject.SetNamedValue(userNetworkKey, JsonValue.CreateStringValue(UserNetwork));
            return friendicaUserObject;
        }

        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public string UserIdStr
        {
            get { return _userIdStr; }
            set { _userIdStr = value; }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public string UserScreenName
        {
            get { return _userScreenName; }
            set { _userScreenName = value; }
        }

        public string UserLocation
        {
            get { return _userLocation; }
            set { _userLocation = value; }
        }

        public string UserDescription
        {
            get { return _userDescription; }
            set { _userDescription = value; }
        }

        public string UserProfileImageUrl
        {
            get { return _userProfileImageUrl; }
            set { _userProfileImageUrl = value; }
        }

        public string UserProfileImageUrlHttps
        {
            get { return _userProfileImageUrlHttps; }
            set { _userProfileImageUrlHttps = value; }
        }

        public string UserUrl
        {
            get { return _userUrl; }
            set { _userUrl = value; }
        }

        public bool UserProtected
        {
            get { return _userProtected; }
            set { _userProtected = value; }
        }

        public double UserFollowersCount
        {
            get { return _userFollowersCount; }
            set { _userFollowersCount = value; }
        }

        public double UserFriendsCount
        {
            get { return _userFriendsCount; }
            set { _userFriendsCount = value; }
        }

        public string UserCreatedAt
        {
            get { return _userCreatedAt; }
            set { _userCreatedAt = value; }
        }

        public double UserFavouritesCount
        {
            get { return _userFavouritesCount; }
            set { _userFavouritesCount = value; }
        }

        public double UserUtcOffset
        {
            get { return _userUtcOffset; }
            set { _userUtcOffset = value; }
        }

        public string UserTimeZone
        {
            get { return _userTimeZone; }
            set { _userTimeZone = value; }
        }

        public double UserStatusesCount
        {
            get { return _userStatusesCount; }
            set { _userStatusesCount = value; }
        }

        public bool UserFollowing
        {
            get { return _userFollowing; }
            set { _userFollowing = value; }
        }

        public bool UserVerified
        {
            get { return _userVerified; }
            set { _userVerified = value; }
        }

        public bool UserStatusnetBlocking
        {
            get { return _userStatusnetBlocking; }
            set { _userStatusnetBlocking = value; }
        }

        public bool UserNotifications
        {
            get { return _userNotifications; }
            set { _userNotifications = value; }
        }

        public string UserStatusnetProfileUrl
        {
            get { return _userStatusnetProfileUrl; }
            set { _userStatusnetProfileUrl = value; }
        }

        public double UserCid
        {
            get { return _userCid; }
            set { _userCid = value; }
        }

        public string UserNetwork
        {
            get { return _userNetwork; }
            set { _userNetwork = value; }
        }
    }
}
