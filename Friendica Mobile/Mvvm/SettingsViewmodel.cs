using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using Friendica_Mobile.PCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Friendica_Mobile.Mvvm
{
    class SettingsViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        
        public String DeviceFamily { get { return App.Settings.DeviceFamily; } }
        public String NavigationSide
        {
            get { return App.Settings.NavigationSide; }
            set
            {
                App.Settings.NavigationSide = value;
                OnPropertyChanged("NavigationSide");
            }
        }

        public string StartPage
        {
            get { return App.Settings.StartPage; }
            set
            {
                App.Settings.StartPage = value;
                OnPropertyChanged("StartPage");
                OnPropertyChanged("IsVisibleNetworkMode");
            }
        }

        public bool IsVisibleNetworkMode
        {
            // selection now disabled because we do not enable the chronological selection anymore
            get
            {
                if (StartPage == "Network_old")
                    return true;
                else
                    return false;
            }
        }

        // display selection of user for mode of displaying items on network page
        public string NetworkMode
        {
            get { return App.Settings.NetworkMode; }
            set
            {
                App.Settings.NetworkMode = value;
                OnPropertyChanged("NetworkMode");
            }
        }

        private bool _friendicaServerCorrect;
        private bool _friendicaUsernameCorrect;
        private bool _friendicaPasswordCorrect;

        private string _friendicaServerHint;
        public string FriendicaServerHint
        {
            get { return _friendicaServerHint; }
            set { _friendicaServerHint = value; OnPropertyChanged("FriendicaServerHint"); }
        }
        private bool _friendicaServerWarningSign;
        public bool FriendicaServerWarningSign
        { 
            get { return _friendicaServerWarningSign; }
            set { _friendicaServerWarningSign = value; OnPropertyChanged("FriendicaServerWarningSign"); }
        }
        private SolidColorBrush _friendicaServerWarningColor;
        public SolidColorBrush FriendicaServerWarningColor
        {
            get { return _friendicaServerWarningColor; }
            set { _friendicaServerWarningColor = value; OnPropertyChanged("FriendicaServerWarningColor"); }
        }

        private string _friendicaUsernameHint;
        public string FriendicaUsernameHint
        {
            get { return _friendicaUsernameHint; }
            set { _friendicaUsernameHint = value; OnPropertyChanged("FriendicaUsernameHint"); }
        }
        private bool _friendicaUsernameWarningSign;
        public bool FriendicaUsernameWarningSign
        {
            get { return _friendicaUsernameWarningSign; }
            set { _friendicaUsernameWarningSign = value; OnPropertyChanged("FriendicaUsernameWarningSign"); }
        }
        private SolidColorBrush _friendicaUsernameWarningColor;
        public SolidColorBrush FriendicaUsernameWarningColor
        {
            get { return _friendicaUsernameWarningColor; }
            set { _friendicaUsernameWarningColor = value; OnPropertyChanged("FriendicaUsernameWarningColor"); }
        }

        private string _friendicaPasswordHint;
        public string FriendicaPasswordHint
        {
            get { return _friendicaPasswordHint; }
            set { _friendicaPasswordHint = value; OnPropertyChanged("FriendicaPasswordHint"); }
        }
        private bool _friendicaPasswordWarningSign;
        public bool FriendicaPasswordWarningSign
        {
            get { return _friendicaPasswordWarningSign; }
            set { _friendicaPasswordWarningSign = value; OnPropertyChanged("FriendicaPasswordWarningSign"); }
        }
        private SolidColorBrush _friendicaPasswordWarningColor;
        public SolidColorBrush FriendicaPasswordWarningColor
        {
            get { return _friendicaPasswordWarningColor; }
            set { _friendicaPasswordWarningColor = value; OnPropertyChanged("FriendicaPasswordWarningColor"); }
        }

        private bool _testConnectionInProgress;
        public bool TestConnectionInProgress
        {
            get { return _testConnectionInProgress; }
            set { _testConnectionInProgress = value;
                // set NavStatus if connection is running, prevents user from navigating away during connection test
                // setting back to OK is done in event handler on reaching a positive test result only
                if (_testConnectionInProgress)
                    App.NavStatus = NavigationStatus.SettingsChanged;
                OnPropertyChanged("TestConnectionInProgress"); }
        }

        private string _testConnectionResult;
        public string TestConnectionResult
        {
            get { return _testConnectionResult; }
            set { _testConnectionResult = value; OnPropertyChanged("TestConnectionResult"); }
        }

        private bool _testConnectionResultVisible;
        public bool TestConnectionResultVisible
        {
            get { return _testConnectionResultVisible; }
            set { _testConnectionResultVisible = value; OnPropertyChanged("TestConnectionResultVisible"); }
        }

        private Brush _testConnectionResultColor;
        public Brush TestConnectionResultColor
        {
            get { return _testConnectionResultColor; }
            set { _testConnectionResultColor = value; OnPropertyChanged("TestConnectionResultColor"); }
        }

        private string _friendicaServer;
        public string FriendicaServer
        {
            get { return _friendicaServer; }
            set
            {
                _friendicaServer = value;
                // set NavStatus to SettingsChanged to prevent user from navigating away if new value is different to stored value
                if (_friendicaServer == App.Settings.FriendicaServer)
                    App.NavStatus = NavigationStatus.OK;
                else
                    App.NavStatus = NavigationStatus.SettingsChanged;

                if (value == "")
                {
                    FriendicaServerWarningSign = true;
                    //FriendicaServerHint = "Friendica Server darf nicht leer sein.";
                    FriendicaServerHint = loader.GetString("FriendicaServerHintEmpty");
                    FriendicaServerWarningColor = new SolidColorBrush(Colors.Red);
                    _friendicaServerCorrect = false;
                }
                else if (value == "http://" || value == "https://")
                {
                    FriendicaServerWarningSign = true;
                    //FriendicaServerHint = "Friendica Server muss eine vollständige URL enthalten.";
                    FriendicaServerHint = loader.GetString("FriendicaServerHintOnlyHttp");
                    FriendicaServerWarningColor = new SolidColorBrush(Colors.Red);
                    _friendicaServerCorrect = false;
                }
                else if (!value.StartsWith("http://") && !value.StartsWith("https://"))
                {
                    FriendicaServerWarningSign = true;
                    // FriendicaServerHint = "Friendica Server muss mit 'http://' oder 'https://' beginnen.";
                    FriendicaServerHint = loader.GetString("FriendicaServerHintWrongStart");
                    FriendicaServerWarningColor = new SolidColorBrush(Colors.Red);
                    _friendicaServerCorrect = false;
                }
                else if (value.EndsWith("/"))
                {
                    FriendicaServerWarningSign = true;
                    //FriendicaServerHint = "Am Ende der Friendica Server URL sollte kein Schrägstrich stehen.";
                    FriendicaServerHint = loader.GetString("FriendicaServerHintEndingSlash");
                    FriendicaServerWarningColor = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
                    _friendicaServerCorrect = true;
                }
                else
                {
                    FriendicaServerWarningSign = false;
                    //FriendicaServerHint = "Geben Sie die URL ihres Friendica Servers ein (zB https://www.tryfriendica.de).";
                    FriendicaServerHint = loader.GetString("FriendicaServerHint");
                    FriendicaServerWarningColor = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
                    _friendicaServerCorrect = true;
                }
                SaveCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("FriendicaServer");
            }
        }

        private string _friendicaUsername;
        public string FriendicaUsername
        {
            get { return _friendicaUsername; }
            set
            {
                _friendicaUsername = value;
                // set NavStatus to SettingsChanged to prevent user from navigating away if new value is different to stored value
                if (_friendicaUsername == App.Settings.FriendicaUsername)
                    App.NavStatus = NavigationStatus.OK;
                else
                    App.NavStatus = NavigationStatus.SettingsChanged;

                if (value == "")
                {
                    FriendicaUsernameWarningSign = true;
                    //FriendicaUsernameHint = "Friendica Benutzername darf nicht leer sein.";
                    FriendicaUsernameHint = loader.GetString("FriendicaUsernameHintEmpty");
                    FriendicaUsernameWarningColor = new SolidColorBrush(Colors.Red);
                    _friendicaUsernameCorrect = false;
                }
                else
                {
                    FriendicaUsernameWarningSign = false;
                    //FriendicaUsernameHint = "Geben Sie den Benutzernamen oder die Mail-Adresse für Ihren Friendica-Zugang ein.";
                    FriendicaUsernameHint = loader.GetString("FriendicaUsernameHint");
                    FriendicaUsernameWarningColor = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
                    _friendicaUsernameCorrect = true;
                }
                SaveCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("FriendicaUsername");
            }
        }

        private string _friendicaPassword;
        public string FriendicaPassword
        {
            get { return _friendicaPassword; }
            set
            {
                _friendicaPassword = value;
                // set NavStatus to SettingsChanged to prevent user from navigating away if new value is different to stored value
                if (_friendicaPassword == App.Settings.FriendicaPassword)
                    App.NavStatus = NavigationStatus.OK;
                else
                    App.NavStatus = NavigationStatus.SettingsChanged;

                if (value == "")
                {
                    FriendicaPasswordWarningSign = true;
                    //FriendicaPasswordHint = "Friendica Passwort darf nicht leer sein.";
                    FriendicaPasswordHint = loader.GetString("FriendicaPasswordHintEmpty");
                    FriendicaPasswordWarningColor = new SolidColorBrush(Colors.Red);
                    _friendicaPasswordCorrect = false;
                }
                else
                {
                    FriendicaPasswordWarningSign = false;
                    //FriendicaPasswordHint = "Geben Sie das Passwort für Ihren Friendica-Zugang ein.";
                    FriendicaPasswordHint = loader.GetString("FriendicaPasswordHint");
                    FriendicaPasswordWarningColor = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseHighColor"]);
                    _friendicaPasswordCorrect = true;
                }
                SaveCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("FriendicaPassword");
            }
        }

        public bool RadioButtonPublicPostChecked
        {
            get { return App.Settings.ACLPublicPost; }
            set { App.Settings.ACLPublicPost = value;
                OnPropertyChanged("RadioButtonPublicPostChecked"); }
        }

        public bool RadioButtonPrivatePostChecked
        {
            get { return App.Settings.ACLPrivatePost; }
            set { App.Settings.ACLPrivatePost = value;
                OnPropertyChanged("RadioButtonPrivatePostChecked"); }
        }

        public ObservableCollection<FriendicaUserExtended> Contacts
        {
            get
            {
                //var obscoll = new ObservableCollection<FriendicaUserExtended>();
                //App.ContactsFriends.CopyTo(obscoll.ToArray(), 0);
                ////App.ContactsForums.CopyTo(obscoll.ToArray(), 0);
                return App.ContactsFriends; }
        }


        public ObservableCollection<FriendicaGroup> Groups
        {
            get { return App.ContactsGroups; }
        }

        private List<FriendicaUserExtended> _selectedContacts;
        public List<FriendicaUserExtended> SelectedContacts
        {
            get { return _selectedContacts; }
            set { _selectedContacts = value;
                SaveSelectedContactsToSettings();
                OnPropertyChanged("SelectedContacts"); }
        }

        private List<FriendicaGroup> _selectedGroups;
        public List<FriendicaGroup> SelectedGroups
        {
            get { return _selectedGroups; }
            set { _selectedGroups = value;
                SaveSelectedGroupsToSettings();
                OnPropertyChanged("SelectedGroups"); }
        }

        private void SaveSelectedContactsToSettings()
        {
            if (SelectedContacts != null)
            {
                var contacts = SelectedContacts.OrderBy(c => c.User.UserCid);
                string selectedContacts = "";
                foreach (var contact in contacts)
                {
                    selectedContacts += "<" + contact.User.UserCid + ">";
                }
                App.Settings.ACLPrivateSelectedContacts = selectedContacts;
            }
        }

        private void SaveSelectedGroupsToSettings()
        {
            if (SelectedGroups != null)
            {
                var groups = SelectedGroups.OrderBy(g => g.GroupGid);
                string selectedGroups = "";
                foreach (var group in groups)
                {
                    selectedGroups += "<" + group.GroupGid + ">";
                }
                App.Settings.ACLPrivateSelectedGroups = selectedGroups;
            }
        }

        private List<FriendicaUserExtended> GetSelectedContactsFromSettings()
        {
            var selectedContacts = new List<FriendicaUserExtended>();
            var settings = App.Settings.ACLPrivateSelectedContacts;
            if (settings != null && settings != "" && App.ContactsFriends != null)
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
            if (settings != null && settings != "" && App.ContactsGroups != null)
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
                        selectedGroups.Add(App.ContactsGroups.Single(g => g.GroupGid == gidDouble));
                    }
                }
            }
            return selectedGroups;
        }

        public String SaveLocalAllowed
        {
            get { return App.Settings.SaveLocalAllowed; }
            set { App.Settings.SaveLocalAllowed = value;
                OnPropertyChanged("SaveLocalAllowed"); }
        }

        public bool SaveFullsizePhotosAllowed
        {
            get { return App.Settings.SaveFullsizePhotosAllowed; }
            set { App.Settings.SaveFullsizePhotosAllowed = value;
                OnPropertyChanged("SaveFullsizePhotosAllowed"); }
        }

        // indicator used to show progress ring for user
        private bool _isDeletingLocalData;
        public bool IsDeletingLocalData
        {
            get { return _isDeletingLocalData; }
            set { _isDeletingLocalData = value;
                OnPropertyChanged("IsDeletingLocalData"); }
        }

        // checkbox for selecting to delete local database
        private bool _deleteLocalDatabaseChecked = true;
        public bool DeleteLocalDatabaseChecked
        {
            get { return _deleteLocalDatabaseChecked; }
            set { _deleteLocalDatabaseChecked = value;
                DeleteLocalDataCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("DeleteLocalDatabaseChecked");
            }
        }

        // checkbox for selecting to delete fullsize photos
        private bool _deleteFullsizePhotosChecked = true;
        public bool DeleteFullsizePhotosChecked
        {
            get { return _deleteFullsizePhotosChecked; }
            set { _deleteFullsizePhotosChecked = value;
                DeleteLocalDataCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("DeleteFullsizePhotosChecked");
            }
        }

        // checkbox for selecting to delete all other photo sizes
        private bool _deleteSmallMediumsizePhotosChecked = true;
        public bool DeleteSmallMediumsizePhotosChecked
        {
            get { return _deleteSmallMediumsizePhotosChecked; }
            set { _deleteSmallMediumsizePhotosChecked = value;
                DeleteLocalDataCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("DeleteSmallMediumsizePhotosChecked");
            }
        }
        
        public String SendCoordinatesAllowed
        {
            get { return App.Settings.SendCoordinatesAllowed; }
            set { App.Settings.SendCoordinatesAllowed = value;
                OnPropertyChanged("SendCoordinatesAllowed"); }
        }

        public bool NotificationActivated
        {
            get { return App.Settings.NotificationActivated; }
            set { App.Settings.NotificationActivated = value;
                OnPropertyChanged("NotificationActivated"); }
        }

        public bool NotificationShowMessageContent
        {
            get { return App.Settings.NotificationShowMessageContent; }
            set
            {
                App.Settings.NotificationShowMessageContent = value;
                OnPropertyChanged("NotificationShowMessageContent");
            }
        }

        public int NotificationFreshnessTime
        {
            get { return App.Settings.NotificationFreshnessTime; }
            set { App.Settings.NotificationFreshnessTime = value;
                OnPropertyChanged("NotificationFreshnessTime");
                OnPropertyChanged("NotificationFreshnessTimeHint"); }
        }

        public bool NotificationEachNewsfeedAllowed
        {
            get { return Settings.NotificationEachNewsfeedAllowed; }
            set { Settings.NotificationEachNewsfeedAllowed = value;
                OnPropertyChanged("NotificationEachNewsfeedAllowed");
            }
        }

        public string NotificationFreshnessTimeHint
        {
            get { return String.Format(loader.GetString("textblockNotificationFreshnessTimeHint"), NotificationFreshnessTime); }
        }

        public SettingsViewmodel()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == null)
                FriendicaServer = "https://";
            else
                FriendicaServer = App.Settings.FriendicaServer;

            FriendicaUsername = App.Settings.FriendicaUsername;
            FriendicaPassword = App.Settings.FriendicaPassword;

            // set SelectedContacts and SelectedGroups with values from App.Settings
            _selectedContacts = GetSelectedContactsFromSettings();
            OnPropertyChanged("SelectedContacts");
            _selectedGroups = GetSelectedGroupsFromSettings();
            OnPropertyChanged("SelectedGroups");
            App.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NotificationActivated")
                OnPropertyChanged("NotificationActivated");
        }

        Mvvm.Command _saveCommand;
        public Mvvm.Command SaveCommand { get { return _saveCommand ?? (_saveCommand = new Mvvm.Command(Execute, CanExecute)); } }
        private bool CanExecute()
        {
            if (_friendicaServerCorrect && _friendicaUsernameCorrect && _friendicaPasswordCorrect)
                return true;
            else
                return false;
        }

        private async void Execute()
        {
            // Testen der Verbindung
            var authenticationtest = new AuthenticationTest();
            TestConnectionInProgress = true;
            authenticationtest.UserAuthenticated += Authenticationtest_UserAuthenticated;
            var url = String.Format("{0}/api/account/verify_credentials?timestamp={1}", _friendicaServer, DateTime.Now.ToString());
            await authenticationtest.GetString(url, _friendicaUsername, _friendicaPassword);
        }

        Mvvm.Command _removeAdCommand;
        public Mvvm.Command RemoveAdCommand { get { return _removeAdCommand ?? (_removeAdCommand = new Mvvm.Command(ExecuteRemoveAd, CanRemoveAd)); } }
        private bool CanRemoveAd()
        {
            if (!App.Settings.PaidForRemovingAds)
                return true;
            else
                return false;
        }

        private async void ExecuteRemoveAd()
        {
            // if other things have been changed, do not overrule that setting
            var isOnlyPurchaseChanged = false;
            if (App.NavStatus != NavigationStatus.SettingsChanged)
            {
                isOnlyPurchaseChanged = true;
                App.NavStatus = NavigationStatus.SettingsChanged;
            }

            await App.detectTrial.BuyFeature();
            RemoveAdCommand.RaiseCanExecuteChanged();

            // only set back to ok if we don't overrule other things
            if (isOnlyPurchaseChanged)
                App.NavStatus = NavigationStatus.OK;
        }



        private async void Authenticationtest_UserAuthenticated(object sender, EventArgs e)
        {
            TestConnectionInProgress = false;
            var httpRequest = sender as AuthenticationTest;
            TestConnectionResultVisible = true;
            if (httpRequest.ServerAnswered && httpRequest.AuthenticationPassed)
            {
                // "Verbindung erfolgreich, Einstellungen wurden gespeichert!"
                TestConnectionResult = loader.GetString("TestConnectionResultOK");
                TestConnectionResultColor = new SolidColorBrush(Colors.Green);
                // set NavStatus back to OK if successfull, in order to allow navigating away again, all other results keep SettingsChanged
                App.NavStatus = NavigationStatus.OK;
                if (App.Settings.FriendicaServer != _friendicaServer || App.Settings.FriendicaUsername != _friendicaUsername)
                {
                    App.Settings.LastReadNetworkPost = 0.0;
                    App.Settings.LastNotifiedNetworkPost = 0.0;
                    App.Settings.LastNotifiedMessage = 0.0;
                    App.TileCounter.CounterUnseenHome = 0;
                    StaticGlobalParameters.CounterUnseenHome = 0;
                    StaticGlobalParameters.CounterUnseenNetwork = 0;
                    StaticGlobalParameters.CounterUnseenNewsfeed = 0;
                    StaticGlobalParameters.CounterUnseenMessages = 0;
                    App.NetworkVm = null;
                    // clear everything related to private messages
                    App.IsSendingNewMessage = false;
                    App.MessagesNavigatedIntoConversation = false;
                    App.MessagesVm = null;
                    App.TileCounter.CounterUnseenMessages = 0;
                    // clear everything related to photos
                    App.PhotosNavigatedIntoAlbum = false;
                    App.PhotosVm = null;
                    // delete locally stored data of the old user@server
                    await DeleteLocalDataAsync();
                }
                App.Settings.FriendicaServer = _friendicaServer;
                App.Settings.FriendicaUsername = _friendicaUsername;
                App.Settings.FriendicaPassword = _friendicaPassword;

                // reset contacts and groups as we have now different credentials
                App.ContactsFriends = new ObservableCollection<FriendicaUserExtended>();
                OnPropertyChanged("Contacts");
                App.ContactsForums = new ObservableCollection<FriendicaUserExtended>();
                App.ContactsGroups = new ObservableCollection<FriendicaGroup>();
                OnPropertyChanged("Groups");
                SelectedContacts = new List<FriendicaUserExtended>();
                SelectedGroups = new List<FriendicaGroup>();
                RadioButtonPublicPostChecked = false;
                RadioButtonPrivatePostChecked = false;

                // reload contacts from server
                var contacts = new ContactsViewmodel();
                contacts.ContactsLoaded += Contacts_ContactsLoaded;
                contacts.GroupsLoaded += Contacts_GroupsLoaded;
                contacts.InitialLoad();

                // reset home and network data
                App.HomePosts = new ObservableCollection<FriendicaPostExtended>();
                App.HomeThreads = new ObservableCollection<FriendicaThread>();
                App.NetworkPosts = new ObservableCollection<FriendicaPostExtended>();
                App.NetworkThreads = new ObservableCollection<FriendicaThread>();
            }
            else if (!httpRequest.ServerAnswered)
            {
                // Info das Server nicht geantwortet hat
                // ": Server hat nicht geantwortet!"
                TestConnectionResult = String.Format(loader.GetString("TestConnectionResultNotAnswered"), httpRequest.StatusCode);
                TestConnectionResultColor = new SolidColorBrush(Colors.Red);
            }
            else if (httpRequest.ServerAnswered && !httpRequest.AuthenticationPassed)
            {
                // Info das Authentication fehlgeschlagen
                // Authentifizierung am Server ist fehlgeschlagen, bitte prüfen Sie Ihre Angaben!
                TestConnectionResult = loader.GetString("TestConnectionResultNotAuthenticated");
                TestConnectionResultColor = new SolidColorBrush(Colors.Red);
            }
            else if (httpRequest.UnexpectedError)
            {
                // Info, dass es einen unerwarteten Fehler gegeben hat. 
                // {0}: ein unerwarteter Fehler ist aufgetreten. /n{1} ({2})
                TestConnectionResult = String.Format(loader.GetString("TestConnectionResultUnexpectedError"),
                    httpRequest.StatusCode,
                    httpRequest.ErrorMessage,
                    httpRequest.ErrorHResult);
                TestConnectionResultColor = new SolidColorBrush(Colors.Red);
            }
        }

        private void Contacts_GroupsLoaded(object sender, EventArgs e)
        {
            // save loaded groups to App and reload UI
            var contacts = sender as ContactsViewmodel;
            App.ContactsGroups = contacts.Groups;
            OnPropertyChanged("Groups");
        }

        private void Contacts_ContactsLoaded(object sender, EventArgs e)
        {
            // save loaded contacts to App and reload UI
            var contacts = sender as ContactsViewmodel;
            App.ContactsFriends = contacts.Friends;
            App.ContactsForums = contacts.Forums;
            OnPropertyChanged("Contacts");
        }

        // Button DeleteLocalDatabase
        Mvvm.Command _deleteLocalDataCommand;
        public Mvvm.Command DeleteLocalDataCommand { get { return _deleteLocalDataCommand ?? (_deleteLocalDataCommand = new Mvvm.Command(ExecuteDeleteLocalData, CanDeleteLocalData)); } }
        private bool CanDeleteLocalData()
        {
            if (DeleteLocalDatabaseChecked || DeleteFullsizePhotosChecked || DeleteSmallMediumsizePhotosChecked)
                return true;
            else
                return false;
        }
        private async void ExecuteDeleteLocalData()
        {
            await DeleteLocalDataAsync();
        }

        public async Task DeleteLocalDataAsync()
        {
            IsDeletingLocalData = true;

            string errorMsg = "";
            bool deleted = true;
            string localDatabaseError = "";
            string fullsizePhotosError = "";
            string smallMediumsizePhotosError = "";

            // delete database entries
            if (DeleteLocalDatabaseChecked)
            {
                var resultTruncate = App.sqliteConnection.TruncatetblAllKnownUser();
                if (resultTruncate)
                {
                    if (deleted)
                        deleted = true;   // don't overwrite if another element had an error
                }
                else
                {
                    deleted = false;
                    localDatabaseError = "Unknown error on deleting table from database."; // no localization, is very unexpectable
                }
            }

            // delete all fullsize images
            if (DeleteFullsizePhotosChecked)
            {
                string folderPath = "";
                try
                {
                    var folder = await GetScaleFolderAsync("0");
                    folderPath = folder.Path;
                    await folder.DeleteAsync();

                    if (deleted)
                        deleted = true;   // don't overwrite if another element had an error
                }
                catch
                {
                    deleted = false;
                    // no localization, is very unexpectable
                    fullsizePhotosError = String.Format("Error occurred on deleting the folder '{0}'.", folderPath);
                }
            }

            // delete all medium size and small size images
            if (DeleteSmallMediumsizePhotosChecked)
            {
                string folderPath = "";

                try
                {
                    var folder = await GetScaleFolderAsync("1");
                    folderPath = folder.Path;
                    await folder.DeleteAsync();

                    folder = await GetScaleFolderAsync("2");
                    folderPath = folder.Path;
                    await folder.DeleteAsync();

                    folder = await GetScaleFolderAsync("4");
                    folderPath = folder.Path;
                    await folder.DeleteAsync();

                    folder = await GetScaleFolderAsync("5");
                    folderPath = folder.Path;
                    await folder.DeleteAsync();

                    folder = await GetScaleFolderAsync("6");
                    folderPath = folder.Path;
                    await folder.DeleteAsync();

                    if (deleted)
                        deleted = true;   // don't overwrite if another element had an error
                }
                catch
                {
                    deleted = false;
                    // no localization, is very unexpectable
                    fullsizePhotosError = String.Format("Error occurred on deleting the folder '{0}'.", folderPath);
                }
            }

            // give success/error message to user
            if (deleted)
            {
                // all successfully deleted
                errorMsg = loader.GetString("messageDialogLocalDataDeleted") + "\n";
                if (DeleteLocalDatabaseChecked)
                    errorMsg += String.Format("\n     {0}", loader.GetString("messageDialogDeletedLocalDatabase"));
                if (DeleteFullsizePhotosChecked)
                    errorMsg += String.Format("\n     {0}", loader.GetString("messageDialogDeletedFullsizePhotos"));
                if (DeleteSmallMediumsizePhotosChecked)
                    errorMsg += String.Format("\n     {0}", loader.GetString("messageDialogDeletedSmallMediumsizePhotos"));
            }
            else
            {
                // all successfully deleted
                errorMsg = loader.GetString("messageDialogLocalDataDeletedError") + "\n";
                if (DeleteLocalDatabaseChecked && localDatabaseError != "")
                    errorMsg += String.Format("\n     {0}: {1}", loader.GetString("messageDialogDeletedLocalDatabase"), localDatabaseError);
                if (DeleteFullsizePhotosChecked && fullsizePhotosError != "")
                    errorMsg += String.Format("\n     {0}: {1}", loader.GetString("messageDialogDeletedFullsizePhotos"), fullsizePhotosError);
                if (DeleteSmallMediumsizePhotosChecked && smallMediumsizePhotosError != "")
                    errorMsg += String.Format("\n     {0}: {1}", loader.GetString("messageDialogDeletedSmallMediumsizePhotos"), smallMediumsizePhotosError);
            }

            IsDeletingLocalData = false;
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);


            // set all options back to true after deleting was performed
            DeleteLocalDatabaseChecked = true;
            DeleteFullsizePhotosChecked = true;
            DeleteSmallMediumsizePhotosChecked = true;
        }

        // Button DeleteLocalDatabase - Cancel
        Mvvm.Command _cancelDeleteLocalDataCommand;
        public Mvvm.Command CancelDeleteLocalDataCommand { get { return _cancelDeleteLocalDataCommand ?? (_cancelDeleteLocalDataCommand = new Mvvm.Command(ExecuteCancelDeleteLocalData)); } }
        private void ExecuteCancelDeleteLocalData()
        {
            // set all options back to true after deleting was performed
            DeleteLocalDatabaseChecked = true;
            DeleteFullsizePhotosChecked = true;
            DeleteSmallMediumsizePhotosChecked = true;
        }

        private async Task<StorageFolder> GetScaleFolderAsync(string scale)
        {
            scale = (scale == "false") ? "0" : scale;

            StorageFolder cacheBaseFolder = ApplicationData.Current.LocalCacheFolder;
            var folder = await cacheBaseFolder.CreateFolderAsync(scale, CreationCollisionOption.OpenIfExists);
            return folder;
        }

    }
}
