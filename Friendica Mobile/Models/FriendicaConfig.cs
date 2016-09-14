using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaConfig
    {
        private const string configSiteKey = "site";

        private FriendicaSite _configSite;

        public FriendicaConfig()
        {
            ConfigSite = new FriendicaSite();
        }

        public FriendicaConfig(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            ConfigSite = new FriendicaSite(jsonObject.GetNamedObject(configSiteKey));
        }

        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject[configSiteKey] = ConfigSite.ToJsonObject();

            return jsonObject.Stringify();
        }

        public FriendicaSite ConfigSite
        {
            get { return _configSite; }
            set { _configSite = value; }
        }

    }
}
