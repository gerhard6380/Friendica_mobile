using System.Collections.Generic;
using Newtonsoft.Json;

namespace Friendica_Mobile.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaConfig
    {
        [JsonProperty(PropertyName = "site")]
        public FriendicaSite ConfigSite { get; set; }

    }
}
