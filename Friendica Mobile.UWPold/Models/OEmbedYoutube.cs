using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.UWP.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class OEmbedYoutube
    {
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser };

        private const string thumbnailUrlKey = "thumbnail_url";
        private const string providerUrlKey = "provider_url";
        private const string thumbnailHeightKey = "thumbnail_height";
        private const string typeKey = "type";
        private const string versionKey = "version";
        private const string providerNameKey = "provider_name";
        private const string htmlKey = "html";
        private const string widthKey = "width";
        private const string heightKey = "height";
        private const string authorNameKey = "author_name";
        private const string titleKey = "title";
        private const string thumbnailWidthKey = "thumbnail_width";
        private const string authorUrlKey = "author_url";

        private string _thumbnailUrl;
        private string _providerUrl;
        private double _thumbnailHeight;
        private string _type;
        private string _version;
        private string _providerName;
        private string _html;
        private double _width;
        private double _height;
        private string _authorName;
        private string _title;
        private double _thumbnailWidth;
        private string _authorUrl;

        public OEmbedYoutube()
        {
            ThumbnailUrl = "";
            ProviderUrl = "";
            ThumbnailHeight = 0;
            Type = "";
            Version = "";
            ProviderName = "";
            Html = "";
            Width = 0;
            Height = 0;
            AuthorName = "";
            Title = "";
            ThumbnailWidth = 0;
            AuthorUrl = "";
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
        
        public OEmbedYoutube(string jsonString)
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);

            ThumbnailUrl = (string)CheckAttribute(jsonObject, thumbnailUrlKey, AttributeTypes.String);
            ProviderUrl = (string)CheckAttribute(jsonObject, providerUrlKey, AttributeTypes.String);
            ThumbnailHeight = (double)CheckAttribute(jsonObject, thumbnailHeightKey, AttributeTypes.Number);
            Type = (string)CheckAttribute(jsonObject, typeKey, AttributeTypes.String);
            Version = (string)CheckAttribute(jsonObject, versionKey, AttributeTypes.String);
            ProviderName = (string)CheckAttribute(jsonObject, providerNameKey, AttributeTypes.String);
            Html = (string)CheckAttribute(jsonObject, htmlKey, AttributeTypes.String);
            Width = (double)CheckAttribute(jsonObject, widthKey, AttributeTypes.Number);
            Height = (double)CheckAttribute(jsonObject, heightKey, AttributeTypes.Number);
            AuthorName = (string)CheckAttribute(jsonObject, authorNameKey, AttributeTypes.String);
            Title = (string)CheckAttribute(jsonObject, titleKey, AttributeTypes.String);
            ThumbnailWidth = (double)CheckAttribute(jsonObject, thumbnailWidthKey, AttributeTypes.Number);
            AuthorUrl = (string)CheckAttribute(jsonObject, authorUrlKey, AttributeTypes.String);
        }


        public JsonObject ToJsonObject()
        {
            JsonObject oEmbedYoutubeObject = new JsonObject();
            oEmbedYoutubeObject.SetNamedValue(thumbnailUrlKey, JsonValue.CreateStringValue(ThumbnailUrl));
            oEmbedYoutubeObject.SetNamedValue(providerUrlKey, JsonValue.CreateStringValue(ProviderUrl));
            oEmbedYoutubeObject.SetNamedValue(thumbnailHeightKey, JsonValue.CreateNumberValue(ThumbnailHeight));
            oEmbedYoutubeObject.SetNamedValue(typeKey, JsonValue.CreateStringValue(Type));
            oEmbedYoutubeObject.SetNamedValue(versionKey, JsonValue.CreateStringValue(Version));
            oEmbedYoutubeObject.SetNamedValue(providerNameKey, JsonValue.CreateStringValue(ProviderName));
            oEmbedYoutubeObject.SetNamedValue(htmlKey, JsonValue.CreateStringValue(Html));
            oEmbedYoutubeObject.SetNamedValue(widthKey, JsonValue.CreateNumberValue(Width));
            oEmbedYoutubeObject.SetNamedValue(heightKey, JsonValue.CreateNumberValue(Height));
            oEmbedYoutubeObject.SetNamedValue(authorNameKey, JsonValue.CreateStringValue(AuthorName));
            oEmbedYoutubeObject.SetNamedValue(titleKey, JsonValue.CreateStringValue(Title));
            oEmbedYoutubeObject.SetNamedValue(thumbnailWidthKey, JsonValue.CreateNumberValue(ThumbnailWidth));
            oEmbedYoutubeObject.SetNamedValue(authorUrlKey, JsonValue.CreateStringValue(AuthorUrl));

            return oEmbedYoutubeObject;
        }

        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set { _thumbnailUrl = value; }
        }

        public string ProviderUrl
        {
            get { return _providerUrl; }
            set { _providerUrl = value; }
        }

        public double ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set { _thumbnailHeight = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public string ProviderName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }

        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        public double Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public double Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public string AuthorName
        {
            get { return _authorName; }
            set { _authorName = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public double ThumbnailWidth
        {
            get { return _thumbnailWidth; }
            set { _thumbnailWidth = value; }
        }

        public string AuthorUrl
        {
            get { return _authorUrl; }
            set { _authorUrl = value; }
        }

    }
}
