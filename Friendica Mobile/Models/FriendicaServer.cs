using System;
using Newtonsoft.Json;

namespace Friendica_Mobile.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaServer
    {
        [JsonProperty(PropertyName = "FRIENDICA_PLATFORM")]
        public string FriendicaPlatform { get; set; }

        [JsonProperty(PropertyName = "FRIENDICA_VERSION")]
        public string FriendicaVersion { get; set; }

        [JsonProperty(PropertyName = "DFRN_PROTOCOL_VERSION")]
        public string DfrnProtocolVersion { get; set; }

        [JsonProperty(PropertyName = "DB_UPDATE_VERSION")]
        public string DbUpdateVersion { get; set; }
    }
}
