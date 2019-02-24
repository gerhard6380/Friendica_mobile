using Newtonsoft.Json;

namespace Friendica_Mobile.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaSite
    {
        [JsonProperty(PropertyName = "name")]
        public string SiteName { get; set; }

        [JsonProperty(PropertyName = "server")]
        public string SiteServer { get; set; }

        [JsonProperty(PropertyName = "theme")]
        public string SiteTheme { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string SitePath { get; set; }

        [JsonProperty(PropertyName = "logo")]
        public string SiteLogo { get; set; }

        [JsonProperty(PropertyName = "fancy")]
        public bool SiteFancy { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string SiteLanguage { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string SiteEmail { get; set; }

        [JsonProperty(PropertyName = "broughtby")]
        public string SiteBroughtby { get; set; }

        [JsonProperty(PropertyName = "broughtbyurl")]
        public string SiteBroughtbyurl { get; set; }

        [JsonProperty(PropertyName = "timezone")]
        public string SiteTimezone { get; set; }

        [JsonProperty(PropertyName = "closed")]
        public string SiteClosed { get; set; }

        [JsonProperty(PropertyName = "inviteonly")]
        public bool SiteInviteonly { get; set; }

        [JsonProperty(PropertyName = "private")]
        public string SitePrivate { get; set; }

        [JsonProperty(PropertyName = "textlimit")]
        public string SiteTextLimit { get; set; }

        [JsonProperty(PropertyName = "sslserver")]
        public string SiteSslserver { get; set; }

        [JsonProperty(PropertyName = "ssl")]
        public string SiteSsl { get; set; }

        [JsonProperty(PropertyName = "shorturllength")]
        public string SiteShorturllength { get; set; }

        [JsonProperty(PropertyName = "friendica")]
        public FriendicaServer SiteFriendica { get; set; }
    }
}
