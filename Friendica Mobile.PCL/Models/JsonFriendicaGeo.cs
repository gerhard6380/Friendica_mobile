using Newtonsoft.Json;
using System.Collections.Generic;

namespace Friendica_Mobile.PCL.Models
{
    public class JsonFriendicaGeo
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


        public JsonFriendicaGeo()
        {
            FriendicaGeoCoordinates = new List<double>();
        }
    }
}
