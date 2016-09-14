using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage.Streams;

namespace BackgroundTasks
{
    public sealed class FriendicaMessageNew
    {
        // used to post a new private message to another friend - currently not possible to post private images

        private enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser, FriendicaGeo };

        private const string newMessageUserIdKey = "user_id";
        private const string newMessageTextKey = "text";
        private const string newMessageReplyToKey = "replyto";
        private const string newMessageTitleKey = "title";

        private string _newMessageUserUrl;
        private string _newMessageText;
        private string _newMessageReplyTo;
        private string _newMessageTitle;
        //private byte[] _newMessageMedia;

        public FriendicaMessageNew()
        {
            NewMessageUserUrl = "";
            NewMessageText = "";
            NewMessageReplyTo = "";
            NewMessageTitle = "";
            //NewMessageMedia = null;
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
                default:
                    return null;
            } 
        }

        public string NewMessageUserUrl
        {
            get { return _newMessageUserUrl; }
            set { _newMessageUserUrl = value; }
        }

        public string NewMessageText
        {
            get { return _newMessageText; }
            set { _newMessageText = value; }
        }

        public string NewMessageReplyTo
        {
            get { return _newMessageReplyTo; }
            set { _newMessageReplyTo = value; }
        }

        public string NewMessageTitle
        {
            get { return _newMessageTitle; }
            set { _newMessageTitle = value; }
        }

        //public byte[] NewMessageMedia
        //{
        //    get { return _newMessageMedia; }
        //    set { _newMessageMedia = value; }
        //}

    }
}
