using System.Collections.Generic;
using Windows.Data.Json;

namespace Friendica_Mobile.UWP.Models
{
    public abstract class FriendicaBaseModel
    {
        public enum AttributeTypes { String, Number, Boolean, StringArray, FriendicaPost, FriendicaUser, FriendicaGeo, FriendicaActivities, FriendicaPhotoComments };

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
                case AttributeTypes.FriendicaActivities:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaActivities(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaPhotoComments:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else if (value.ValueType == JsonValueType.Object)
                        return null;
                    else
                    {
                        List<FriendicaPostExtended> list = new List<FriendicaPostExtended>();
                        JsonArray array = jsonObject.GetNamedArray(key);
                        foreach (JsonValue item in array)
                        {
                            var post = new FriendicaPostExtended();
                            post.Post = new FriendicaPost(item.GetObject());
                            list.Add(post);
                        }
                        return list;
                    }
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
