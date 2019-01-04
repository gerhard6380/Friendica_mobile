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
    public class FriendicaGroup
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser };

        private const string groupNameKey = "name";
        private const string groupGidKey = "gid";
        private const string groupUserKey = "user";

        private string _groupName;
        private double _groupGid;
        private List<FriendicaUser> _groupUser;

        public FriendicaGroup()
        {
            GroupName = "";
            GroupGid = 0;
            GroupUser = new List<FriendicaUser>();
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

        public FriendicaGroup(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            GroupName = (string)CheckAttribute(jsonObject, groupNameKey, AttributeTypes.String);
            GroupGid = (double)CheckAttribute(jsonObject, groupGidKey, AttributeTypes.Number);
            JsonArray array = jsonObject.GetNamedArray(groupUserKey);
            foreach (var element in array)
            {
                GroupUser.Add(new FriendicaUser(element.GetObject()));
            }
        }

        // manual filling of class instance with sample data for contacts page
        public FriendicaGroup(double sampleGid, string sampleName, List<FriendicaUser> sampleUser)
        {
            GroupGid = sampleGid;
            GroupName = sampleName;
            GroupUser = sampleUser;
        }


        public FriendicaGroup(JsonObject jsonObject) : this()
        {
            GroupName = (string)CheckAttribute(jsonObject, groupNameKey, AttributeTypes.String);
            GroupGid = (double)CheckAttribute(jsonObject, groupGidKey, AttributeTypes.Number);
            JsonArray array = jsonObject.GetNamedArray(groupUserKey);
            foreach (var element in array)
            {
                GroupUser.Add(new FriendicaUser(element.GetObject()));
            }
        }

        public string Stringify()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject[groupNameKey] = JsonValue.CreateStringValue(GroupName);
            jsonObject[groupGidKey] = JsonValue.CreateNumberValue(GroupGid);
            JsonArray jsonArray = new JsonArray();
            foreach (var user in GroupUser)
            {
                //jsonArray.Add(new FriendicaUser(user.Stringify()).ToJsonObject());
                jsonArray.Add(user.ToJsonObject());
            }
            jsonObject[groupUserKey] = jsonArray;
            return jsonObject.Stringify();
        }

        public JsonObject ToJsonObject()
        {
            JsonObject friendicaGroupObject = new JsonObject();
            friendicaGroupObject.SetNamedValue(groupNameKey, JsonValue.CreateStringValue(GroupName));
            friendicaGroupObject.SetNamedValue(groupGidKey, JsonValue.CreateNumberValue(GroupGid));
            return friendicaGroupObject;
        }

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; }
        }

        public double GroupGid
        {
            get { return _groupGid; }
            set { _groupGid = value; }
        }

        public List<FriendicaUser> GroupUser
        {
            get { return _groupUser; }
            set { _groupUser = value; }
        }

        public event EventHandler GroupDeleted;

        // react on delete button next to each group
        Mvvm.Command<FriendicaGroup> _deleteGroupCommand;
        public Mvvm.Command<FriendicaGroup> DeleteGroupCommand { get { return _deleteGroupCommand ?? (_deleteGroupCommand = new Mvvm.Command<FriendicaGroup>(ExecuteDeleteGroup)); } }

        private async void ExecuteDeleteGroup(FriendicaGroup group)
        {
            
            string errorMsg = String.Format(loader.GetString("messageDialogContactsDeleteGroup"), group.GroupName);
            var dialog = new MessageDialog(errorMsg);
            //var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
            dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
            dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
            dialog.DefaultCommandIndex = 1;
            dialog.CancelCommandIndex = 1;
            var result = await dialog.ShowAsync();
            if ((int)result.Id == 0)
            {
                if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                        || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
                {
                    var messageDialog = new MessageDialogMessage(loader.GetString("messageDialogGroupChangeSamples"), "", loader.GetString("buttonYes"), "");
                    await messageDialog.ShowDialog(0,0);
                }
                else
                {
                    // Delete group
                    var deleteGroup = new GetFriendicaGroups();
                    deleteGroup.FriendicaGroupsLoaded += DeleteGroup_FriendicaGroupsLoaded;
                    deleteGroup.FriendicaGroupDelete(group.GroupGid, group.GroupName);
                }
            }
            else
            {
                // group not deleted no further action
            }
        }

        private async void DeleteGroup_FriendicaGroupsLoaded(object sender, EventArgs e)
        {
            var deleteGroup = sender as GetFriendicaGroups;

            if (deleteGroup.ReturnString.Contains("error"))
            {
                string err = "";
                if (deleteGroup.ReturnString.Contains("gid or name not specified"))
                    err = "gid or name not specified";
                else if (deleteGroup.ReturnString.Contains("gid not available"))
                    err = "gid not available";
                else if (deleteGroup.ReturnString.Contains("wrong group name"))
                    err = "wrong group name";
                else if (deleteGroup.ReturnString.Contains("other API error"))
                    err = "other API error";
                string errorMsg = String.Format("Delete group: server return error '{0}'", err);
                var dialog = new MessageDialog(errorMsg);
                dialog.Commands.Add(new UICommand("OK", null, 0));
                var result = await dialog.ShowAsync();
            }
            else
            {
                if (GroupDeleted != null)
                    GroupDeleted(this, EventArgs.Empty);
            }
        }

        public event EventHandler GroupChange;

        // react on delete button next to each group
        Mvvm.Command<FriendicaGroup> _changeGroupCommand;
        public Mvvm.Command<FriendicaGroup> ChangeGroupCommand { get { return _changeGroupCommand ?? (_changeGroupCommand = new Mvvm.Command<FriendicaGroup>(ExecuteChangeGroup)); } }

        private void ExecuteChangeGroup(FriendicaGroup group)
        {
            if (GroupChange != null)
                GroupChange(this, EventArgs.Empty);
        }

    }
}