using Friendica_Mobile.UWP.HttpRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace Friendica_Mobile.UWP.Models
{
    public class FriendicaGroupResult
    {
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser };

        private const string groupSuccessKey = "success";
        private const string groupGidKey = "gid";
        private const string groupNameKey = "name";
        private const string groupStatusKey = "status";
        private const string groupWrongUsersKey = "wrong users";

        private bool _groupSuccess;
        private double _groupGid;
        private string _groupName;
        private string _groupStatus;
        private List<FriendicaUser> _groupWrongUsers;

        public FriendicaGroupResult()
        {
            GroupSuccess = false;
            GroupGid = 0;
            GroupName = "";
            GroupStatus = "";
            GroupWrongUsers = new List<FriendicaUser>();
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

        public FriendicaGroupResult(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            GroupSuccess = (bool)CheckAttribute(jsonObject, groupSuccessKey, AttributeTypes.Boolean);
            GroupGid = (double)CheckAttribute(jsonObject, groupGidKey, AttributeTypes.Number);
            GroupName = (string)CheckAttribute(jsonObject, groupNameKey, AttributeTypes.String);
            GroupStatus = (string)CheckAttribute(jsonObject, groupStatusKey, AttributeTypes.String);
            JsonArray array = jsonObject.GetNamedArray(groupWrongUsersKey);
            foreach (var element in array)
            {
                GroupWrongUsers.Add(new FriendicaUser(element.GetObject()));
            }
        }

        public FriendicaGroupResult(JsonObject jsonObject) : this()
        {
            GroupSuccess = (bool)CheckAttribute(jsonObject, groupSuccessKey, AttributeTypes.Boolean);
            GroupGid = (double)CheckAttribute(jsonObject, groupGidKey, AttributeTypes.Number);
            GroupName = (string)CheckAttribute(jsonObject, groupNameKey, AttributeTypes.String);
            GroupStatus = (string)CheckAttribute(jsonObject, groupStatusKey, AttributeTypes.String);
            JsonArray array = jsonObject.GetNamedArray(groupWrongUsersKey);
            foreach (var element in array)
            {
                GroupWrongUsers.Add(new FriendicaUser(element.GetObject()));
            }
        }

        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject[groupSuccessKey] = JsonValue.CreateBooleanValue(GroupSuccess);
            jsonObject[groupGidKey] = JsonValue.CreateNumberValue(GroupGid);
            jsonObject[groupNameKey] = JsonValue.CreateStringValue(GroupName);
            jsonObject[groupStatusKey] = JsonValue.CreateStringValue(GroupStatus);
            JsonArray jsonArray = new JsonArray();
            foreach (var user in GroupWrongUsers)
            {
                //jsonArray.Add(new FriendicaUser(user.Stringify()).ToJsonObject());
                jsonArray.Add(user.ToJsonObject());
            }
            jsonObject[groupWrongUsersKey] = jsonArray;
            return jsonObject.Stringify();
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaGroupObject = new JsonObject();
            friendicaGroupObject.SetNamedValue(groupSuccessKey, JsonValue.CreateBooleanValue(GroupSuccess));
            friendicaGroupObject.SetNamedValue(groupGidKey, JsonValue.CreateNumberValue(GroupGid));
            friendicaGroupObject.SetNamedValue(groupNameKey, JsonValue.CreateStringValue(GroupName));
            friendicaGroupObject.SetNamedValue(groupStatusKey, JsonValue.CreateStringValue(GroupStatus));
            return friendicaGroupObject;
        }

        public bool GroupSuccess
        {
            get { return _groupSuccess; }
            set { _groupSuccess = value; }
        }

        public double GroupGid
        {
            get { return _groupGid; }
            set { _groupGid = value; }
        }

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }

        public string GroupStatus
        {
            get { return _groupStatus; }
            set { _groupStatus = value; }
        }

        public List<FriendicaUser> GroupWrongUsers
        {
            get { return _groupWrongUsers; }
            set { _groupWrongUsers = value; }
        }


    }
}