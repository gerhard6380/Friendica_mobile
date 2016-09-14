using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaPost
    {
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser, FriendicaGeo, FriendicaActivities };

        private const string postTextKey = "text";
        private const string postTruncatedKey = "truncated";
        private const string postCreatedAtKey = "created_at";
        private const string postInReplyToStatusIdKey = "in_reply_to_status_id";
        private const string postInReplyToStatusIdStrKey = "in_reply_to_status_id_str";
        private const string postSourceKey = "source";
        private const string postIdKey = "id";
        private const string postIdStrKey = "id_str";
        private const string postInReplyToUserIdKey = "in_reply_to_user_id";
        private const string postInReplyToUserIdStrKey = "in_reply_to_user_id_str";
        private const string postInReplyToScreenNameKey = "in-reply_to_screen_name";
        private const string postGeoKey = "geo";
        private const string postLocationKey = "location";
        private const string postFavoritedKey = "favorited";
        private const string postUserKey = "user";
        private const string postStatusnetHtmlKey = "statusnet_html";
        private const string postStatusnetConversationIdKey = "statusnet_conversation_id";
        private const string postRetweetedStatusKey = "retweeted_status";
        private const string postFriendicaActivitiesKey = "friendica_activities";

        private string _postText;
        private bool _postTruncated;
        private string _postCreatedAt;
        private double _postInReplyToStatusId;
        private string _postInReplyToStatusIdStr;
        private string _postSource;
        private double _postId;
        private string _postIdStr;
        private double _postInReplyToUserId;
        private string _postInReplyToUserIdStr;
        private string _postInReplyToScreenName;
        private FriendicaGeo _postGeo;
        private string _postLocation;
        private bool _postFavorited;
        private FriendicaUserExtended _postUser;
        private string _postStatusnetHtml;
        private string _postStatusnetConversationId;
        private FriendicaPost _postRetweetedStatus;
        private FriendicaActivities _postFriendicaActivities;

        public FriendicaPost()
        {
            PostText = "";
            PostTruncated = false;
            PostCreatedAt = "";
            PostInReplyToStatusId = 0;
            PostInReplyToStatusIdStr = "";
            PostSource = "";
            PostId = 0;
            PostIdStr = "";
            PostInReplyToUserId = 0;
            PostInReplyToUserIdStr = "";
            PostInReplyToScreenName = "";
            //PostGeo = new FriendicaGeo();
            PostLocation = "";
            PostFavorited = false;
            PostUser = new FriendicaUserExtended();
            PostStatusnetHtml = "";
            PostStatusnetConversationId = "";
            //PostRetweetedStatus = new FriendicaPost();
            PostFriendicaActivities = new FriendicaActivities();
        }


    public FriendicaPost(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            PostText = (string)CheckAttribute(jsonObject, postTextKey, AttributeTypes.String);
            PostTruncated = jsonObject.GetNamedBoolean(postTruncatedKey, false);
            PostCreatedAt = jsonObject.GetNamedString(postCreatedAtKey, "");
            PostInReplyToStatusId = (double)CheckAttribute(jsonObject, postInReplyToStatusIdKey, AttributeTypes.Number);
            PostInReplyToStatusIdStr = (string)CheckAttribute(jsonObject, postInReplyToStatusIdStrKey, AttributeTypes.String);
            PostSource = jsonObject.GetNamedString(postSourceKey, "");
            PostId = jsonObject.GetNamedNumber(postIdKey, 0);
            PostIdStr = jsonObject.GetNamedString(postIdStrKey, "");
            PostInReplyToUserId = (double)CheckAttribute(jsonObject, postInReplyToUserIdKey, AttributeTypes.Number);
            PostInReplyToUserIdStr = (string)CheckAttribute(jsonObject, postInReplyToUserIdStrKey, AttributeTypes.String);
            PostInReplyToScreenName = jsonObject.GetNamedString(postInReplyToScreenNameKey, "");
            PostGeo = (FriendicaGeo)CheckAttribute(jsonObject, postGeoKey, AttributeTypes.FriendicaGeo);
            PostLocation = jsonObject.GetNamedString(postLocationKey, "");
            PostFavorited = jsonObject.GetNamedBoolean(postFavoritedKey, false);
            PostUser = (FriendicaUserExtended)CheckAttribute(jsonObject, postUserKey, AttributeTypes.FriendicaUser);
            PostStatusnetHtml = jsonObject.GetNamedString(postStatusnetHtmlKey, "");
            PostStatusnetConversationId = jsonObject.GetNamedString(postStatusnetConversationIdKey, "");
            PostRetweetedStatus = (FriendicaPost)CheckAttribute(jsonObject, postRetweetedStatusKey, AttributeTypes.FriendicaPost);
            PostFriendicaActivities = (FriendicaActivities)CheckAttribute(jsonObject, postFriendicaActivitiesKey, AttributeTypes.FriendicaActivities);
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
                        return jsonObject.GetNamedString(key, "");
                case AttributeTypes.Number:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return 0.0;
                    else
                        return jsonObject.GetNamedNumber(key, 0);
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
                case AttributeTypes.FriendicaGeo:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaGeo(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaUser:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    // if user is an empty array, we jump over to display the friendica_owner
                    else if (value.ValueType == JsonValueType.Array)
                        return new FriendicaUserExtended(jsonObject.GetNamedObject("friendica_owner"));
                    else
                        return new FriendicaUserExtended(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaActivities:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaActivities(jsonObject.GetNamedObject(key));
                default:
                    return null;
            } 
        }

        public FriendicaPost(JsonObject jsonObject) : this()
        {
            PostText = (string)CheckAttribute(jsonObject, postTextKey, AttributeTypes.String);
            PostTruncated = jsonObject.GetNamedBoolean(postTruncatedKey, false);
            PostCreatedAt = jsonObject.GetNamedString(postCreatedAtKey, "");
            PostInReplyToStatusId = (double)CheckAttribute(jsonObject, postInReplyToStatusIdKey, AttributeTypes.Number);
            PostInReplyToStatusIdStr = (string)CheckAttribute(jsonObject, postInReplyToStatusIdStrKey, AttributeTypes.String);
            PostSource = jsonObject.GetNamedString(postSourceKey, "");
            PostId = jsonObject.GetNamedNumber(postIdKey, 0);
            PostIdStr = jsonObject.GetNamedString(postIdStrKey, "");
            PostInReplyToUserId = (double)CheckAttribute(jsonObject, postInReplyToUserIdKey, AttributeTypes.Number);
            PostInReplyToUserIdStr = (string)CheckAttribute(jsonObject, postInReplyToUserIdStrKey, AttributeTypes.String);
            PostInReplyToScreenName = jsonObject.GetNamedString(postInReplyToScreenNameKey, "");
            PostGeo = (FriendicaGeo)CheckAttribute(jsonObject, postGeoKey, AttributeTypes.FriendicaGeo);
            PostLocation = jsonObject.GetNamedString(postLocationKey, "");
            PostFavorited = jsonObject.GetNamedBoolean(postFavoritedKey, false);
            PostUser = (FriendicaUserExtended)CheckAttribute(jsonObject, postUserKey, AttributeTypes.FriendicaUser);
            PostStatusnetHtml = jsonObject.GetNamedString(postStatusnetHtmlKey, "");
            PostStatusnetConversationId = jsonObject.GetNamedString(postStatusnetConversationIdKey, "");
            PostRetweetedStatus = (FriendicaPost)CheckAttribute(jsonObject, postRetweetedStatusKey, AttributeTypes.FriendicaPost);
            //IJsonValue value;
            //jsonObject.TryGetValue(postRetweetedStatusKey, out value);
            //if (value == null || value.ValueType == JsonValueType.Null)
            //    PostRetweetedStatus = null;
            //else
            //    PostRetweetedStatus = new FriendicaPost(jsonObject.GetNamedObject(postRetweetedStatusKey));
            PostFriendicaActivities = (FriendicaActivities)CheckAttribute(jsonObject, postFriendicaActivitiesKey, AttributeTypes.FriendicaActivities);
        }

        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject[postTextKey] = JsonValue.CreateStringValue(PostText);
            jsonObject[postTruncatedKey] = JsonValue.CreateBooleanValue(PostTruncated);
            jsonObject[postCreatedAtKey] = JsonValue.CreateStringValue(PostCreatedAt);
            jsonObject[postInReplyToStatusIdKey] = JsonValue.CreateNumberValue(PostInReplyToStatusId);
            jsonObject[postInReplyToStatusIdStrKey] = JsonValue.CreateStringValue(PostInReplyToStatusIdStr);
            jsonObject[postSourceKey] = JsonValue.CreateStringValue(PostSource);
            jsonObject[postIdKey] = JsonValue.CreateNumberValue(PostId);
            jsonObject[postIdStrKey] = JsonValue.CreateStringValue(PostIdStr);
            jsonObject[postInReplyToUserIdKey] = JsonValue.CreateNumberValue(PostInReplyToUserId);
            jsonObject[postInReplyToUserIdStrKey] = JsonValue.CreateStringValue(PostInReplyToUserIdStr);
            jsonObject[postInReplyToScreenNameKey] = JsonValue.CreateStringValue(PostInReplyToScreenName);
            jsonObject[postGeoKey] = PostGeo.ToJsonObject();
            jsonObject[postLocationKey] = JsonValue.CreateStringValue(PostLocation);
            jsonObject[postFavoritedKey] = JsonValue.CreateBooleanValue(PostFavorited);
            jsonObject[postUserKey] = PostUser.ToJsonObject();
            jsonObject[postStatusnetHtmlKey] = JsonValue.CreateStringValue(PostStatusnetHtml);
            jsonObject[postStatusnetConversationIdKey] = JsonValue.CreateStringValue(PostStatusnetConversationId);
            jsonObject[postRetweetedStatusKey] = PostRetweetedStatus.ToJsonObject();
            jsonObject[postFriendicaActivitiesKey] = JsonValue.CreateNullValue();
            return jsonObject.Stringify();
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaPostObject = new JsonObject();
            friendicaPostObject.SetNamedValue(postTextKey, JsonValue.CreateStringValue(PostText));
            friendicaPostObject.SetNamedValue(postTruncatedKey, JsonValue.CreateBooleanValue(PostTruncated));
            friendicaPostObject.SetNamedValue(postCreatedAtKey, JsonValue.CreateStringValue(PostCreatedAt));
            friendicaPostObject.SetNamedValue(postInReplyToStatusIdKey, JsonValue.CreateNumberValue(PostInReplyToStatusId));
            friendicaPostObject.SetNamedValue(postInReplyToStatusIdStrKey, JsonValue.CreateStringValue(PostInReplyToStatusIdStr));
            friendicaPostObject.SetNamedValue(postSourceKey, JsonValue.CreateStringValue(PostSource));
            friendicaPostObject.SetNamedValue(postIdKey, JsonValue.CreateNumberValue(PostId));
            friendicaPostObject.SetNamedValue(postIdStrKey, JsonValue.CreateStringValue(PostIdStr));
            friendicaPostObject.SetNamedValue(postInReplyToUserIdKey, JsonValue.CreateNumberValue(PostInReplyToUserId));
            friendicaPostObject.SetNamedValue(postInReplyToUserIdStrKey, JsonValue.CreateStringValue(PostInReplyToUserIdStr));
            friendicaPostObject.SetNamedValue(postInReplyToScreenNameKey, JsonValue.CreateStringValue(PostInReplyToScreenName));
            FriendicaGeo friendicaGeo = new FriendicaGeo();
            //friendicaPostObject.SetNamedValue(postGeoKey, friendicaGeo.ToJsonObject());
            friendicaPostObject.SetNamedValue(postLocationKey, JsonValue.CreateStringValue(PostLocation));
            friendicaPostObject.SetNamedValue(postFavoritedKey, JsonValue.CreateBooleanValue(PostFavorited));
            FriendicaUserExtended friendicaUser = new FriendicaUserExtended();
            friendicaPostObject.SetNamedValue(postUserKey, friendicaUser.ToJsonObject());
            friendicaPostObject.SetNamedValue(postStatusnetHtmlKey, JsonValue.CreateStringValue(PostStatusnetHtml));
            friendicaPostObject.SetNamedValue(postStatusnetConversationIdKey, JsonValue.CreateStringValue(PostStatusnetConversationId));
            FriendicaPost friendicaPost = new FriendicaPost();
            friendicaPostObject.SetNamedValue(postRetweetedStatusKey, friendicaPost.ToJsonObject());
            FriendicaActivities friendicaActivities = new FriendicaActivities();
            friendicaPostObject.SetNamedValue(postFriendicaActivitiesKey, JsonValue.CreateNullValue());
            return friendicaPostObject;
        }

        public string PostText
        {
            get { return _postText; }
            set { _postText = value; }
        }

        public bool PostTruncated
        {
            get { return _postTruncated; }
            set { _postTruncated = value; }
        }

        public string PostCreatedAt
        {
            get { return _postCreatedAt; }
            set { _postCreatedAt = value; }
        }

        public double PostInReplyToStatusId
        {
            get { return _postInReplyToStatusId; }
            set { _postInReplyToStatusId = value; }
        }

        public string PostInReplyToStatusIdStr
        {
            get { return _postInReplyToStatusIdStr; }
            set { _postInReplyToStatusIdStr = value; }
        }

        public string PostSource
        {
            get { return _postSource; }
            set { _postSource = value; }
        }

        public double PostId
        {
            get { return _postId; }
            set { _postId = value; }
        }

        public string PostIdStr
        {
            get { return _postIdStr; }
            set { _postIdStr = value; }
        }

        public double PostInReplyToUserId
        {
            get { return _postInReplyToUserId; }
            set { _postInReplyToUserId = value; }
        }

        public string PostInReplyToUserIdStr
        {
            get { return _postInReplyToUserIdStr; }
            set { _postInReplyToUserIdStr = value; }
        }

        public string PostInReplyToScreenName
        {
            get { return _postInReplyToScreenName; }
            set { _postInReplyToScreenName = value; }
        }

        public FriendicaGeo PostGeo
        {
            get { return _postGeo; }
            set { _postGeo = value; }
        }

        public string PostLocation
        {
            get { return _postLocation; }
            set { _postLocation = value; }
        }

        public bool PostFavorited
        {
            get { return _postFavorited; }
            set { _postFavorited = value; }
        }

        public FriendicaUserExtended PostUser
        {
            get { return _postUser; }
            set { _postUser = value; }
        }

        public string PostStatusnetHtml
        {
            get { return _postStatusnetHtml; }
            set { _postStatusnetHtml = value; }
        }

        public string PostStatusnetConversationId
        {
            get { return _postStatusnetConversationId; }
            set { _postStatusnetConversationId = value; }
        }

        public FriendicaPost PostRetweetedStatus
        {
            get { return _postRetweetedStatus; }
            set { _postRetweetedStatus = value; }
        }

        public FriendicaActivities PostFriendicaActivities
        {
            get { return _postFriendicaActivities; }
            set { _postFriendicaActivities = value; }
        }
    }
}
