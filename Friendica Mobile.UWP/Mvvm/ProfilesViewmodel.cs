using Friendica_Mobile.UWP.HttpRequests;
using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.Web.Http;


namespace Friendica_Mobile.UWP.Mvvm
{
    public class ProfilesViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        
        public Thickness BottomAppBarMargin { get { return App.Settings.BottomAppBarMargin; } }
        public double ListViewWidth { get { return App.Settings.ShellWidth; } }

        // contains the profiles from the server
        private ObservableCollection<FriendicaProfile> _profiles;
        public ObservableCollection<FriendicaProfile> Profiles
        {
            get { return _profiles; }
            set { _profiles = value;
                OnPropertyChanged("Profiles"); }
        }

        // following used to save profiles on navigating from and restoring on navigating to 30_Profiles.xaml
        private List<FriendicaProfile> _saveProfiles;
        private FriendicaProfile _saveSelectedProfile;

        // contains the selected Conversation
        private FriendicaProfile _selectedProfile;
        public FriendicaProfile SelectedProfile
        {
            get { return _selectedProfile; }
            set { _selectedProfile = value;
                ShowUserlistCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedProfile"); }
        }

        // indicator showing if multiprofile is enabled - profile selector then enabled
        private bool _isMultiProfileEnabled;
        public bool IsMultiProfileEnabled
        {
            get { return _isMultiProfileEnabled; }
            set { _isMultiProfileEnabled = value;
                OnPropertyChanged("IsMultiProfileEnabled"); }
        }

        // url for the global Directory from the server
        private string _globalDirectory;
        public string GlobalDirectory
        {
            get { return _globalDirectory; }
            set { _globalDirectory = value;
                OnPropertyChanged("GlobalDirectory"); }
        }

        // indicator showing that selected profile is public
        private bool _selectedProfileIsPublic;
        public bool SelectedProfileIsPublic
        {
            get { return _selectedProfileIsPublic; }
            set { _selectedProfileIsPublic = value;
                OnPropertyChanged("SelectedProfileIsPublic");
            }
        }


        // indicator showing if editor mode is enabled - wird erst später implementiert
        private bool _isEditorEnabled;
        public bool IsEditorEnabled
        {
            get { return _isEditorEnabled; }
            set { _isEditorEnabled = value;
                OnPropertyChanged("IsEditorEnabled"); }
        }

        // indicator if no settings available (then display sample conversations)
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set { _noSettings = value;
                OnPropertyChanged("NoSettings"); }
        }

        // indicator if server is not supporting messages API for App
        private bool _noServerSupport;
        public bool NoServerSupport
        {
            get { return _noServerSupport; }
            set { _noServerSupport = value;
                CreateProfileCommand.RaiseCanExecuteChanged();
                DeleteProfileCommand.RaiseCanExecuteChanged();
                UploadProfileImageCommand.RaiseCanExecuteChanged();
                EnableEditorCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("NoServerSupport"); }
        }

        // indicator on top for loading initial conversations
        private bool _isLoadingProfiles;
        public bool IsLoadingProfiles
        {
            get { return _isLoadingProfiles; }
            set { _isLoadingProfiles = value;
                CreateProfileCommand.RaiseCanExecuteChanged();
                DeleteProfileCommand.RaiseCanExecuteChanged();
                UploadProfileImageCommand.RaiseCanExecuteChanged();
                EnableEditorCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingProfiles"); }
        }

        // set Property if refresh is currently in progress (showing progress bar and preventing from clicking Refresh again)
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                CreateProfileCommand.RaiseCanExecuteChanged();
                DeleteProfileCommand.RaiseCanExecuteChanged();
                UploadProfileImageCommand.RaiseCanExecuteChanged();
                EnableEditorCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsRefreshing");
            }
        }

        // indicator showing that app is sending profile updates to server
        private bool _isSendingProfileUpdates;
        public bool IsSendingProfileUpdates
        {
            get { return _isSendingProfileUpdates; }
            set { _isSendingProfileUpdates = value;
                CreateProfileCommand.RaiseCanExecuteChanged();
                DeleteProfileCommand.RaiseCanExecuteChanged();
                UploadProfileImageCommand.RaiseCanExecuteChanged();
                EnableEditorCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsSendingProfileUpdates"); }
        }

        // event handlers for 
        //      creating a new profile
        public event EventHandler ButtonCreateProfileClicked;


        // add a new profile button
        Mvvm.Command _createProfileCommand;
        public Mvvm.Command CreateProfileCommand { get { return _createProfileCommand ?? (_createProfileCommand = new Mvvm.Command(ExecuteCreateProfile, CanCreateProfile)); } }
        private bool CanCreateProfile()
        {
                return true;
        }

        private async void ExecuteCreateProfile()
        {
            //TODO: später implementieren

            // message to user: "no adding possible in this version"
            string errorMsg;
            errorMsg = loader.GetString("messageDialogProfilesAddingNotSupported");
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }

        // delete a profile button
        Mvvm.Command _deleteProfileCommand;
        public Mvvm.Command DeleteProfileCommand { get { return _deleteProfileCommand ?? (_deleteProfileCommand = new Mvvm.Command(ExecuteDeleteProfile, CanDeleteProfile)); } }
        private bool CanDeleteProfile()
        {
            return true;
        }

        private async void ExecuteDeleteProfile()
        {
            //TODO: später implementieren
            //ButtonCreateProfileClicked?.Invoke(this, EventArgs.Empty);

            // message to user: "no deleting possible in this version"
            string errorMsg;
            errorMsg = loader.GetString("messageDialogProfilesDeletingNotSupported");
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }


        // upload a new profile photo button
        Mvvm.Command _uploadProfileImageCommand;
        public Mvvm.Command UploadProfileImageCommand { get { return _uploadProfileImageCommand ?? (_uploadProfileImageCommand = new Mvvm.Command(ExecuteUploadProfileImage, CanUploadProfileImage)); } }
        private bool CanUploadProfileImage()
        {
            return true;
        }

        private async void ExecuteUploadProfileImage()
        {
            //TODO: später implementieren

            // message to user: "no photo manipulation possible in this version"
            string errorMsg;
            errorMsg = loader.GetString("messageDialogProfilesChangeImageNotSupported");
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }

        // button for showing users for a profile
        Mvvm.Command _showUserlistCommand;
        public Mvvm.Command ShowUserlistCommand { get { return _showUserlistCommand ?? (_showUserlistCommand = new Mvvm.Command(ExecuteShowUserlist, CanShowUserlist)); } }
        private bool CanShowUserlist()
        {
            return (App.IsLoadedContacts && SelectedProfile != null && !SelectedProfile.ProfileIsDefault);
        }

        private void ExecuteShowUserlist()
        {
            // navigate to view A4_ProfileUsers.xaml
            var frame = App.GetFrameForNavigation();
            frame.Navigate(typeof(Views.A4_ProfileUsers), SelectedProfile);
        }



        // toggle button - enabling editor
        Mvvm.Command _enableEditorCommand;
        public Mvvm.Command EnableEditorCommand { get { return _enableEditorCommand ?? (_enableEditorCommand = new Mvvm.Command(ExecuteEnableEditor, CanEnableEditor)); } }
        private bool CanEnableEditor()
        {
                return false;
        }

        private void ExecuteEnableEditor()
        {
            // TODO: später implementieren
        }


        // send profile data command
        Mvvm.Command _sendProfileCommand;
        public Mvvm.Command SendProfileCommand { get { return _sendProfileCommand ?? (_sendProfileCommand = new Mvvm.Command(ExecuteSendProfile, CanSendProfile)); } }
        private bool CanSendProfile()
        {
            //if (NewMessageContent != null && NewMessageContent != "\r\r")
            //{
            //    if (IsStartingNewConversation && SelectedContact == null)
            //        return false;
            //    else
            //    {
            //        App.NavStatus = NavigationStatus.NewMessageChanged;
            //        return true;
            //    }
            //}
            //else
                return false;
        }

        private async void ExecuteSendProfile()
        {
            //// if we are displaying sample data we cannot send a message to the server
            //if (NoSettings)
            //{
            //    // message to user: "no sending possible in test mode"
            //    string errorMsg;
            //    errorMsg = loader.GetString("messageDialogMessagesNewNoSettings");
            //    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            //    await dialog.ShowDialog(0, 0);
            //}
            //else
            //{
            //    var message = CreateMessage();
            //    ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);

            //    IsSendingNewMessage = true;
            //    // sending new message to server
            //    var postMessage = new PostFriendicaMessage();
            //    postMessage.FriendicaMessageSent += PostMessage_FriendicaMessageSent;
            //    postMessage.PostFriendicaMessageNew(message);
            //}
        }

        private async void PostMessage_FriendicaMessageSent(object sender, EventArgs e)
        {
            //var postMessage = sender as PostFriendicaMessage;
            //if (!postMessage.IsSuccessStateCode)
            //{
            //    IsSendingNewMessage = false;
            //    // message to user: "there was an error in sending the message"
            //    string errorMsg;
            //    errorMsg = String.Format(loader.GetString("messageDialogMessagesNewErrorSending"), postMessage.ErrorMessage);
            //    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            //    await dialog.ShowDialog(0, 0);
            //}
            //else
            //{
            //    IsSendingNewMessage = false;
            //    IsEditorEnabled = false;
            //    App.NavStatus = NavigationStatus.OK;
            //    ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);

            //    if (postMessage.NewMessage.NewMessageReplyTo != "")
            //    {
            //        // we were answering an existing message - reload conversation messages
            //        SelectedConversation.NewMessageAdded += Conv_NewMessageAdded;
            //        SelectedConversation.ReloadConversation();
            //    }
            //    else
            //    {
            //        // we were creating a new conversation - load new conversations and set selection
            //        LoadMessagesNew();
            //        IsStartingNewConversation = false;
            //    }
            //}
        }


        public ProfilesViewmodel()
        {
            Profiles = new ObservableCollection<FriendicaProfile>();
            _saveProfiles = new List<FriendicaProfile>();

            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;
            
            // check if there is a setting for the server, otherwise we will use sample data for the user
            CheckServerSettings();

            App.ContactsLoaded += App_ContactsLoaded;
        }


        private void CheckServerSettings()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                NoSettings = true;
                PrepareSampleData();
            }
            else
                NoSettings = false;
        }


        private void PrepareSampleData()
        {
            // load sample data from separate class and save it in Observable Collection
            var sampleData = new FriendicaProfileSamples();
            Profiles = sampleData.ProfileSamples;
            IsMultiProfileEnabled = sampleData.MultiProfiles;
            GlobalDirectory = sampleData.GlobalDirectory;
            foreach (var profile in sampleData.ProfileSamples)
                _saveProfiles.Add(profile);
            SelectedProfile = Profiles.Single(p => p.ProfileIsDefault == true);
        }


        private void SetNavigationStatus()
        {
            // TODO: implement with editing/creating profiles
            //if (NewMessageTitle != null || (NewMessageContent != null && NewMessageContent != "\r\r") || SelectedContact != null)
            //{
            //    App.NavStatus = NavigationStatus.NewMessageChanged;
            //}
            //else
            //{
            //    if (App.NavStatus == NavigationStatus.NewMessageChanged)
            //        App.NavStatus = NavigationStatus.OK;
            //}
            //SendMessageCommand.RaiseCanExecuteChanged();
        }


        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth" || e.PropertyName == "OrientationDevice")
                OnPropertyChanged("ListViewWidth");
        }

        private void App_ContactsLoaded(object sender, EventArgs e)
        {
            ShowUserlistCommand.RaiseCanExecuteChanged();
        }

        public void SaveProfiles()
        {
            // TODO: issue with navigating between pages - "is already child of another element" errors 
            //_saveSelectedProfile = new FriendicaProfile();
            //_saveSelectedProfile.ProfileHomepage = SelectedProfile.ProfileHomepage;
            //SelectedProfile.ProfileHomepage = "";
            //_saveProfiles = new List<FriendicaProfile>();
            //foreach (var profile in Profiles)
            //{
            //    var profileNew = new FriendicaProfile();
            //    profileNew.ProfileIsDefault = profile.ProfileIsDefault;
            //    profileNew.ProfileHomepage = profile.ProfileHomepage;
            //    profile.ProfileHomepage = "";
            //    _saveProfiles.Add(profileNew);
            //}
            //Profiles = null;
            //SelectedProfile = null;
        }


        public void RestoreProfiles()
        {
            // TODO: issue with navigating between pages - "is already child of another element" errors 
            //Profiles = new ObservableCollection<FriendicaProfile>();
            ////SelectedProfile = new FriendicaProfile();

            //foreach (var profile in _saveProfiles)
            //{
            //    //    profile.ClearOutParagraphFields();
            //    //    profile.RecreateParagraphFields();
            //    var profileNew = new FriendicaProfile();
            //    profileNew.ProfileIsDefault = profile.ProfileIsDefault;
            //    profileNew.ProfileHomepage = "";
            //    profileNew.ProfileHomepage = profile.ProfileHomepage;
            //    Profiles.Add(profileNew);
            //}
            //SelectedProfile = Profiles.Single(p => p.ProfileIsDefault == true);
            //SelectedProfile.RecreateParagraphFields();
            ////if (_saveSelectedProfile != null)
            ////{
            ////    SelectedProfile = _saveSelectedProfile;
            ////}
            ////else
            ////    SelectedProfile = Profiles.Single(p => p.ProfileIsDefault == true);
        }


        public void LoadProfiles()
        {
            if (!NoSettings && !IsLoadingProfiles)
            {
                // load data from server
                IsLoadingProfiles = true;
                var getProfiles = new GetFriendicaProfiles();
                getProfiles.FriendicaProfilesLoaded += GetProfiles_FriendicaProfilesLoaded;
                getProfiles.LoadProfiles();
            }
        }

        private async void GetProfiles_FriendicaProfilesLoaded(object sender, EventArgs e)
        {
            var getProfiles = sender as GetFriendicaProfiles;

            switch (getProfiles.ErrorProfileFriendica)
            {
                case GetFriendicaProfiles.ProfileErrors.OK:
                    foreach (var profile in getProfiles.ProfilesReturned)
                    {
                        Profiles.Add(profile);
                        if (App.GetNameOfCurrentView() != "30_Profiles")
                            _saveProfiles.Add(profile);
                    }
                    IsMultiProfileEnabled = getProfiles.MultiProfiles;
                    GlobalDirectory = getProfiles.GlobalDirectory;
                    SelectedProfile = Profiles.Single(p => p.ProfileIsDefault == true);
                    break;
                case GetFriendicaProfiles.ProfileErrors.NoServerSupport:
                    NoServerSupport = true;
                    this.IsMultiProfileEnabled = false;
                    this.GlobalDirectory = "";
                    Profiles = new ObservableCollection<FriendicaProfile>();
                    break;
                case GetFriendicaProfiles.ProfileErrors.AuthenticationFailed:
                case GetFriendicaProfiles.ProfileErrors.ProfileIdNotFound:
                case GetFriendicaProfiles.ProfileErrors.UnknownError:
                default:
                    // message to user: "error on loading profiles"
                    NoServerSupport = true;
                    var errorMsg = String.Format(loader.GetString("messageDialogProfilesUnknown error"), getProfiles.StatusCode, getProfiles.ErrorMessage);
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                    break;
            }

            IsLoadingProfiles = false;
        }

    }
}