using BackgroundTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI.Xaml.Documents;

namespace BackgroundTasks
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public sealed class FriendicaMessage
    {
        AppSettings appSettings = new AppSettings();
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();

        private enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser, FriendicaGeo };

        // API interface properties
        private const string messageIdKey = "id";
        private const string messageSenderIdKey = "sender_id";
        private const string messageTextKey = "text";
        private const string messageRecipientIdKey = "recipient_id";
        private const string messageCreatedAtKey = "created_at";
        private const string messageSenderScreenNameKey = "sender_screen_name";
        private const string messageRecipientScreenNameKey = "recipient_screen_name";
        private const string messageSenderKey = "sender";
        private const string messageRecipientKey = "recipient";
        private const string messageTitleKey = "title";
        private const string messageSeenKey = "friendica_seen";
        private const string messageParentUriKey = "friendica_parent_uri";

        private string _messageId;
        private double _messageSenderId;
        private string _messageText;
        private double _messageRecipientId;
        private string _messageCreatedAt;
        private string _messageSenderScreenName;
        private string _messageRecipientScreenName;
        private FriendicaUser _messageSender;
        private FriendicaUser _messageRecipient;
        private string _messageTitle;
        private string _messageSeen;
        private string _messageParentUri;

        // additional derived properties
        private int _messageIdInt;
        public int MessageIdInt
        {
            get { return _messageIdInt; }
            set { _messageIdInt = value; }
        }

        private DateTimeOffset _messageCreatedAtDateTime;
        public DateTimeOffset MessageCreatedAtDateTime
        {
            get { return _messageCreatedAtDateTime; }
            set { _messageCreatedAtDateTime = value;
                MessageCreatedAtLocalized = value.ToString("ddd") + " " + value.ToString("d") + " " + value.ToString("t");
                SetCreatedAtNotification(value); }
        }

        private string _messageCreatedAtLocalized;
        public string MessageCreatedAtLocalized
        {
            get { return _messageCreatedAtLocalized; }
            set { _messageCreatedAtLocalized = value; }
        }

        private Paragraph _messageTextTransformed;
        public Paragraph MessageTextTransformed
        {
            get { return _messageTextTransformed; }
            set { _messageTextTransformed = value;  }
        }

        private string _messageCreatedAtNotification;
        public string MessageCreatedAtNotification
        {
            get { return _messageCreatedAtNotification; }
            set { _messageCreatedAtNotification = value; }
        }

        private string _messageTextNotification;
        public string MessageTextNotification
        {
            get { return _messageTextNotification; }
            set { _messageTextNotification = value; }
        }

        private bool _isReceivedMessage;
        public bool IsReceivedMessage
        {
            get { return _isReceivedMessage; }
            set { _isReceivedMessage = value; }
        }

        //public FriendicaMessage()
        //{
        //    MessageId = "";
        //    MessageSenderId = 0;
        //    MessageText = "";
        //    MessageRecipientId = 0;
        //    MessageCreatedAt = "";
        //    MessageSenderScreenName = "";
        //    MessageRecipientScreenName = "";
        //    MessageSender = new FriendicaUser();
        //    MessageRecipient = new FriendicaUser();
        //    MessageTitle = "";
        //    MessageSeen = "";
        //    MessageParentUri = "";
        //}


    public FriendicaMessage(string jsonString)
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            MessageId = (string)CheckAttribute(jsonObject, messageIdKey, AttributeTypes.String);
            MessageSenderId = (double)CheckAttribute(jsonObject, messageSenderIdKey, AttributeTypes.Number);
            MessageText = (string)CheckAttribute(jsonObject, messageTextKey, AttributeTypes.String);
            MessageRecipientId = (double)CheckAttribute(jsonObject, messageRecipientIdKey, AttributeTypes.Number);
            MessageCreatedAt = (string)CheckAttribute(jsonObject, messageCreatedAtKey, AttributeTypes.String);
            MessageSenderScreenName = (string)CheckAttribute(jsonObject, messageSenderScreenNameKey, AttributeTypes.String);
            MessageRecipientScreenName = (string)CheckAttribute(jsonObject, messageRecipientScreenNameKey, AttributeTypes.String);
            MessageSender = (FriendicaUser)CheckAttribute(jsonObject, messageSenderKey, AttributeTypes.FriendicaUser);
            MessageRecipient = (FriendicaUser)CheckAttribute(jsonObject, messageRecipientKey, AttributeTypes.FriendicaUser);
            MessageTitle = (string)CheckAttribute(jsonObject, messageTitleKey, AttributeTypes.String);
            MessageSeen = (string)CheckAttribute(jsonObject, messageSeenKey, AttributeTypes.String);
            MessageParentUri = (string)CheckAttribute(jsonObject, messageParentUriKey, AttributeTypes.String);
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
                        return new FriendicaPost(jsonObject.GetNamedObject(key), null);
                case AttributeTypes.FriendicaGeo:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaGeo(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaUser:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaUser(jsonObject.GetNamedObject(key), null);
                default:
                    return null;
            } 
        }

        public FriendicaMessage(JsonObject jsonObject, string Objekt)
        {
            MessageId = (string)CheckAttribute(jsonObject, messageIdKey, AttributeTypes.String);
            MessageSenderId = (double)CheckAttribute(jsonObject, messageSenderIdKey, AttributeTypes.Number);
            MessageText = (string)CheckAttribute(jsonObject, messageTextKey, AttributeTypes.String);
            MessageRecipientId = (double)CheckAttribute(jsonObject, messageRecipientIdKey, AttributeTypes.Number);
            MessageCreatedAt = (string)CheckAttribute(jsonObject, messageCreatedAtKey, AttributeTypes.String);
            MessageSenderScreenName = (string)CheckAttribute(jsonObject, messageSenderScreenNameKey, AttributeTypes.String);
            MessageRecipientScreenName = (string)CheckAttribute(jsonObject, messageRecipientScreenNameKey, AttributeTypes.String);
            MessageSender = (FriendicaUser)CheckAttribute(jsonObject, messageSenderKey, AttributeTypes.FriendicaUser);
            MessageRecipient = (FriendicaUser)CheckAttribute(jsonObject, messageRecipientKey, AttributeTypes.FriendicaUser);
            MessageTitle = (string)CheckAttribute(jsonObject, messageTitleKey, AttributeTypes.String);
            MessageSeen = (string)CheckAttribute(jsonObject, messageSeenKey, AttributeTypes.String);
            MessageParentUri = (string)CheckAttribute(jsonObject, messageParentUriKey, AttributeTypes.String);
        }

        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject[messageIdKey] = JsonValue.CreateStringValue(MessageId);
            jsonObject[messageSenderIdKey] = JsonValue.CreateNumberValue(MessageSenderId);
            jsonObject[messageTextKey] = JsonValue.CreateStringValue(MessageText);
            jsonObject[messageRecipientIdKey] = JsonValue.CreateNumberValue(MessageRecipientId);
            jsonObject[messageCreatedAtKey] = JsonValue.CreateStringValue(MessageCreatedAt);
            jsonObject[messageSenderScreenNameKey] = JsonValue.CreateStringValue(MessageSenderScreenName);
            jsonObject[messageRecipientScreenNameKey] = JsonValue.CreateStringValue(MessageRecipientScreenName);
            jsonObject[messageSenderKey] = MessageSender.ToJsonObject();
            jsonObject[messageRecipientKey] = MessageRecipient.ToJsonObject();
            jsonObject[messageTitleKey] = JsonValue.CreateStringValue(MessageTitle);
            jsonObject[messageSeenKey] = JsonValue.CreateStringValue(MessageSeen);
            jsonObject[messageParentUriKey] = JsonValue.CreateStringValue(MessageParentUri);
            return jsonObject.Stringify();
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaMessageObject = new JsonObject();
            friendicaMessageObject.SetNamedValue(messageIdKey, JsonValue.CreateStringValue(MessageId));
            friendicaMessageObject.SetNamedValue(messageSenderIdKey, JsonValue.CreateNumberValue(MessageSenderId));
            friendicaMessageObject.SetNamedValue(messageTextKey, JsonValue.CreateStringValue(MessageText));
            friendicaMessageObject.SetNamedValue(messageRecipientIdKey, JsonValue.CreateNumberValue(MessageRecipientId));
            friendicaMessageObject.SetNamedValue(messageCreatedAtKey, JsonValue.CreateStringValue(MessageCreatedAt));
            friendicaMessageObject.SetNamedValue(messageSenderScreenNameKey, JsonValue.CreateStringValue(MessageSenderScreenName));
            friendicaMessageObject.SetNamedValue(messageRecipientScreenNameKey, JsonValue.CreateStringValue(MessageRecipientScreenName));
            FriendicaUser friendicaSender = null;
            friendicaMessageObject.SetNamedValue(messageSenderKey, friendicaSender.ToJsonObject());
            FriendicaUser friendicaRecipient = null;
            friendicaMessageObject.SetNamedValue(messageRecipientKey, friendicaRecipient.ToJsonObject());
            friendicaMessageObject.SetNamedValue(messageTitleKey, JsonValue.CreateStringValue(MessageTitle));
            friendicaMessageObject.SetNamedValue(messageSeenKey, JsonValue.CreateStringValue(MessageSeen));
            friendicaMessageObject.SetNamedValue(messageParentUriKey, JsonValue.CreateStringValue(MessageParentUri));
            return friendicaMessageObject;
        }


        private int GetMessageIdInt(string idStr)
        {
            try
            {
                return Convert.ToInt16(idStr);
            }
            catch (FormatException)
            {
                return 0;
            }
        }

        private void SetIsReceivedMessage()
        {
            if (appSettings.FriendicaUsername != null)
            {
                if (MessageSenderScreenName != null && MessageSenderScreenName.ToLower() == appSettings.FriendicaUsername.ToLower() || MessageSenderScreenName == "Sample")
                    IsReceivedMessage = false;
                else if (MessageRecipientScreenName != null && MessageRecipientScreenName.ToLower() == appSettings.FriendicaUsername.ToLower() || MessageRecipientScreenName == "Sample")
                    IsReceivedMessage = true;
            }
        }

        public void ConvertHtmlToParagraph()
        {
            // placeholder text shown if there is no text in message (very rare case)
            if (MessageText == "")
            {
                var paragraph = new Paragraph();
                var noText = new Run();
                noText.Text = loader.GetString("paragraphMessagesNoContent");
                noText.FontStyle = Windows.UI.Text.FontStyle.Italic;
                noText.FontSize = 12;
                paragraph.Inlines.Add(noText);
                MessageTextTransformed = paragraph;
            }
            else
            {
                //var htmlToRichTextBlock = new clsHtmlToRichTextBlock(MessageText);
                //MessageTextTransformed = htmlToRichTextBlock.ApplyHtmlToParagraph();
            }
        }

        private async void ConvertHtmlToXml()
        {
            var htmlToXml = new clsHtmlToXml();
            MessageTextNotification = await htmlToXml.TransformHtmlToXml(MessageText);
        }

        private void SetCreatedAtNotification(DateTimeOffset createdAt)
        {
            var timeString = "";
            if (createdAt.Date == DateTime.Now.Date) // show date only if post is not of today
                timeString = loader.GetString("stringAtTime") + " " + createdAt.ToString("t");
            else
                timeString = loader.GetString("stringOnDate") + " " + createdAt.ToString("d") + " " + loader.GetString("stringAtTime") + " " + createdAt.ToString("t");
            MessageCreatedAtNotification = timeString;
        }


        public string MessageId
        {
            get { return _messageId; }
            set { _messageId = value;
                MessageIdInt = GetMessageIdInt(value);
            }
        }

        public double MessageSenderId
        {
            get { return _messageSenderId; }
            set { _messageSenderId = value; }
        }

        public string MessageText
        {
            get { return _messageText; }
            set { _messageText = value;
                ConvertHtmlToParagraph();
                ConvertHtmlToXml();
            }
        }

        public double MessageRecipientId
        {
            get { return _messageRecipientId; }
            set { _messageRecipientId = value; }
        }

        public string MessageCreatedAt
        {
            get { return _messageCreatedAt; }
            set { _messageCreatedAt = value;
                if (value != "")
                    MessageCreatedAtDateTime = DateTime.ParseExact(value, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture); }
        }

        public string MessageSenderScreenName
        {
            get { return _messageSenderScreenName; }
            set { _messageSenderScreenName = value;
                SetIsReceivedMessage();
            }
        }

        public string MessageRecipientScreenName
        {
            get { return _messageRecipientScreenName; }
            set { _messageRecipientScreenName = value;
                SetIsReceivedMessage();
            }
        }

        public FriendicaUser MessageSender
        {
            get { return _messageSender; }
            set { _messageSender = value; }
        }

        public FriendicaUser MessageRecipient
        {
            get { return _messageRecipient; }
            set { _messageRecipient = value; }
        }

        public string MessageTitle
        {
            get { return _messageTitle; }
            set { _messageTitle = value; }
        }

        public string MessageSeen
        {
            get { return _messageSeen; }
            set { _messageSeen = value; }
        }

        public string MessageParentUri
        {
            get { return _messageParentUri; }
            set { _messageParentUri = value; }
        }

    }
}
