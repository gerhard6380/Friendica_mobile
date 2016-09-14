using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Friendica_Mobile.Mvvm
{
    class NewPostViewmodel : BindableClass
    {
        public FriendicaNewPost newPost;

        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public event EventHandler SendingNewPostStarted;
        public double ListViewWidth { get { return App.Settings.ShellWidth; } }

        // selected post if user has clicked the add button on the post
        private FriendicaPostExtended _postToShow;
        public FriendicaPostExtended PostToShow
        {
            get { return _postToShow; }
            set { _postToShow = value; }
        }

        public string NewPostTitle
        {
            get { return newPost.NewPostTitle; }
            set
            {
                newPost.NewPostTitle = value;
                OnPropertyChanged("NewPostTitle");
            }
        }

        public string NewPostStatus
        {
            get { return newPost.NewPostStatus; }
            set
            {
                newPost.NewPostStatus = value;
                PostCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("NewPostStatus");
            }
        }

        private bool _sendCoordinates;
        public bool SendCoordinates
        {
            get { return _sendCoordinates; }
            set { _sendCoordinates = value;
                GetLocation();
                OnPropertyChanged("SendCoordinates"); }
        }

        private bool _sendCoordinatesImage;
        public bool SendCoordinatesImage
        {
            get { return _sendCoordinatesImage; }
            set { _sendCoordinatesImage = value;
                OnPropertyChanged("SendCoordinatesImage"); }
        }

        private bool _sendCoordinatesImageActive;
        public bool SendCoordinatesImageActive
        {
            get { return _sendCoordinatesImageActive; }
            set { _sendCoordinatesImageActive = value;
                OnPropertyChanged("SendCoordinatesImageActive"); }
        }

        private double _imageLatitude;
        public double ImageLatitude
        {
            get { return _imageLatitude; }
            set { _imageLatitude = value;
                EnableCoordinatesImage();
                OnPropertyChanged("ImageLatitude"); }
        }

        private double _imageLongitude;
        public double ImageLongitude
        {
            get { return _imageLongitude; }
            set { _imageLongitude = value;
                EnableCoordinatesImage();
                OnPropertyChanged("ImageLongitude"); }
        }

        private double _locationLatitude;
        public double LocationLatitude
        {
            get { return _locationLatitude; }
            set { _locationLatitude = value;
                OnPropertyChanged("LocationLatitude"); }
        }

        private double _locationLongitude;
        public double LocationLongitude
        {
            get { return _locationLongitude; }
            set { _locationLongitude = value;
                OnPropertyChanged("LocationLongitude"); }
        }

        private void EnableCoordinatesImage()
        {
            if (ImageLatitude != 0.0 && ImageLongitude != 0.0)
            {
                SendCoordinatesImageActive = true;
                SendCoordinatesImage = true;
            }
            else
            {
                SendCoordinatesImageActive = false;
                SendCoordinatesImage = false;
            }
        }

        private BitmapImage _newPostImage;
        public BitmapImage NewPostImage
        {
            get { return _newPostImage; }
            set { _newPostImage = value;
                ImageSelected = (value != null) ? true : false;
                OnPropertyChanged("NewPostImage"); }
        }

        private bool _imageSelected;
        public bool ImageSelected
        {
            get { return _imageSelected; }
            set { _imageSelected = value;
                PostCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("ImageSelected"); }
        }


        private bool _radioButtonPublicPostChecked;
        public bool RadioButtonPublicPostChecked
        {
            get { return _radioButtonPublicPostChecked; }
            set { _radioButtonPublicPostChecked = value;
                CheckChangesInACL();
                OnPropertyChanged("RadioButtonPublicPostChecked"); }
        }


        private bool _radioButtonPrivatePostChecked;
        public bool RadioButtonPrivatePostChecked
        {
            get { return _radioButtonPrivatePostChecked; }
            set { _radioButtonPrivatePostChecked = value;
                CheckChangesInACL();
                OnPropertyChanged("RadioButtonPrivatePostChecked"); }
        }

        private bool _noContactsGroupsSelectedVisible;
        public bool NoContactsGroupsSelectedVisible
        {
            get { return _noContactsGroupsSelectedVisible; }
            set { _noContactsGroupsSelectedVisible = value;
                OnPropertyChanged("NoContactsGroupsSelectedVisible"); }
        }
        private string GetSelectedContactsString()
        {
            var selectedContacts = "";
            if (SelectedContacts != null)
            {
                var contacts = SelectedContacts.OrderBy(c => c.User.UserCid);
                foreach (var contact in contacts)
                    selectedContacts += "<" + contact.User.UserCid + ">";
            }
            return selectedContacts;
        }

        private string GetSelectedGroupsString()
        {
            var selectedGroups = "";
            if (SelectedGroups != null)
            {
                var groups = SelectedGroups.OrderBy(g => g.GroupGid);
                foreach (var group in groups)
                    selectedGroups += "<" + group.GroupGid + ">";
            }
            return selectedGroups;
        }

        private void CheckChangesInACL()
        {
            var selectedContacts = GetSelectedContactsString();
            var selectedGroups = GetSelectedGroupsString();

            bool isChanged = false;
            // compare App.Settings with current state of RadioButton, if changed enable SaveACLCheckBox
            if (!isChanged && RadioButtonPublicPostChecked != App.Settings.ACLPublicPost)
                isChanged = true;
            if (!isChanged && RadioButtonPrivatePostChecked != App.Settings.ACLPrivatePost)
                isChanged = true;

            // compare App.Settings with current list of selected contacts/groups, if changed enable SaveACLCheckBox (only on private posts)
            if (RadioButtonPrivatePostChecked)
            {
                if (!isChanged && selectedContacts != App.Settings.ACLPrivateSelectedContacts)
                    isChanged = true;
                if (!isChanged && selectedGroups != App.Settings.ACLPrivateSelectedGroups)
                    isChanged = true;
            }
            if (RadioButtonPrivatePostChecked && (SelectedContacts == null || SelectedContacts.Count == 0) && (SelectedGroups == null || SelectedGroups.Count == 0))
                NoContactsGroupsSelectedVisible = true;
            else
                NoContactsGroupsSelectedVisible = false;

            SaveACLActive = isChanged;
            PostCommand.RaiseCanExecuteChanged();
        }

        private bool _saveACL;
        public bool SaveACL
        {
            get { return _saveACL; }
            set { _saveACL = value;
                OnPropertyChanged("SaveACL"); }
        }

        private bool _saveACLActive;
        public bool SaveACLActive
        {
            get { return _saveACLActive; }
            set { _saveACLActive = value;
                OnPropertyChanged("SaveACLActive"); }
        }

        public ObservableCollection<FriendicaUserExtended> Contacts
        {
            get { return App.ContactsFriends; }
        }


        public ObservableCollection<FriendicaGroup> Groups
        {
            get { return App.ContactsGroups; }
        }

        private List<FriendicaUserExtended> _selectedContacts;
        public List<FriendicaUserExtended> SelectedContacts
        {
            get { return _selectedContacts; }
            set
            {
                _selectedContacts = value;
                CheckChangesInACL();
                OnPropertyChanged("SelectedContacts");
            }
        }

        private List<FriendicaGroup> _selectedGroups;
        public List<FriendicaGroup> SelectedGroups
        {
            get { return _selectedGroups; }
            set
            {
                _selectedGroups = value;
                CheckChangesInACL();
                OnPropertyChanged("SelectedGroups");
            }
        }


        // set visibility for the ACL part
        private bool _showACLVisible;
        public bool ShowACLVisible
        {
            get { return _showACLVisible; }
            set
            {
                _showACLVisible = value;
                OnPropertyChanged("ShowACLVisible");
            }
        }


        // set visibility for the ShowThread listview
        private bool _showThreadVisible;
        public bool ShowThreadVisible
        {
            get { return _showThreadVisible; }
            set { _showThreadVisible = value;
                OnPropertyChanged("ShowThreadVisible"); }
        }


        // containing data which will be shown in listview
        private ObservableCollection<FriendicaThread> _showThread;
        public ObservableCollection<FriendicaThread> ShowThread
        {
            get { return _showThread; }
            set
            {
                _showThread = value;
                OnPropertyChanged("ShowThread");
            }
        }

        // show indicator for sending a post if answering directly to a toast notification/Cortana speech in put
        private bool _isSendingNewPost;
        public bool IsSendingNewPost
        {
            get { return _isSendingNewPost; }
            set { _isSendingNewPost = value;
                OnPropertyChanged("IsSendingNewPost"); }
        }

        public void ReloadThreadData()
        {
            // recreate transformed post entries
            var thread = _showThread.ElementAt(0);
            var obscoll = new ObservableCollection<FriendicaPostExtended>();
            foreach (var post in thread.Posts)
            {
                post.ConvertHtmlToParagraph(post.Post.PostStatusnetHtml);
                obscoll.Add(post);
            }
            thread.Posts = obscoll;
            ShowThread = new ObservableCollection<FriendicaThread>();
            ShowThread.Add(thread);
            OnPropertyChanged("ShowThread");
        }


        // post button
        Mvvm.Command _postCommand;
        public Mvvm.Command PostCommand { get { return _postCommand ?? (_postCommand = new Mvvm.Command(ExecutePost, CanPost)); } }
        private bool CanPost()
        {
            if ((NewPostStatus != "" && NewPostStatus != "\r\r") || ImageSelected)
            {
                if (ShowACLVisible && !RadioButtonPrivatePostChecked && !RadioButtonPublicPostChecked)
                {
                    App.NavStatus = NavigationStatus.OK;
                    return false;
                }
                else
                {
                    App.NavStatus = NavigationStatus.NewPostChanged;
                    return true;
                }
            }
            else
            {
                App.NavStatus = NavigationStatus.OK;
                return false;
            }
        }

        private async void ExecutePost()
        {
            // checking if we are in the test mode without settings enabling user to play around with the app
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                        || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                string errorMsg;
                errorMsg = loader.GetString("messageDialogNewPostNoSettings");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                App.NavStatus = NavigationStatus.OK;
            }
            else
            {
                // set final lat and long from the two different sets
                if (SendCoordinates && SendCoordinatesImageActive && SendCoordinatesImage)
                {
                    newPost.NewPostLatitude = ImageLatitude;
                    newPost.NewPostLongitude = ImageLongitude;
                }
                else if (SendCoordinates)
                {
                    newPost.NewPostLatitude = LocationLatitude;
                    newPost.NewPostLongitude = LocationLongitude;
                }

                // set acl depending on selections (if public post is selected save empty strings as selections are kept in background)
                if (RadioButtonPrivatePostChecked)
                {
                    newPost.NewPostContactAllow = GetSelectedContactsString();
                    newPost.NewPostGroupAllow = GetSelectedGroupsString();
                }
                else
                {
                    newPost.NewPostContactAllow = "";
                    newPost.NewPostGroupAllow = "";
                }

                // save ACL change to App.Settings if user wishes to do so
                if (SaveACL)
                {
                    App.Settings.ACLPrivateSelectedContacts = GetSelectedContactsString();
                    App.Settings.ACLPrivateSelectedGroups = GetSelectedGroupsString();
                }

                // transfer newPost to App.PostFriendica and start sending
                App.PostFriendica = new PostFriendicaNewPost();
                App.PostFriendica.FriendicaNewPostSent += PostFriendica_FriendicaNewPostSent;
                IsSendingNewPost = true;
                App.PostFriendica.PostFriendicaStatus(newPost);

                // fire event triggering the navigation back to origin page
                if (SendingNewPostStarted != null)
                    SendingNewPostStarted(this, EventArgs.Empty);
            }
        }


        private async void PostFriendica_FriendicaNewPostSent(object sender, EventArgs e)
        {
            // post was sent to server, inform user before app close
            IsSendingNewPost = false;

            if (App.NewPostTriggered == NewPostTrigger.ToastNotification || App.NewPostTriggered == NewPostTrigger.SpeechCommand)
            {
                string errorMsg;
                errorMsg = loader.GetString("messageDialogNewPostSentAppClosing");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);

                // exit app
                Application.Current.Exit();
            }
        }


        // load image button
        Mvvm.Command _loadImageCommand;
        public Mvvm.Command LoadImageCommand { get { return _loadImageCommand ?? (_loadImageCommand = new Mvvm.Command(ExecuteLoadImage)); } }
        private async void ExecuteLoadImage()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                var imageProperties = await file.Properties.GetImagePropertiesAsync();
                if (imageProperties.Latitude != null)
                    ImageLatitude = (double)imageProperties.Latitude;
                else
                    ImageLatitude = 0.0;
                if (imageProperties.Longitude != null)
                    ImageLongitude = (double)imageProperties.Longitude;
                else
                    ImageLongitude = 0.0;
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                var image = new BitmapImage();
                image.SetSource(stream);
                NewPostImage = image;

                using (IRandomAccessStream inputStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var reader = new DataReader(inputStream);
                    var bytes = new byte[inputStream.Size];
                    await reader.LoadAsync((uint)inputStream.Size);
                    reader.ReadBytes(bytes);
                    newPost.NewPostMedia = bytes;
                }                
            }
        }

        // delete image button
        Mvvm.Command _deleteImageCommand;
        public Mvvm.Command DeleteImageCommand { get{ return _deleteImageCommand ?? (_deleteImageCommand = new Mvvm.Command(ExecuteDeleteImage)); } }
        private void ExecuteDeleteImage()
        {
            NewPostImage = null;
            newPost.NewPostMedia = null;
            ImageLatitude = 0.0;
            ImageLongitude = 0.0;
        }


        public NewPostViewmodel()
        {
            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;

            var stringSendCoordinates = App.Settings.SendCoordinatesAllowed;
            if (stringSendCoordinates == "true")
                SendCoordinates = true;
            else
                SendCoordinates = false;

            // load state of radiobuttons from App.Settings
            RadioButtonPublicPostChecked = App.Settings.ACLPublicPost;
            RadioButtonPrivatePostChecked = App.Settings.ACLPrivatePost;

            App.ContactsLoaded += App_ContactsLoaded;

            _selectedContacts = GetSelectedContactsFromSettings();
            OnPropertyChanged("SelectedContacts");
            _selectedGroups = GetSelectedGroupsFromSettings();
            OnPropertyChanged("SelectedGroups");
        }

        private void App_ContactsLoaded(object sender, EventArgs e)
        {
            OnPropertyChanged("Contacts");
            OnPropertyChanged("Groups");
            // set SelectedContacts and SelectedGroups with values from App.Settings
            _selectedContacts = GetSelectedContactsFromSettings();
            OnPropertyChanged("SelectedContacts");
            _selectedGroups = GetSelectedGroupsFromSettings();
            OnPropertyChanged("SelectedGroups");
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {;
            if (e.PropertyName == "ShellWidth")
                OnPropertyChanged("ListViewWidth");
            if (e.PropertyName == "OrientationDevice")
                OnPropertyChanged("ListViewWidth");
        }

        private List<FriendicaUserExtended> GetSelectedContactsFromSettings()
        {
            var selectedContacts = new List<FriendicaUserExtended>();
            var settings = App.Settings.ACLPrivateSelectedContacts;
            if (settings != null && settings != "")
            {
                string[] cids = Regex.Split(settings, @"<");
                foreach (var cid in cids)
                {
                    if (cid != "")
                    {
                        string cidClean = cid;
                        cidClean = cidClean.Replace("<", "");
                        cidClean = cidClean.Replace(">", "");
                        double cidDouble = Convert.ToDouble(cidClean);
                        if (App.ContactsFriends != null && App.ContactsFriends.Count > 0)
                            selectedContacts.Add(App.ContactsFriends.Single(c => c.User.UserCid == cidDouble));
                    }
                }
            }
            return selectedContacts;
        }

        private List<FriendicaGroup> GetSelectedGroupsFromSettings()
        {
            var selectedGroups = new List<FriendicaGroup>();
            var settings = App.Settings.ACLPrivateSelectedGroups;
            if (settings != null && settings != "")
            {
                string[] gids = Regex.Split(settings, @"<");
                foreach (var gid in gids)
                {
                    if (gid != "")
                    {
                        string gidClean = gid;
                        gidClean = gidClean.Replace("<", "");
                        gidClean = gidClean.Replace(">", "");
                        double gidDouble = Convert.ToDouble(gidClean);
                        if (App.ContactsGroups != null && App.ContactsGroups.Count > 0)
                            selectedGroups.Add(App.ContactsGroups.Single(g => g.GroupGid == gidDouble));
                    }
                }
            }
            return selectedGroups;
        }

        private async void GetLocation()
        {
            if (SendCoordinates)
            {
                var geolocator = new Geolocator();
                try
                {
                    var position = await geolocator.GetGeopositionAsync();
                    LocationLatitude = position.Coordinate.Latitude;
                    LocationLongitude = position.Coordinate.Longitude;
                }
                catch
                { }
            }
            else
            {
                LocationLatitude = 0.0;
                LocationLongitude = 0.0;
            }
        }

        public void LoadThread()
        {
            var threadId = GetThreadId();

            foreach (var thread in App.NetworkThreads)
            {
                if (thread.ThreadId == threadId)
                {
                    foreach (var post in thread.Posts)
                    {
                        if (!post.IsComment)
                            post.ToggleShowCommentsState = false;
                    }
                    ShowThread.Add(thread);
                }
                thread.ReduceComments();
            }
            foreach (var thread in App.HomeThreads)
            {
                if (thread.ThreadId == threadId)
                {
                    foreach (var post in thread.Posts)
                    {
                        if (!post.IsComment)
                            post.ToggleShowCommentsState = false;
                    }
                    ShowThread.Add(thread);
                }
                thread.ReduceComments();
            }
        }

        private double GetThreadId()
        {
            double id = 0;
            if (PostToShow.Post.PostInReplyToStatusId == 0)
                id = PostToShow.Post.PostId;
            else if (PostToShow.Post.PostInReplyToStatusId != 0)
                id = Convert.ToDouble(PostToShow.Post.PostInReplyToStatusIdStr);
            return id;
        }
    }
}