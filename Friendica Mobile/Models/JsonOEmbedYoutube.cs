using Newtonsoft.Json;

namespace Friendica_Mobile.Models
{
    public class JsonOEmbedYoutube
    {
        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("provider_url")]
        public string ProviderUrl { get; set; }

        [JsonProperty("thumbnail_height")]
        public double? ThumbnailHeight { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("provider_name")]
        public string ProviderName { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }

        [JsonProperty("width")]
        public double? Width { get; set; }

        [JsonProperty("height")]
        public double? Height { get; set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("thumbnail_width")]
        public double? ThumbnailWidth { get; set; }

        [JsonProperty("author_url")]
        public string AuthorUrl { get; set; }

    }
}
