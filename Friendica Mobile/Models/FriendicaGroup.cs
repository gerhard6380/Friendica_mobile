using System;
using System.Collections.Generic;
using System.Windows.Input;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Strings;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Friendica_Mobile.Models
{
    public class FriendicaGroup
    {
        [JsonProperty(PropertyName = "name")]
        public string GroupName { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public int GroupGid { get; set; }

        [JsonProperty(PropertyName = "user")]
        public List<FriendicaUser> GroupUser { get; set; }

        public event EventHandler GroupDeleted;

        private ICommand _deleteGroupCommand;
        [JsonIgnore]
        public ICommand DeleteGroupCommand => _deleteGroupCommand ?? (_deleteGroupCommand = new Command(DeleteGroup));
        private async void DeleteGroup()
        {
            var answer = await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                         String.Format(AppResources.messageDialogContactsDeleteGroup, GroupName),
                                                                         AppResources.buttonYes, AppResources.buttonNo);
            if (answer)
            {
                if (!Settings.IsFriendicaLoginDefined())
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                    AppResources.messageDialogGroupChangeSamples,
                                                                    AppResources.buttonOK);
                }
                else
                {
                    var groupDeleteResults = await App.Contacts.DeleteFriendicaGroupDeleteAsync(GroupGid, GroupName);
                    // let's work on the results
                    switch (groupDeleteResults)
                    {
                        case HttpFriendicaContacts.DeleteFriendicaGroupResults.ErrorReceived:
                            await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                      String.Format(AppResources.MessageDialogGroupDeleteError, App.Contacts.ErrorGroupDelete),
                                                                      AppResources.buttonOK);
                            break;
                        case HttpFriendicaContacts.DeleteFriendicaGroupResults.NotAnswered:
                            await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                    AppResources.TestConnectionResultNotAnswered,
                                                                    AppResources.buttonOK);
                            break;
                        case HttpFriendicaContacts.DeleteFriendicaGroupResults.OK:
                            GroupDeleted?.Invoke(this, EventArgs.Empty);
                            break;
                        case HttpFriendicaContacts.DeleteFriendicaGroupResults.NotAuthenticated:
                        case HttpFriendicaContacts.DeleteFriendicaGroupResults.NotImplemented:
                        case HttpFriendicaContacts.DeleteFriendicaGroupResults.UnexpectedError:
                        default:
                            await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                    String.Format(AppResources.messageDialogManageGroupErrorResult, App.Contacts.StatusCode, App.Contacts.ReturnString),
                                                                    AppResources.buttonOK);
                            break;
                    }
                }
            }
            // group not to be deleted, no further action needed in else routine
        }
    }
}