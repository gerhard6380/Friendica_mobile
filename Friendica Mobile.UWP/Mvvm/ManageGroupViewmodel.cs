using Friendica_Mobile.UWP.HttpRequests;
using Friendica_Mobile.UWP.Models;
using Friendica_Mobile.UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace Friendica_Mobile.UWP.Mvvm
{
    class ManageGroupViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public Thickness BottomAppBarMargin { get { return App.Settings.BottomAppBarMargin; } }
        public double ListViewWidth { get { return App.Settings.ShellWidth; } }
        public double ListViewHeight { get { return App.Settings.ShellHeight; } }

        // data for GridView listing friends incl. Eventhandler triggering preparation of group header overview
        public event EventHandler SourceContactsLoaded;
        private IOrderedEnumerable<IGrouping<string, FriendicaUserExtended>> _sourceContacts;
        public IOrderedEnumerable<IGrouping<string, FriendicaUserExtended>> SourceContacts
        {
            get { return _sourceContacts; }
            set
            {
                _sourceContacts = value;
                OnPropertyChanged("SourceContacts");
                if (SourceContactsLoaded != null)
                    SourceContactsLoaded(this, EventArgs.Empty);
            }
        }

        // containing data which will be shown in listview friends
        private ObservableCollection<FriendicaUserExtended> _contacts;
        public ObservableCollection<FriendicaUserExtended> Contacts
        {
            get { return _contacts; }
            set
            {
                _contacts = value;
                OnPropertyChanged("Contacts");
            }
        }

        // contain selected users
        private List<FriendicaUserExtended> _selectedItems;
        public List<FriendicaUserExtended> SelectedItems
        {
            get { return _selectedItems; }
            set { _selectedItems = value;
                SaveCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedItems"); }
        }

        // indicator for showing that we are creating a new group instead of changing an existing
        private bool _isNewGroup;
        public bool IsNewGroup
        {
            get { return _isNewGroup; }
            set
            {
                _isNewGroup = value;
                OnPropertyChanged("IsNewGroup");
            }
        }

        // indicator that user has already changed something
        private string _isChanged;
        public string IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
            }
        }

        // containing data which will be shown in listview groups 
        private FriendicaGroup _groupOld;
        public FriendicaGroup GroupOld
        {
            get { return _groupOld; }
            set
            {
                _groupOld = value;
                GetPropertiesFromOldGroup();
            }
        }

        // group name for display and input
        private string _groupName;
        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value;
                SaveCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("GroupName"); }
        }



        // show or hide loading indicator and change status of buttons
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        // show or hide loading indicator and change status of buttons
        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            set { _isSaving = value;
                OnPropertyChanged("IsSaving");
            }
        }

        // show or hide indicator next to save button showing that saving has finished
        private bool _hasSaved;
        public bool HasSaved
        {
            get { return _hasSaved; }
            set
            {
                _hasSaved = value;
                OnPropertyChanged("HasSaved");
            }
        }


        // Indicator for no Settings declaring that the data are samples
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set
            {
                _noSettings = value;
                OnPropertyChanged("NoSettings");
            }
        }


        // show or hide error message on server problems
        private bool _noDataAvailableContacts;
        public bool NoDataAvailableContacts
        {
            get { return _noDataAvailableContacts; }
            set
            {
                _noDataAvailableContacts = value;
                OnPropertyChanged("NoDataAvailableContacts");
            }
        }


        // indicating number of columns shown in gridview zoomout depending on the device type and orientation
        private int _maxColumns;
        public int MaxColumns
        {
            get { return _maxColumns; }
            set
            {
                _maxColumns = value;
                OnPropertyChanged("MaxColumns");
            }
        }


        // save button
        Mvvm.Command _saveCommand;
        public Mvvm.Command SaveCommand { get { return _saveCommand ?? (_saveCommand = new Mvvm.Command(ExecuteSave, CanSave)); } }
        private bool CanSave()
        {
            bool enableSave;
            if (IsNewGroup)
            {
                enableSave = (GroupName != "" || SelectedItems.Count != 0);
            }
            else
            {
                if (GroupOld.GroupUser.Count != SelectedItems.Count)
                    enableSave = true;
                else
                {
                    bool hasDifferences = true;
                    foreach (var contact in SelectedItems)
                            hasDifferences &= GroupOld.GroupUser.Exists(c => c.UserCid == contact.User.UserCid);
                    enableSave = !hasDifferences;
                }
            }

            App.NavStatus = (enableSave ? NavigationStatus.GroupChanged : NavigationStatus.OK);
            return enableSave;
        }

        private async void ExecuteSave()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
|| App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                var messageDialog = new MessageDialogMessage(loader.GetString("messageDialogGroupChangeSamples"), "", loader.GetString("buttonYes"), "");
                await messageDialog.ShowDialog(0,0);
                SaveCommand.RaiseCanExecuteChanged();
                HasSaved = true;
                App.NavStatus = NavigationStatus.OK;
            }
            else
            {
                // warning if user has not selected any user
                if (SelectedItems.Count == 0)
                {
                    var messageDialog = new MessageDialogMessage(loader.GetString("messageDialogWarningOnEmptyUserlist"),
                                                            "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await messageDialog.ShowDialog(1, 1);
                    if (messageDialog.Result == 1)
                        return;
                }
                // send data to the server (different calls on create or change a group)
                IsSaving = true;
                var getGroups = new GetFriendicaGroups();
                getGroups.FriendicaGroupsLoaded += GetGroups_FriendicaGroupsLoaded;

                var groupNew = new FriendicaGroup();
                groupNew.GroupName = GroupName;

                GroupOld.GroupUser.Clear();
                foreach (var user in SelectedItems)
                    groupNew.GroupUser.Add(user.User);

                if (IsNewGroup)
                {
                    getGroups.FriendicaGroupCreate(groupNew);
                }
                else
                {
                    groupNew.GroupGid = GroupOld.GroupGid;
                    getGroups.FriendicaGroupUpdate(groupNew);
                }
                GetPropertiesFromOldGroup();
            }
        }


        private async void GetGroups_FriendicaGroupsLoaded(object sender, EventArgs e)
        {
            var getGroups = sender as GetFriendicaGroups;
            if (getGroups.StatusCode == HttpStatusCode.Ok)
            {
                if (getGroups.ReturnString.StartsWith("{\"error\":"))
                {
                    string errorMsg = "";
                    if (getGroups.ReturnString.Contains("group name already exists"))
                        errorMsg = String.Format(loader.GetString("messageDialogGroupAlreadyExisting"), getGroups.CreateGroup.GroupName);
                    // 'group name not specified' doesn't need to be handled as this checked already in this app before sending the webrequest
                    else 
                        errorMsg = loader.GetString("messageDialogGroupAPIerror");

                    // insert text again in textbox
                    GroupName = getGroups.CreateGroup.GroupName;

                    // info to user that group has been reactivated
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", "");
                    await dialog.ShowDialog(0, 0);
                }
                else
                {
                    var result = new FriendicaGroupResult(getGroups.ReturnString);
                    GroupOld.GroupGid = result.GroupGid;
                    GroupOld.GroupName = result.GroupName;
                    OnPropertyChanged("GroupOld");
                    GroupName = result.GroupName;

                    GroupOld.GroupUser.Clear();
                    foreach (var user in SelectedItems)
                        GroupOld.GroupUser.Add(user.User);

                    IsNewGroup = false;
                    // force app to reload the contacts
                    App.IsLoadedContacts = false;

                    if (result.GroupStatus == "reactivated")
                    {
                        // info to user that group has been reactivated
                        var dialog = new MessageDialogMessage(loader.GetString("messageDialogGroupReactivated"), "", "OK", "");
                        await dialog.ShowDialog(0, 0);
                    }
                    SaveCommand.RaiseCanExecuteChanged();
                    HasSaved = true;
                }
            }
            else
            {
                string msgError = String.Format(loader.GetString("messageDialogManageGroupErrorResult"), getGroups.StatusCode, getGroups.ReturnString);
                var dialog = new MessageDialogMessage(msgError, "", "OK", "");
                await dialog.ShowDialog(0, 0);
            }
            IsSaving = false;
        }

        public ManageGroupViewmodel()
        {
            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;

            // initialize observable collection
            if (Contacts == null)
                Contacts = new ObservableCollection<FriendicaUserExtended>();
            if (SelectedItems == null)
                SelectedItems = new List<FriendicaUserExtended>();
        }


        public void InitialLoad()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                IsLoading = true;
                NoSettings = true;
                LoadFromApp();

                // hide red bar with progress ring for loading process
                IsLoading = false;
                NoDataAvailableContacts = false;
            }
            else
            {
                IsLoading = true;
                NoSettings = false;
                LoadFromApp();

                IsLoading = false;
                // Anzeige einer Info triggern, die dem User anzeigt, dass die Daten geladen wurden, aber nichts gekommen ist
                NoDataAvailableContacts = (Contacts == null || Contacts.Count == 0);
            }
        }


        private void PrepareSources()
        {
            SourceContacts = from user in Contacts group user by user.CharacterGroup into grp orderby grp.Key select grp;
        }

        private void LoadFromApp()
        {
            // load data for friends and forums into unordered list
            List<FriendicaUserExtended> list = new List<FriendicaUserExtended>();
            foreach (var friend in App.ContactsFriends)
                list.Add(friend);
            foreach (var forum in App.ContactsForums)
                list.Add(forum);

            // order list alphabetically and prepare CollectionViewSource binding element for character groups
            var orderedList = list.OrderBy(m => m.User.UserName.ToLower());
            foreach (var contact in orderedList)
                Contacts.Add(contact);
            PrepareSources();
        }


        private void GetPropertiesFromOldGroup()
        {
            if (GroupOld != null)
            {
                // prepare properties like SelectedItems if we are in changing mode
                GroupName = GroupOld.GroupName;
                foreach (var contact in GroupOld.GroupUser)
                {
                    SelectedItems.Add(Contacts.SingleOrDefault(c => c.User.UserCid == contact.UserCid));
                }
            }
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth")
                OnPropertyChanged("ListViewWidth");
            if (e.PropertyName == "ShellHeight")
                OnPropertyChanged("ListViewHeight");
            // if MobilePortrait is activated, recheck the height and width of the gridview
            if (e.PropertyName == "OrientationDevice")
            {
                OnPropertyChanged("ListViewHeight");
                // set number of columns depending on mobile device, otherwise columns are vanishing
                switch (App.Settings.OrientationDevice)
                {
                    case OrientationDeviceFamily.MobilePortrait:
                        MaxColumns = 4;
                        break;
                    case OrientationDeviceFamily.MobileLandscape:
                        MaxColumns = 7;
                        break;
                    default:
                        MaxColumns = 10;
                        break;
                }
            }
        }
    }
}