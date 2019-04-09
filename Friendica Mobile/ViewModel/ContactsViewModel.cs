using System.Threading.Tasks;
using System.Windows.Input;
using Friendica_Mobile.Views;
using Friendica_Mobile.Strings;
using Xamarin.Forms;
using Friendica_Mobile.HttpRequests;
using System;
using System.Collections.ObjectModel;
using Friendica_Mobile.Models;
using MvvmHelpers;
using System.Linq;
using System.Collections.Generic;

namespace Friendica_Mobile.ViewModel
{
    public class ContactsViewModel : BaseViewModel
    {
        private bool _refreshContactsEnabled = true;
        public bool RefreshContactsEnabled
        {
            get { return _refreshContactsEnabled; }
            set { SetProperty(ref _refreshContactsEnabled, value); 
            }
        }

        public bool NoSettings
        {
            get { return !Settings.IsFriendicaLoginDefined(); }
        }

        private bool _noDataAvailableFriends;
        public bool NoDataAvailableFriends
        {
            get { return _noDataAvailableFriends; }
            set { SetProperty(ref _noDataAvailableFriends, value); }
        }

        private bool _noDataAvailableForums;
        public bool NoDataAvailableForums
        {
            get { return _noDataAvailableForums; }
            set { SetProperty(ref _noDataAvailableForums, value); }
        }

        private bool _noDataAvailableGroups;
        public bool NoDataAvailableGroups
        {
            get { return _noDataAvailableGroups; }
            set { SetProperty(ref _noDataAvailableGroups, value); }
        }

        private string _searchFriends;
        public string SearchFriends
        {
            get { return _searchFriends; }
            set { SetProperty(ref _searchFriends, value);
                FilteringFriends();
            }
        }

        private string _searchForums;
        public string SearchForums
        {
            get { return _searchForums; }
            set
            {
                SetProperty(ref _searchForums, value);
                FilteringForums();
            }
        }

        public ObservableRangeCollection<FriendicaUser> FilteredFriends { get; set; }
        public ObservableRangeCollection<FriendicaUser> FilteredForums { get; set; }
        public ObservableRangeCollection<FriendicaGroup> FilteredGroups { get; set; }

        private bool _isGroupEditorVisible;
        public bool IsGroupEditorVisible
        {
            get { return _isGroupEditorVisible; }
            set { SetProperty(ref _isGroupEditorVisible, value);
            }
        }

        private bool _isGroupEditorInChangeMode;
        public bool IsGroupEditorInChangeMode
        {
            get { return _isGroupEditorInChangeMode; }
            set
            {
                SetProperty(ref _isGroupEditorInChangeMode, value);
                OnPropertyChanged("GroupEditorHeader");
            }
        }

        public string GroupEditorHeader
        {
            get { return (IsGroupEditorInChangeMode) ? AppResources.pageTitleChangeGroup_Text : AppResources.pageTitleCreateGroup_Text; }
        }

        private FriendicaGroup _groupInEditor;
        public FriendicaGroup GroupInEditor
        {
            get { return _groupInEditor; }
            set
            {
                SetProperty(ref _groupInEditor, value);
                if (value != null)
                    GroupName = _groupInEditor.GroupName;
            }
        }

        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set
            {
                SetProperty(ref _groupName, value);
                SetIsGroupSaveEnabled();
            }
        }

        private ObservableCollection<SelectableData<FriendicaUser>> _friendsInGroup;
        public ObservableCollection<SelectableData<FriendicaUser>> FriendsInGroup
        {
            get { return _friendsInGroup; }
            set { SetProperty(ref _friendsInGroup, value); }
        }

        private bool _isGroupSaveEnabled;
        public bool IsGroupSaveEnabled
        {
            get { return _isGroupSaveEnabled; }
            set
            {
                SetProperty(ref _isGroupSaveEnabled, value);
            }
        }

#region Commands
        private ICommand _navigateSettingsCommand;
        public ICommand NavigateSettingsCommand => _navigateSettingsCommand ?? (_navigateSettingsCommand = new Command(NavigateSettings));
        private void NavigateSettings()
        {
            NavigateTo(new Views.Settings());
        }

        private ICommand _refreshContactsCommand;
        public ICommand RefreshContactsCommand => _refreshContactsCommand ?? (_refreshContactsCommand = new Command(RefreshContacts));
        private void RefreshContacts()
        {
            ReloadContactsData();
        }

        private ICommand _findLocalCommand;
        public ICommand FindLocalCommand => _findLocalCommand ?? (_findLocalCommand = new Command(FindLocal));
        private void FindLocal()
        {
            Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                             AppResources.messageDialogContactsFindLocalNotAvailable, "OK");
        }

        private ICommand _findGlobalCommand;
        public ICommand FindGlobalCommand => _findGlobalCommand ?? (_findGlobalCommand = new Command(FindGlobal));
        private void FindGlobal()
        {
            Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                             AppResources.messageDialogContactsFindGlobalNotAvailable, "OK");
        }

        private ICommand _createNewGroupCommand;
        public ICommand CreateNewGroupCommand => _createNewGroupCommand ?? (_createNewGroupCommand = new Command(CreateNewGroup));
        private void CreateNewGroup()
        {
            // set parameters to display the form for a new group
            IsGroupEditorVisible = true;
            IsGroupEditorInChangeMode = false;
            GroupInEditor = new FriendicaGroup();
            CreateGroupEditorListView();
        }


        private ICommand _groupEditorSaveCommand;
        public ICommand GroupEditorSaveCommand => _groupEditorSaveCommand ?? (_groupEditorSaveCommand = new Command(GroupEditorSave));
        private async void GroupEditorSave()
        {
            // info that we cannot save something on server in demo mode
            if (!Settings.IsFriendicaLoginDefined())
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                          AppResources.messageDialogGroupChangeSamples, "OK");
                return;
            }

            // ask user if ok to save a group without users selected (only in changing mode, as for new groups it is not possible to save the group without users)
            if (!FriendsInGroup.Any(u => u.IsSelected))
            {
                var answer = await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                         AppResources.messageDialogWarningOnEmptyUserlist,
                                                          AppResources.buttonYes, AppResources.buttonNo);
                if (!answer)
                    return;
            }

            // now let's prepare the group for saving
            var groupNew = new FriendicaGroup();
            groupNew.GroupName = GroupName;
            var selectedUsers = FriendsInGroup.Where(u => u.IsSelected).ToList();
            groupNew.GroupUser = new List<FriendicaUser>();
            foreach (var user in selectedUsers)
                groupNew.GroupUser.Add(user.Data);

            // now we can send the data to server
            ActivityIndicatorText = AppResources.textblockIndicatorSavingGroup_Text;
            ActivityIndicatorVisible = true;
            HttpFriendicaContacts.PostFriendicaGroupsResults groupsResults;
            if (IsGroupEditorInChangeMode)
            {
                groupNew.GroupGid = GroupInEditor.GroupGid;
                groupsResults = await App.Contacts.PostFriendicaGroupUpdateAsync(groupNew);
            }
            else
                groupsResults = await App.Contacts.PostFriendicaGroupCreateAsync(groupNew);

            // let's work on the results
            switch (groupsResults)
            {
                case HttpFriendicaContacts.PostFriendicaGroupsResults.GroupAlreadyExisting:
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                    AppResources.messageDialogGroupAlreadyExisting,
                                                                    AppResources.buttonOK);
                    break;
                
                case HttpFriendicaContacts.PostFriendicaGroupsResults.NotAnswered:
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                    AppResources.TestConnectionResultNotAnswered,
                                                                    AppResources.buttonOK);
                    break;

                case HttpFriendicaContacts.PostFriendicaGroupsResults.OK:
                    ClearGroupEditor();
                    ReloadContactsData();
                    break;

                case HttpFriendicaContacts.PostFriendicaGroupsResults.OkReactivatedGroup:
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                    AppResources.messageDialogGroupReactivated,
                                                                    AppResources.buttonOK);
                    ClearGroupEditor();
                    ReloadContactsData();
                    break;
                
                case HttpFriendicaContacts.PostFriendicaGroupsResults.SerializationError:
                case HttpFriendicaContacts.PostFriendicaGroupsResults.NotAuthenticated:
                case HttpFriendicaContacts.PostFriendicaGroupsResults.NotImplemented:    
                case HttpFriendicaContacts.PostFriendicaGroupsResults.UnexpectedError:
                default:    
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                    String.Format(AppResources.messageDialogManageGroupErrorResult, App.Contacts.StatusCode, App.Contacts.ReturnString),
                                                                    AppResources.buttonOK);
                    break;
            }
            ActivityIndicatorVisible = false;
        }

        private ICommand _groupEditorCancelCommand;
        public ICommand GroupEditorCancelCommand => _groupEditorCancelCommand ?? (_groupEditorCancelCommand = new Command(GroupEditorCancel));
        private async void GroupEditorCancel()
        {
            await CheckBeforeCancelling();
        }

        public async Task CheckBeforeCancelling()
        {
            if (IsGroupSaveEnabled)
            {
                var answer = await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                                       AppResources.MessageDialogGroupEditorClose,
                                                                       AppResources.buttonYes, AppResources.buttonNo);
                if (!answer)
                    return;
                else
                {
                    ClearGroupEditor();
                    SetNavigationAllowed(true);
                }
            }
            else
                ClearGroupEditor();
        }

        private ICommand _selectAllCommand;
        public ICommand SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new Command(SelectAll));
        private void SelectAll()
        {
            foreach (var item in FriendsInGroup)
            {
                if (item.Data.UserFollowing)
                    item.IsSelected = true;
            }
            SetIsGroupSaveEnabled();
        }

        private ICommand _deselectAllCommand;
        public ICommand DeselectAllCommand => _deselectAllCommand ?? (_deselectAllCommand = new Command(DeselectAll));
        private void DeselectAll()
        {
            foreach (var item in FriendsInGroup)
                item.IsSelected = false;
            SetIsGroupSaveEnabled();
        }
#endregion

        public ContactsViewModel()
        {
            Title = AppResources.pageTitleContacts_Text;

            FilteredFriends = new ObservableRangeCollection<FriendicaUser>();
            FilteredForums = new ObservableRangeCollection<FriendicaUser>();
            FilteredGroups = new ObservableRangeCollection<FriendicaGroup>();

            // init HttpFriendicaContacts
            if (App.Contacts == null)
                App.Contacts = new HttpFriendicaContacts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);

            if (App.Contacts.Friends == null && App.Contacts.Groups == null)
                ReloadContactsData();
            else
                RebuildListViews();
        }


        private async void ReloadContactsData()
        {
            if (!Settings.IsFriendicaLoginDefined())
            {
                RebuildListViews();
                return;
            }

            RefreshContactsEnabled = false;
            ActivityIndicatorText = AppResources.TextSettingsLoadingContacts;
            ActivityIndicatorVisible = true;
            var result = await App.Contacts.GetStatusesFriends();
            var resultGroup = await App.Contacts.GetFriendicaGroupShow();
            ActivityIndicatorVisible = false;
            RefreshContactsEnabled = true;

            // now work with the results
            if (result == HttpFriendicaContacts.GetFriendicaFriendsResults.OK &&
               resultGroup == HttpFriendicaContacts.GetFriendicaFriendsResults.OK)
            {
                RebuildListViews();
            }
            else
            {
                // display error messages
                ErrorMessages(result);
                ErrorMessages(resultGroup);
            }
        }

        private void ErrorMessages(HttpFriendicaContacts.GetFriendicaFriendsResults result)
        {
            ServerActivityFailed = true;
            switch (result)
            {
                case HttpFriendicaContacts.GetFriendicaFriendsResults.NotAnswered:
                    LabelServerActivityFailed = AppResources.messageDialogHttpStatusNone;
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.NotImplemented:
                    // show a hint if server version is too old to retrieve group data
                    LabelServerActivityFailed = AppResources.textblockNoGroupsAvailable_Text;
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.SerializationError:
                    // show error with requested URL and the returned JSON upon serialization error
                    LabelServerActivityFailed = String.Format(AppResources.TextSerializationError,
                                                        App.Contacts._requestedUrl, App.Contacts.ReturnString);
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.UnexpectedError:
                    LabelServerActivityFailed = String.Format(AppResources.TestConnectionResultUnexpectedError,
                                                              App.Contacts.StatusCode, App.Contacts.ErrorMessage, App.Contacts.ErrorHResult);
                    break;
            }
        }

        private void RebuildListViews()
        {
            // check if there was data to display, otherwise show a hint
            NoDataAvailableFriends = (App.Contacts.Friends == null || App.Contacts.Friends.Count == 0);
            NoDataAvailableForums = (App.Contacts.Forums == null || App.Contacts.Forums.Count == 0);
            NoDataAvailableGroups = (App.Contacts.Groups == null || App.Contacts.Groups.Count == 0);

            // reset search fields
            SearchFriends = "";
            SearchForums = "";

            // fill listviews
            FilteringFriends();
            FilteringForums();
            FilteringGroups();
        }

        void FilteringFriends()
        {
            if (App.Contacts.Friends != null)
                FilteredFriends.ReplaceRange(App.Contacts.Friends.Where(f => f.UserName.ToLower().Contains(SearchFriends.ToLower())));
        }

        void FilteringForums()
        {
            if (App.Contacts.Forums != null)
                FilteredForums.ReplaceRange(App.Contacts.Forums.Where(f => f.UserName.ToLower().Contains(SearchForums.ToLower())));
        }

        void FilteringGroups()
        {
            if (App.Contacts.Groups != null)
                FilteredGroups.ReplaceRange(App.Contacts.Groups.Where(f => f.GroupName.ToLower().Contains("")));
            foreach (var group in FilteredGroups)
                group.GroupDeleted += (object sender, EventArgs e) => { ReloadContactsData(); };
        }

        public void CreateGroupEditorListView()
        {
            if (GroupInEditor != null && GroupInEditor.GroupUser == null)
                GroupInEditor.GroupUser = new System.Collections.Generic.List<FriendicaUser>();

            FriendsInGroup = new ObservableCollection<SelectableData<FriendicaUser>>();
            if (App.Contacts.Friends != null)
            {
                foreach (var friend in App.Contacts.Friends)
                {
                    var selectedData = new SelectableData<FriendicaUser>(friend)
                    {
                        IsSelected = (GroupInEditor.GroupUser.Any(u => u.UserCid == friend.UserCid))
                    };
                    FriendsInGroup.Add(selectedData);
                }
            }
            if (App.Contacts.Forums != null)
            {
                foreach (var forum in App.Contacts.Forums)
                {
                    var selectedData = new SelectableData<FriendicaUser>(forum)
                    {
                        IsSelected = (GroupInEditor.GroupUser.Any(u => u.UserCid == forum.UserCid))
                    };
                    FriendsInGroup.Add(selectedData);
                }
            }
        }

        public void SetIsGroupSaveEnabled()
        {
            if (!IsGroupEditorVisible)
                return;

            bool enableSave;
            if (IsGroupEditorInChangeMode)
            {
                if (GroupInEditor.GroupUser.Count() != FriendsInGroup.Count(u => u.IsSelected))
                    enableSave = true;
                else
                {
                    bool hasDifferences = true;
                    foreach (var contact in FriendsInGroup.Where(u => u.IsSelected))
                        hasDifferences &= GroupInEditor.GroupUser.Exists(u => u.UserCid == contact.Data.UserCid);
                    enableSave = !hasDifferences;
                }
            }
            else
            {
                if (FriendsInGroup == null)
                    enableSave = false;
                else
                {
                    enableSave = (_groupName != null && _groupName != "" && FriendsInGroup.Any(u => u.IsSelected));
                }
            }
            IsGroupSaveEnabled = enableSave;
            SetNavigationAllowed(!enableSave);
        }

        private void ClearGroupEditor()
        {
            IsGroupEditorVisible = false;
            IsGroupSaveEnabled = false;
            GroupInEditor = null;
            FriendsInGroup = null;
        }

    }
}
