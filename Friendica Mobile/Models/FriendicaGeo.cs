using Newtonsoft.Json;
using System.Collections.Generic;

namespace Friendica_Mobile.Models
{
    public class FriendicaGeo
    {
        private string _friendicaGeoType;
        private List<double> _friendicaGeoCoordinates;


        [JsonProperty("type")]
        public string FriendicaGeoType
        {
            get { return _friendicaGeoType; }
            set { _friendicaGeoType = value; }
        }

        [JsonProperty("coordinates")]
        public List<double> FriendicaGeoCoordinates
        {
            get { return _friendicaGeoCoordinates; }
            set { _friendicaGeoCoordinates = value; }
        }


        public FriendicaGeo()
        {
            FriendicaGeoCoordinates = new List<double>();
        }
    }
}
