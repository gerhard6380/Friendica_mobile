using System.Collections.Generic;
using System.Windows.Input;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Friendica_Mobile.Models
{
    public class FriendicaGroupResult
    {
        [JsonProperty(PropertyName = "success")]
        public bool GroupSuccess { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public int GroupGid { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string GroupStatus { get; set; }

        [JsonProperty(PropertyName = "wrong users")]
        public List<FriendicaUser> GroupWrongUsers { get; set; }
    }
}