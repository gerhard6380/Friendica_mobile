using Newtonsoft.Json;

namespace Friendica_Mobile.Models
{
    public class JsonFriendicaPost
    {
        private string _postText;
        private bool _postTruncated;
        private string _postCreatedAt;
        private double? _postInReplyToStatusId;
        private string _postInReplyToStatusIdStr;
        private string _postSource;
        private double _postId;
        private string _postIdStr;
        private double? _postInReplyToUserId;
        private string _postInReplyToUserIdStr;
        private string _postInReplyToScreenName;
        private JsonFriendicaGeo _postGeo;
        private string _postLocation;
        private bool _postFavorited;
        private JsonFriendicaUser _postUser;
        private string _postStatusnetHtml;
        private string _postStatusnetConversationId;
        private JsonFriendicaPost _postRetweetedStatus;
        private JsonFriendicaActivities _postFriendicaActivities;


        [JsonProperty("text")]
        public string PostText
        {
            get { return _postText; }
            set { _postText = value; }
        }

        [JsonProperty("truncated")]
        public bool PostTruncated
        {
            get { return _postTruncated; }
            set { _postTruncated = value; }
        }

        [JsonProperty("created_at")]
        public string PostCreatedAt
        {
            get { return _postCreatedAt; }
            set { _postCreatedAt = value; }
        }

        [JsonProperty("in_reply_to_status_id")]
        public double? PostInReplyToStatusId
        {
            get { return _postInReplyToStatusId; }
            set { _postInReplyToStatusId = value; }
        }

        [JsonProperty("in_reply_to_status_id_str")]
        public string PostInReplyToStatusIdStr
        {
            get { return _postInReplyToStatusIdStr; }
            set { _postInReplyToStatusIdStr = value; }
        }

        [JsonProperty("source")]
        public string PostSource
        {
            get { return _postSource; }
            set { _postSource = value; }
        }

        [JsonProperty("id")]
        public double PostId
        {
            get { return _postId; }
            set { _postId = value; }
        }

        [JsonProperty("id_str")]
        public string PostIdStr
        {
            get { return _postIdStr; }
            set { _postIdStr = value; }
        }

        [JsonProperty("in_reply_to_user_id")]
        public double? PostInReplyToUserId
        {
            get { return _postInReplyToUserId; }
            set { _postInReplyToUserId = value; }
        }

        [JsonProperty("in_reply_to_user_id_str")]
        public string PostInReplyToUserIdStr
        {
            get { return _postInReplyToUserIdStr; }
            set { _postInReplyToUserIdStr = value; }
        }

        [JsonProperty("in_reply_to_screen_name")]
        public string PostInReplyToScreenName
        {
            get { return _postInReplyToScreenName; }
            set { _postInReplyToScreenName = value; }
        }

        [JsonProperty("geo")]
        public JsonFriendicaGeo PostGeo
        {
            get { return _postGeo; }
            set { _postGeo = value; }
        }

        [JsonProperty("location")]
        public string PostLocation
        {
            get { return _postLocation; }
            set { _postLocation = value; }
        }

        [JsonProperty("favorited")]
        public bool PostFavorited
        {
            get { return _postFavorited; }
            set { _postFavorited = value; }
        }

        [JsonProperty("user")]
        public JsonFriendicaUser PostUser
        {
            get { return _postUser; }
            set { _postUser = value; }
        }

        [JsonProperty("statusnet_html")]
        public string PostStatusnetHtml
        {
            get { return _postStatusnetHtml; }
            set { _postStatusnetHtml = value; }
        }

        [JsonProperty("statusnet_conversation_id")]
        public string PostStatusnetConversationId
        {
            get { return _postStatusnetConversationId; }
            set { _postStatusnetConversationId = value; }
        }

        [JsonProperty("retweeted_status")]
        public JsonFriendicaPost PostRetweetedStatus
        {
            get { return _postRetweetedStatus; }
            set { _postRetweetedStatus = value; }
        }

        [JsonProperty("friendica_activities")]
        public JsonFriendicaActivities PostFriendicaActivities
        {
            get { return _postFriendicaActivities; }
            set { _postFriendicaActivities = value; }
        }

    }
}
