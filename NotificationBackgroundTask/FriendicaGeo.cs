using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BackgroundTasks
{
    public sealed class FriendicaGeo
    {
        private const string friendicaGeoTypeKey = "type";
        private const string friendicaGeoCoordinatesKey = "coordinates";

        private string _friendicaGeoType;
        private IList<double> _friendicaGeoCoordinates;
 
        public FriendicaGeo()
        {
            FriendicaGeoType = "";
            FriendicaGeoCoordinates = new List<double>();
        }

        public FriendicaGeo(JsonObject jsonObject)
        {
            FriendicaGeoCoordinates = new List<double>();
            FriendicaGeoType = jsonObject.GetNamedString(friendicaGeoTypeKey);
            JsonArray array = jsonObject.GetNamedArray(friendicaGeoCoordinatesKey);
            foreach (var element in array)
            {
                FriendicaGeoCoordinates.Add(element.GetNumber());
            }
                
            //FriendicaGeoCoordinates = jsonObject.GetNamedString(friendicaGeoCoordinatesKey);
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaGeoObject = new JsonObject();
            friendicaGeoObject.SetNamedValue(friendicaGeoTypeKey, JsonValue.CreateStringValue(FriendicaGeoType));
            //friendicaGeoObject.SetNamedValue(friendicaGeoCoordinatesKey, JsonValue.CreateStringValue(FriendicaGeoCoordinates));
            return friendicaGeoObject;
        }

        public string FriendicaGeoType
        {
            get
            {
                return _friendicaGeoType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _friendicaGeoType = value;
            }
        }

        public IList<double> FriendicaGeoCoordinates
        {
            get
            {
                return _friendicaGeoCoordinates;
            }
            set
            {
                _friendicaGeoCoordinates = value;
            }
        }
    }
}
