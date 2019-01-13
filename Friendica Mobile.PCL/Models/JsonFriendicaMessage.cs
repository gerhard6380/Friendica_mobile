using Newtonsoft.Json;

namespace Friendica_Mobile.PCL.Models
{
    public class JsonFriendicaMessage
    {
        private string _messageId;
        private double? _messageSenderId;
        private string _messageText;
        private double? _messageRecipientId;
        private string _messageCreatedAt;
        private string _messageSenderScreenName;
        private string _messageRecipientScreenName;
        private JsonFriendicaUser _messageSender;
        private JsonFriendicaUser _messageRecipient;
        private string _messageTitle;
        private string _messageSeen;
        private string _messageParentUri;

        [JsonProperty("id")]
        public string MessageId
        {
            get { return _messageId; }
            set { _messageId = value; }
        }

        [JsonProperty("sender_id")]
        public double? MessageSenderId
        {
            get { return _messageSenderId; }
            set { _messageSenderId = value; }
        }

        [JsonProperty("text")]
        public string MessageText
        {
            get { return _messageText; }
            set { _messageText = value; }
        }

        [JsonProperty("recipient_id")]
        public double? MessageRecipientId
        {
            get { return _messageRecipientId; }
            set { _messageRecipientId = value; }
        }

        [JsonProperty("created_at")]
        public string MessageCreatedAt
        {
            get { return _messageCreatedAt; }
            set { _messageCreatedAt = value; }
        }

        [JsonProperty("sender_screen_name")]
        public string MessageSenderScreenName
        {
            get { return _messageSenderScreenName; }
            set { _messageSenderScreenName = value; }
        }

        [JsonProperty("recipient_screen_name")]
        public string MessageRecipientScreenName
        {
            get { return _messageRecipientScreenName; }
            set { _messageRecipientScreenName = value; }
        }

        [JsonProperty("sender")]
        public JsonFriendicaUser MessageSender
        {
            get { return _messageSender; }
            set { _messageSender = value; }
        }

        [JsonProperty("recipient")]
        public JsonFriendicaUser MessageRecipient
        {
            get { return _messageRecipient; }
            set { _messageRecipient = value; }
        }

        [JsonProperty("title")]
        public string MessageTitle
        {
            get { return _messageTitle; }
            set { _messageTitle = value; }
        }

        [JsonProperty("friendica_seen")]
        public string MessageSeen
        {
            get { return _messageSeen; }
            set { _messageSeen = value; }
        }

        [JsonProperty("friendica_parent_uri")]
        public string MessageParentUri
        {
            get { return _messageParentUri; }
            set { _messageParentUri = value; }
        }
    }
}
