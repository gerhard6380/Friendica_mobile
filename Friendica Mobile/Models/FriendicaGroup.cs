using System.Collections.Generic;
using Newtonsoft.Json;

namespace Friendica_Mobile.Models
{
    public class FriendicaGroup
    {
        [JsonProperty(PropertyName = "name")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public int GroupGid { get; set; }

        [JsonProperty(PropertyName = "user")]
        public List<FriendicaUser> GroupUser { get; set; }

    }
}