using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace Friendica_Mobile.Mvvm
{
    public class PhotoRightsViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        #region properties
        // get photo data from App.
        public FriendicaPhotoExtended Photo
        {
            get
            {
                if (App.PhotosVm.SelectedPhotoalbum != null && App.PhotosVm.SelectedPhotoalbum.SelectedPhoto != null)
                    return App.PhotosVm.SelectedPhotoalbum.SelectedPhoto;
                else
                    return null;
            }
        }

        // indicator if photo has already comments or likes/dislikes (as this will lead to problems when changing rights)
        private bool _issCommentedLiked;
        public bool IsCommentedLiked
        {
            get { return _issCommentedLiked; }
            set { _issCommentedLiked = value; }
        }

        // showing hint on changing 
        private bool _isShowingHintOnChanging;
        public bool IsShowingHintOnChanging
        {
            get { return _isShowingHintOnChanging; }
            set { _isShowingHintOnChanging = value;
                OnPropertyChanged("IsShowingHintOnChanging");
                OnPropertyChanged("IsHintVisibleFull");
                OnPropertyChanged("IsPhotoDisplayed");
            }
        }

        // text to be shown in hint on changing rights
        private string _hintOnChanging;
        public string HintOnChanging
        {
            get { return _hintOnChanging; }
            set { _hintOnChanging = value;
                OnPropertyChanged("HintOnChanging"); }
        }

        // indicator if hint is shown in full for Desktop or in small version for mobile
        public bool IsHintVisibleFull
        {
            get
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobilePortrait ||
                    App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                    return false;
                else
                    return true;
            }
        }

        // indicator if change should be realised by a new upload of photo or by a simple update
        private bool _updateAsNewUpload;
        public bool UpdateAsNewUpload
        {
            get { return _updateAsNewUpload; }
            set { _updateAsNewUpload = value; }
        }

        // indicator if photo is displayed (don't display it on MobilePortrait)
        public bool IsPhotoDisplayed
        {
            get
            {
                if (App.Settings.OrientationDevice != OrientationDeviceFamily.MobilePortrait)
                {
                    if (!IsShowingHintOnChanging)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        // is true when photo is publicly visible
        private bool _isPublicPhoto;
        public bool IsPublicPhoto
        {
            get { return _isPublicPhoto; }
            set { _isPublicPhoto = value;
                CheckChangedSettings();
                OnPropertyChanged("IsPublicPhoto");
            }
        }

        // is true when photo is only visible to limited groups/contacts
        private bool _isPrivatePhoto;
        public bool IsPrivatePhoto
        {
            get { return _isPrivatePhoto; }
            set { _isPrivatePhoto = value;
                OnPropertyChanged("IsPrivatePhoto"); }
        }

        // containing data which will be shown in listview groups
        private ObservableCollection<FriendicaGroup> _groups;
        public ObservableCollection<FriendicaGroup> Groups
        {
            get { return _groups; }
            set { _groups = value;
                OnPropertyChanged("Groups"); }
        }

        // contain selected groups
        private List<FriendicaGroup> _selectedGroups;
        public List<FriendicaGroup> SelectedGroups
        {
            get { return _selectedGroups; }
            set
            {
                _selectedGroups = value;
                CheckChangedSettings();
                OnPropertyChanged("SelectedGroups");
            }
        }

        // list of original selected groups 
        private List<double> _selectedGroupsOriginal;
        private List<double> _selectedContactsOriginal;

        // list of currently selected groups (not yet saved on server) 
        private List<double> _selectedGroupsCurrent;
        private List<double> _selectedContactsCurrent;

        // containing data which will be shown in listview contacts
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

        // data for GridView listing friends 
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

        // contain selected contacts
        private List<FriendicaUserExtended> _selectedContacts;
        public List<FriendicaUserExtended> SelectedContacts
        {
            get { return _selectedContacts; }
            set
            {
                _selectedContacts = value;
                CheckChangedSettings();
                OnPropertyChanged("SelectedContacts");
            }
        }

        #endregion


        #region commands
        // undo button
        Mvvm.Command _undoChangesCommand;
        public Mvvm.Command UndoChangesCommand { get { return _undoChangesCommand ?? (_undoChangesCommand = new Mvvm.Command(ExecuteUndoChanges)); } }
        private void ExecuteUndoChanges()
        {
            SelectedGroups = new List<FriendicaGroup>();
            SelectedContacts = new List<FriendicaUserExtended>();
            LoadCurrentRightsFromPhoto(true);
            CheckChangedSettings();
            OnSelectedContactsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion


        #region eventhandlers
        // triggering preparation of group header overview for contacts semanticzoom listview
        public event EventHandler SourceContactsLoaded;

        // called when user has clicked the undo button - this will trigger to recreate the "selection ticks" in the contacts listview
        public event EventHandler OnSelectedContactsChanged;
        
        #endregion


        public PhotoRightsViewmodel()
        {
            // initialize observable collection
            if (Groups == null)
                Groups = new ObservableCollection<FriendicaGroup>();
            _selectedGroupsOriginal = new List<double>();
            if (SelectedGroups == null) 
                SelectedGroups = new List<FriendicaGroup>();
            if (Contacts == null)
                Contacts = new ObservableCollection<FriendicaUserExtended>();
            _selectedContactsOriginal = new List<double>();
            if (SelectedContacts == null)
                SelectedContacts = new List<FriendicaUserExtended>();

            App.Settings.PropertyChanged += Settings_PropertyChanged;
        }


        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OrientationDevice")
            {
                OnPropertyChanged("IsPhotoDisplayed");
                OnPropertyChanged("IsHintVisibleFull");
            }
        }


        public void LoadPhotoRights()
        {
            // needs to be done here from external call because there is an event handler waiting for updating SourceContacts
            LoadContactData();

            if (Photo != null)
            {
                LoadCurrentRightsFromPhoto();

                // check for comments or likes
                IsCommentedLiked = Photo.IsConversationStarted;

                // set always to true as this will be standard way if user wants to change the rights
                UpdateAsNewUpload = true;
            }
        }


        public void SaveChangedRights()
        {
            // transfer what user has selected for the updating way
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.UpdateAsNewUpload = UpdateAsNewUpload;

            // transfer what user has selected for groups and contacts
            if (IsPublicPhoto)
            {
                App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.IsPubliclyVisible = true;
                App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.NewPhotoAllowCid = "";
                App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.NewPhotoAllowGid = "";
            }
            else
            {
                App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.IsPubliclyVisible = false;
                App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.NewPhotoAllowCid = ConvertContactsToString();
                App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.NewPhotoAllowGid = ConvertGroupsToString();
            }
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.NewPhotoDenyCid = "";
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.NewPhotoDenyGid = "";
        }


        private string ConvertContactsToString()
        {
            string stringifiedContacts = "";
            foreach (var contact in SelectedContacts)
                stringifiedContacts += "<" + contact.User.UserCid + ">";
            return stringifiedContacts;
        }


        private string ConvertGroupsToString()
        {
            string stringifiedGroups = "";
            foreach (var group in SelectedGroups)
                stringifiedGroups += "<" + group.GroupGid + ">";
            return stringifiedGroups;
        }


        private async void LoadContactData()
        {
            // load all available groups/contacts into unordered list
            List<FriendicaGroup> listGroups = new List<FriendicaGroup>();
            List<FriendicaUserExtended> listContacts = new List<FriendicaUserExtended>();

            if (App.ContactsGroups == null || App.ContactsFriends == null)
            {
                string errorMsg = loader.GetString("messageDialogPhotoRightsNoContactData");
                var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                await dialog.ShowDialog(0, 1);
                if (dialog.Result == 0)
                {
                    LoadCurrentRightsFromPhoto();
                    return;
                }
            }

            foreach (var group in App.ContactsGroups)
                listGroups.Add(group);
            foreach (var contact in App.ContactsFriends)
                listContacts.Add(contact);

            // order list alphabetically and prepare CollectionViewSource binding element for character groups
            var orderedList = listGroups.OrderBy(g => g.GroupName.ToLower());
            foreach (var group in orderedList)
                Groups.Add(group);

            var ordererdListContacts = listContacts.OrderBy(c => c.User.UserName.ToLower());
            foreach (var contact in ordererdListContacts)
                Contacts.Add(contact);

            // load contacts into source for semanticlistview
            SourceContacts = from user in Contacts group user by user.CharacterGroup into grp orderby grp.Key select grp;
        }


        private void LoadCurrentRightsFromPhoto(bool undo = false)
        {
            // convert acl string (i.e. "<1><2>") into list of double values
            _selectedGroupsOriginal = Photo.ConvertContactListStringToList(Photo.Photo.PhotoAllowGid);
            _selectedContactsOriginal = Photo.ConvertContactListStringToList(Photo.Photo.PhotoAllowCid);

            if (undo)
            {
                // load data from new right fields when the photo has not yet been uploaded to server
                if (Photo.Photo.PhotoAllowCid == "" && Photo.Photo.PhotoAllowGid == "" && Photo.Photo.PhotoDenyCid == "" && Photo.Photo.PhotoDenyGid == "")
                    IsPublicPhoto = true;
                else
                    IsPublicPhoto = false;
                IsPrivatePhoto = !IsPublicPhoto;

                _selectedGroupsCurrent = Photo.ConvertContactListStringToList(Photo.Photo.PhotoAllowGid);
                _selectedContactsCurrent = Photo.ConvertContactListStringToList(Photo.Photo.PhotoAllowCid);
            }
            else
            {
                // load data from new right fields when the photo has not yet been uploaded to server
                if (Photo.NewPhotoAllowCid == "" && Photo.NewPhotoAllowGid == "" && Photo.Photo.PhotoDenyCid == "" && Photo.Photo.PhotoDenyGid == "")
                    IsPublicPhoto = true;
                else
                    IsPublicPhoto = false;
                IsPrivatePhoto = !IsPublicPhoto;

                _selectedGroupsCurrent = Photo.ConvertContactListStringToList(Photo.NewPhotoAllowGid);
                _selectedContactsCurrent = Photo.ConvertContactListStringToList(Photo.NewPhotoAllowCid);
            }

            // convert list of id's into list of elements for displaying the selections
            foreach (var groupId in _selectedGroupsCurrent)
                SelectedGroups.Add(Groups.SingleOrDefault(g => g.GroupGid == groupId));
            foreach (var contactId in _selectedContactsCurrent)
                try
                {
                    // might fail because we have the own id in the rights list, but not shown in contacts
                    SelectedContacts.Add(Contacts.Single(c => c.User.UserCid == contactId));
                }
                catch { }
        }


        private void CheckChangedSettings()
        {
            var isOriginalPhotoPublic = false; 
            if (Photo.Photo.PhotoAllowCid == "" && Photo.Photo.PhotoAllowGid == "" && Photo.Photo.PhotoDenyCid == "" && Photo.Photo.PhotoDenyGid == "")
                isOriginalPhotoPublic = true;

            if (isOriginalPhotoPublic == true && IsPublicPhoto == false)
            {
                if (!Photo.NewUploadPlanned)
                {
                    IsShowingHintOnChanging = true;
                    HintOnChanging = loader.GetString("textPhotoRightsChangePubToPriv");
                    App.Settings.HideNavigationElements = true;
                }
                return;
            }

            if (isOriginalPhotoPublic == false && IsPublicPhoto == true)
            {
                if (!Photo.NewUploadPlanned)
                {
                    IsShowingHintOnChanging = true;
                    HintOnChanging = loader.GetString("textPhotoRightsChangePrivToPub");
                    App.Settings.HideNavigationElements = true;
                }
                return;
            }

            // check removals before addings as the consequences of removals are more serious
            if (SelectedRightsRemoved())
            {
                if (!Photo.NewUploadPlanned)
                {
                    IsShowingHintOnChanging = true;
                    HintOnChanging = loader.GetString("textPhotoRightsRemoveContact");
                    App.Settings.HideNavigationElements = true;
                }
                return;
            }

            if (SelectedRightsAdded())
            {
                if (!Photo.NewUploadPlanned)
                {
                    IsShowingHintOnChanging = true;
                    HintOnChanging = loader.GetString("textPhotoRightsAddContact");
                    App.Settings.HideNavigationElements = true;
                }
                return;
            }

            if (!Photo.NewUploadPlanned)
            {
                IsShowingHintOnChanging = false;
                HintOnChanging = "";
                App.Settings.HideNavigationElements = false;
            }
        }


        private bool SelectedRightsAdded()
        {
            var hasAdded = false;

            if (SelectedGroups == null || SelectedContacts == null)
                return hasAdded;

            foreach (var group in SelectedGroups)
                hasAdded |= (!_selectedGroupsOriginal.Exists(g => g == group.GroupGid));
            foreach (var contact in SelectedContacts)
                hasAdded |= (!_selectedContactsOriginal.Exists(c => c == contact.User.UserCid));

            return hasAdded;
        }


        private bool SelectedRightsRemoved()
        {
            var hasRemoved = false;

            if (SelectedGroups == null || SelectedContacts == null)
                return hasRemoved;

            foreach (var groupId in _selectedGroupsOriginal)
                hasRemoved |= (!SelectedGroups.Exists(g => g.GroupGid == groupId));
            foreach (var contactId in _selectedContactsOriginal)
                hasRemoved |= (!SelectedContacts.Exists(c => c.User.UserCid == contactId));

            return hasRemoved;
        }

    }
}