using Newtonsoft.Json;

namespace Friendica_Mobile.Models
{
    public class JsonOEmbedYoutube
    {
        private string _thumbnailUrl;
        private string _providerUrl;
        private double? _thumbnailHeight;
        private string _type;
        private string _version;
        private string _providerName;
        private string _html;
        private double? _width;
        private double? _height;
        private string _authorName;
        private string _title;
        private double? _thumbnailWidth;
        private string _authorUrl;


        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl
        {
            get { return _thumbnailUrl; }
            set { _thumbnailUrl = value; }
        }

        [JsonProperty("provider_url")]
        public string ProviderUrl
        {
            get { return _providerUrl; }
            set { _providerUrl = value; }
        }

        [JsonProperty("thumbnail_height")]
        public double? ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set { _thumbnailHeight = value; }
        }

        [JsonProperty("type")]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        [JsonProperty("version")]
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        [JsonProperty("provider_name")]
        public string ProviderName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }

        [JsonProperty("html")]
        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        [JsonProperty("width")]
        public double? Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [JsonProperty("height")]
        public double? Height
        {
            get { return _height; }
            set { _height = value; }
        }

        [JsonProperty("author_name")]
        public string AuthorName
        {
            get { return _authorName; }
            set { _authorName = value; }
        }

        [JsonProperty("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        [JsonProperty("thumbnail_width")]
        public double? ThumbnailWidth
        {
            get { return _thumbnailWidth; }
            set { _thumbnailWidth = value; }
        }

        [JsonProperty("author_url")]
        public string AuthorUrl
        {
            get { return _authorUrl; }
            set { _authorUrl = value; }
        }

    }
}
