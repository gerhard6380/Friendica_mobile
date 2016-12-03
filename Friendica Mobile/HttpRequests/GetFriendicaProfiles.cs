using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using Windows.Data.Json;

namespace Friendica_Mobile.HttpRequests
{
    class GetFriendicaProfiles : clsHttpRequests
    {
        AppSettings appSettings = new AppSettings();
        public enum ProfileErrors { OK, NoServerSupport, AuthenticationFailed, ProfileIdNotFound, UnknownError };

        public string ProfileId { get; set; }
        public bool MultiProfiles { get; set; }
        public string GlobalDirectory { get; set; }
        public FriendicaUser FriendicaOwner { get; set; }
        public List<FriendicaProfile> ProfilesReturned { get; set; }
        public bool IsErrorOccurred { get; set; }
        public ProfileErrors ErrorProfileFriendica { get; set; }

        public event EventHandler FriendicaProfilesLoaded;
        protected virtual void OnFriendicaProfilesLoaded()
        {
            FriendicaProfilesLoaded?.Invoke(this, EventArgs.Empty);
        }
        

        public GetFriendicaProfiles()
        {
        }


        public async void LoadProfiles()
        {
            var url = String.Format("{0}/api/friendica/profile/show.json?timestamp={1}",
                appSettings.FriendicaServer,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaProfiles_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        private void GetFriendicaProfiles_RequestFinished(object sender, EventArgs e)
        {
            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.Ok:
                    IsErrorOccurred = false;
                    JsonObject jsonObject = JsonObject.Parse(this.ReturnString);
                    try
                    {
                        MultiProfiles = jsonObject.GetNamedBoolean("multi_profiles");
                        GlobalDirectory = jsonObject.GetNamedString("global_dir");
                        FriendicaOwner = new FriendicaUser(jsonObject.GetNamedObject("friendica_owner"));
                        ProfilesReturned = ConvertJsonToObjects(jsonObject.GetNamedArray("profiles")); 
                    }
                    catch (Exception ex)
                    {

                    }
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorProfileFriendica = ProfileErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorProfileFriendica = ProfileErrors.AuthenticationFailed;
                    break;
                case Windows.Web.Http.HttpStatusCode.BadRequest:
                    // api returned 400 BadRequest because given profile-id was wrong
                    IsErrorOccurred = true;
                    ErrorProfileFriendica = ProfileErrors.ProfileIdNotFound;
                    break;
                default:
                    IsErrorOccurred = true;
                    ErrorProfileFriendica = ProfileErrors.UnknownError;
                    break;
            }
            OnFriendicaProfilesLoaded();
        }


        private List<FriendicaProfile> ConvertJsonToObjects(JsonArray array = null)
        {
            JsonArray resultArray;

            if (array != null)
                resultArray = array;
            else
                resultArray = JsonArray.Parse(this.ReturnString);

            var list = new List<FriendicaProfile>();
            int arraySize = resultArray.Count;
            for (int i = 0; i < arraySize; i++)
            {
                IJsonValue element = resultArray.GetArray()[i];
                switch (element.ValueType)
                {
                    case JsonValueType.Object:
                        var result = new FriendicaProfile(element.ToString());
                        list.Add(result);
                        break;
                }
            }
            return list;
        }

    }
}
