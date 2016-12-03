using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.Web.Http;

namespace Friendica_Mobile.Mvvm
{
    class ProfileUsersViewmodel : BindableClass
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
                SourceContactsLoaded?.Invoke(this, EventArgs.Empty);
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
        private List<FriendicaUserExtended> _selectedUsers;
        public List<FriendicaUserExtended> SelectedUsers
        {
            get { return _selectedUsers; }
            set { _selectedUsers = value;
                SaveCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedUsers"); }
        }

        // indicator that user has already changed something (used later)
        private string _isChanged;
        public string IsChanged
        {
            get { return _isChanged; }
            set
            {
                _isChanged = value;
            }
        }

        // selected profile given as parameter in navigation
        private FriendicaProfile _selectedProfile;
        public FriendicaProfile SelectedProfile
        {
            get { return _selectedProfile; }
            set { _selectedProfile = value;
                SaveCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedProfile"); }
        }


        // show or hide loading indicator and change status of buttons
        private bool _isUpdatingServer;
        public bool IsUpdatingServer
        {
            get { return _isUpdatingServer; }
            set { _isUpdatingServer = value;
                OnPropertyChanged("IsUpdatingServer");
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


        // show or hide error message if profile has no attached users
        private bool _noUsersAvailable;
        public bool NoUsersAvailable
        {
            get { return _noUsersAvailable; }
            set
            {
                _noUsersAvailable = value;
                OnPropertyChanged("NoUsersAvailable");
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
            //bool enableSave;
            //if (IsNewGroup)
            //{
            //    enableSave = (GroupName != "" || SelectedItems.Count != 0);
            //}
            //else
            //{
            //    if (GroupOld.GroupUser.Count != SelectedItems.Count)
            //        enableSave = true;
            //    else
            //    {
            //        bool hasDifferences = true;
            //        foreach (var contact in SelectedItems)
            //                hasDifferences &= GroupOld.GroupUser.Exists(c => c.UserCid == contact.User.UserCid);
            //        enableSave = !hasDifferences;
            //    }
            //}

            //App.NavStatus = (enableSave ? NavigationStatus.GroupChanged : NavigationStatus.OK);
            //return enableSave;
            return false;
        }

        private async void ExecuteSave()
        {
//            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
//|| App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
//            {
//                var messageDialog = new MessageDialogMessage(loader.GetString("messageDialogGroupChangeSamples"), "", loader.GetString("buttonYes"), "");
//                await messageDialog.ShowDialog(0,0);
//                SaveCommand.RaiseCanExecuteChanged();
//                App.NavStatus = NavigationStatus.OK;
//            }
//            else
//            {
//                // warning if user has not selected any user
//                if (SelectedUsers.Count == 0)
//                {
//                    var messageDialog = new MessageDialogMessage(loader.GetString("messageDialogWarningOnEmptyUserlist"),
//                                                            "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
//                    await messageDialog.ShowDialog(1, 1);
//                    if (messageDialog.Result == 1)
//                        return;
//                }
//                // send data to the server (different calls on create or change a group)
//                IsSaving = true;
//                var getGroups = new GetFriendicaGroups();
//                getGroups.FriendicaGroupsLoaded += GetGroups_FriendicaGroupsLoaded;

//                var groupNew = new FriendicaGroup();
//                groupNew.GroupName = GroupName;

//                GroupOld.GroupUser.Clear();
//                foreach (var user in SelectedItems)
//                    groupNew.GroupUser.Add(user.User);

//                if (IsNewGroup)
//                {
//                    getGroups.FriendicaGroupCreate(groupNew);
//                }
//                else
//                {
//                    groupNew.GroupGid = GroupOld.GroupGid;
//                    getGroups.FriendicaGroupUpdate(groupNew);
//                }
//                GetPropertiesFromOldGroup();
//            }
        }


        public ProfileUsersViewmodel()
        {
            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;

            // initialize observable collection
            if (Contacts == null)
                Contacts = new ObservableCollection<FriendicaUserExtended>();
            if (SelectedUsers == null)
                SelectedUsers = new List<FriendicaUserExtended>();

            CheckServerSettings();
        }

        private void CheckServerSettings()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                NoSettings = true;
            }
            else
                NoSettings = false;
        }


        public void InitialLoad()
        {
            if (!NoSettings)
            {
                LoadFromApp();
            }
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
            SelectUsers();
        }

        private void PrepareSources()
        {
            SourceContacts = from user in Contacts group user by user.CharacterGroup into grp orderby grp.Key select grp;
        }

        private void SelectUsers()
        {
            foreach (var contact in SelectedProfile.ProfileUsers)
            {
                SelectedUsers.Add(Contacts.SingleOrDefault(c => c.User.UserCid == contact.UserCid));
            }

            NoUsersAvailable = (SelectedUsers.Count == 0) ? true : false;
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