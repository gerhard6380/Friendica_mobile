using System.Collections.Generic;
using Windows.Data.Json;

namespace Friendica_Mobile.Models
{
    public abstract class FriendicaBaseModel
    {
        public enum AttributeTypes { String, Number, Boolean, StringArray, FriendicaPost, FriendicaUser, FriendicaGeo };

        protected virtual object CheckAttribute(JsonObject jsonObject, string key, AttributeTypes type)
        {
            IJsonValue value;
            jsonObject.TryGetValue(key, out value);
            switch (type)
            {
                case AttributeTypes.String:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedString(key, "");
                case AttributeTypes.Number:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return 0.0;
                    else
                        return jsonObject.GetNamedNumber(key, 0);
                case AttributeTypes.Boolean:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedBoolean(key, false);
                case AttributeTypes.StringArray:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else if (value.ValueType == JsonValueType.Object)
                    {
                        return null;
                    }
                    else
                    {
                        List<string> list = new List<string>();
                        JsonArray array = jsonObject.GetNamedArray(key);
                        foreach (JsonValue link in array)
                            list.Add(link.GetString());
                        return list;
                    }
                case AttributeTypes.FriendicaPost:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaPost(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaGeo:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaGeo(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaUser:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaUser(jsonObject.GetNamedObject(key));
                default:
                    return null;
            }
        }

        protected virtual List<FriendicaUserExtended> ConvertJsonToObjects(object type, JsonArray array)
        {
            if (array == null)
                return null;

            var list = new List<FriendicaUserExtended>();
            int arraySize = array.Count;
            for (int i = 0; i < arraySize; i++)
            {
                IJsonValue element = array.GetArray()[i];
                switch (element.ValueType)
                {
                    case JsonValueType.Object:
                        FriendicaUserExtended result = null;
                        if (type.GetType() == typeof(FriendicaUserExtended))
                            result = new FriendicaUserExtended(element.ToString());
                        list.Add(result);
                        break;
                }
            }
            return list;
        }

    }
}
