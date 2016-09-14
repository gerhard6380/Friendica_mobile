using Friendica_Mobile.HttpRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace Friendica_Mobile.Models
{
    public class FriendicaErrorResult
    {
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser };

        private const string errorResultKey = "result";
        private const string errorMessageKey = "message";

        private string _errorResult;
        private string _errorMessage;

        public FriendicaErrorResult()
        {
            ErrorResult = "";
            ErrorMessage = "";
        }

        private object CheckAttribute(JsonObject jsonObject, string key, AttributeTypes type)
        {
            IJsonValue value;
            jsonObject.TryGetValue(key, out value);
            switch (type)
            {
                case AttributeTypes.String:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                    {
                        // es gibt Fälle, wo ein String-Feld als integer im Json-String enthalten ist (user/id), darauf reagieren
                        if (value.ValueType == JsonValueType.String)
                            return jsonObject.GetNamedString(key, "");
                        else if (value.ValueType == JsonValueType.Number)
                            return jsonObject.GetNamedNumber(key, 0.0).ToString();
                        else
                            return null;
                    }
                        
                case AttributeTypes.Number:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return 0.0;
                    else
                    {
                        if (value.ValueType == JsonValueType.Number)
                            return jsonObject.GetNamedNumber(key, 0);
                        else if (value.ValueType == JsonValueType.String)
                            return Double.Parse(jsonObject.GetNamedString(key, ""));
                        else
                            return 0.0;
                    }
                case AttributeTypes.Boolean:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedBoolean(key, false);
                case AttributeTypes.FriendicaUser:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaUser(jsonObject.GetNamedObject(key));
                default:
                    return null;
            }
        }

        public FriendicaErrorResult(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            ErrorResult = (string)CheckAttribute(jsonObject, errorResultKey, AttributeTypes.String);
            ErrorMessage = (string)CheckAttribute(jsonObject, errorMessageKey, AttributeTypes.String);
        }

        public FriendicaErrorResult(JsonObject jsonObject) : this()
        {
            ErrorResult = (string)CheckAttribute(jsonObject, errorResultKey, AttributeTypes.String);
            ErrorMessage = (string)CheckAttribute(jsonObject, errorMessageKey, AttributeTypes.String);
        }


        public JsonObject ToJsonObject()
        {
            JsonObject friendicaGroupObject = new JsonObject();
            friendicaGroupObject.SetNamedValue(errorResultKey, JsonValue.CreateStringValue(ErrorResult));
            friendicaGroupObject.SetNamedValue(errorMessageKey, JsonValue.CreateStringValue(ErrorMessage));
            return friendicaGroupObject;
        }

        public string ErrorResult
        {
            get { return _errorResult; }
            set { _errorResult = value; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

    }
}