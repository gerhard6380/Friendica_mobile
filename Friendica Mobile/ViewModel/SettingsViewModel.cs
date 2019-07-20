using System.Threading.Tasks;
using System.Windows.Input;
using Friendica_Mobile.Views;
using Friendica_Mobile.Strings;
using Xamarin.Forms;
using Friendica_Mobile.HttpRequests;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using Friendica_Mobile.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Friendica_Mobile.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        private bool _credentialsOk;

        private string _friendicaServer;
        public string FriendicaServer
        {
            get { return _friendicaServer; }
            set
            {
                SetProperty(ref _friendicaServer, value);
                CheckCredentials();
            }
        }

        public enum FriendicaServerHints { Standard, Empty, EndingSlash, OnlyHttp, WrongStart, NoHttps };
        private FriendicaServerHints _friendicaServerHint = FriendicaServerHints.Standard;
        public FriendicaServerHints FriendicaServerHint
        {
            get { return _friendicaServerHint; }
            set { SetProperty(ref _friendicaServerHint, value); }
        }


        private string _friendicaUsername = Settings.FriendicaUsername;
        public string FriendicaUsername
        {
            get { return _friendicaUsername; }
            set
            {
                SetProperty(ref _friendicaUsername, value);
                CheckCredentials();
            }
        }

        public enum FriendicaUsernameHints { Standard, Empty };
        private FriendicaUsernameHints _friendicaUsernameHint = FriendicaUsernameHints.Standard;
        public FriendicaUsernameHints FriendicaUsernameHint
        {
            get { return _friendicaUsernameHint; }
            set { SetProperty(ref _friendicaUsernameHint, value); }
        }

        private string _friendicaPassword = Settings.FriendicaPassword;
        public string FriendicaPassword
        {
            get { return _friendicaPassword; }
            set
            {
                SetProperty(ref _friendicaPassword, value);
                CheckCredentials();
            }
        }

        public enum FriendicaPasswordHints { Standard, Empty };
        private FriendicaPasswordHints _friendicaPasswordHint = FriendicaPasswordHints.Standard;
        public FriendicaPasswordHints FriendicaPasswordHint
        {
            get { return _friendicaPasswordHint; }
            set { SetProperty(ref _friendicaPasswordHint, value); }
        }

        private bool _testConnectionResultVisible;
        public bool TestConnectionResultVisible
        {
            get { return _testConnectionResultVisible; }
            set { SetProperty(ref _testConnectionResultVisible, value); }
        }

        public bool IsUsingSystemTheme
        {
            get { return Settings.AppThemeUseSystemTheme; }
            set { Settings.AppThemeUseSystemTheme = value; }
        }

        public bool IsDarkModeEnabled
        {
            get { return Settings.AppThemeDarkModeEnabled; }
            set { Settings.AppThemeDarkModeEnabled = value; }
        }

        private bool _isNavigationSideVisible;
        public bool IsNavigationSideVisible
        {
            get { return _isNavigationSideVisible; }
            set { SetProperty(ref _isNavigationSideVisible, value); }
        }

        public bool IsNavigationOnRightSide
        {
            get { return Settings.NavigationOnRightSide; }
            set
            {
                Settings.NavigationOnRightSide = value;
                OnPropertyChanged("LabelNavigationSide");
            }
        }

        public string LabelNavigationSide
        {
            get { return (Settings.NavigationOnRightSide) ? AppResources.LabelSettingsNavigationSideRight : AppResources.LabelSettingsNavigationSideLeft; }
        }

        public int StartPage
        {
            get { return ConvertStartPageToInt(); }
            set { Settings.StartPage = ConvertIntToStartPage(value); }
        }

        private int ConvertStartPageToInt()
        {
            var page = Settings.StartPage;
            switch (page)
            {
                case "Home": return 0;
                case "Network": return 1;
                case "Newsfeed": return 2;
                default: return -1;
            }
        }

        private string ConvertIntToStartPage(int value)
        {
            switch (value)
            {
                case -1: return "";
                case 0: return "Home";
                case 1: return "Network";
                case 2: return "Newsfeed";
                default: return "";
            }
        }

        public int DefaultACLPosts
        {
            get { return ConvertDefaultACLPosts(); }
            set
            {
                SaveDefaultACLPosts(value);
                OnPropertyChanged("IsPrivateACLVisible");
            }
        }

        private int ConvertDefaultACLPosts()
        {
            var publicPost = Settings.ACLPublicPost;
            var privatePost = Settings.ACLPrivatePost;

            if (publicPost && !privatePost)
                return 0;
            if (!publicPost && privatePost)
                return 1;
            else
                return -1;
        }

        private void SaveDefaultACLPosts(int value)
        {
            var current = ConvertDefaultACLPosts();
            if (current == value)
                return;

            switch (value)
            {
                case 0:
                    Settings.ACLPublicPost = true;
                    Settings.ACLPrivatePost = false;
                    break;
                case 1:
                    Settings.ACLPublicPost = false;
                    Settings.ACLPrivatePost = true;
                    break;
                default:
                    Settings.ACLPublicPost = false;
                    Settings.ACLPrivatePost = false;
                    break;
            }
        }

        public bool IsPrivateACLVisible
        {
            get { return (Settings.ACLPrivatePost); }
        }

        private bool _isListViewsVisible;
        public bool IsListViewsVisible
        {
            get { return _isListViewsVisible; }
            set { SetProperty(ref _isListViewsVisible, value); }
        }

        private bool _isButtonReloadVisible;
        public bool IsButtonReloadVisible
        {
            get { return _isButtonReloadVisible; }
            set { SetProperty(ref _isButtonReloadVisible, value); }
        }

        private ObservableCollection<SelectableData<FriendicaGroup>> _groups;
        public ObservableCollection<SelectableData<FriendicaGroup>> Groups
        {
            get { return _groups; }
            set { SetProperty(ref _groups, value); }
        }

        private ObservableCollection<SelectableData<FriendicaUser>> _friends;
        public ObservableCollection<SelectableData<FriendicaUser>> Friends
        {
            get { return _friends; }
            set { SetProperty(ref _friends, value); }
        }

        List<int> SelectedContacts { get; set; }
        List<int> SelectedGroups { get; set; }

        public bool SaveLocalAllowed
        {
            get { return Settings.SaveLocalAllowed; }
            set
            {
                Settings.SaveLocalAllowed = value;
                if (!value)
                    ShowDeleteLocalDataDialog();
                OnPropertyChanged("SaveLocalAllowed");
            }
        }

        public bool SaveFullsizePhotosAllowed
        {
            get { return Settings.SaveFullsizePhotosAllowed; }
            set
            {
                Settings.SaveFullsizePhotosAllowed = value;
                if (!value)
                    ShowDeleteLocalDataDialogFullsize();
                OnPropertyChanged("SaveFullsizePhotosAllowed");
            }
        }

        private bool _isDeleteLocalDataDialogVisible;
        public bool IsDeleteLocalDataDialogVisible
        {
            get { return _isDeleteLocalDataDialogVisible; }
            set { SetProperty(ref _isDeleteLocalDataDialogVisible, value); }
        }

        private bool _deleteLocalDatabaseChecked;
        public bool DeleteLocalDatabaseChecked
        {
            get { return _deleteLocalDatabaseChecked; }
            set
            {
                SetProperty(ref _deleteLocalDatabaseChecked, value);
                (DeleteLocalDataCommand as Command).ChangeCanExecute();
            }
        }

        private bool _deleteFullsizePhotosChecked;
        public bool DeleteFullsizePhotosChecked
        {
            get { return _deleteFullsizePhotosChecked; }
            set
            {
                SetProperty(ref _deleteFullsizePhotosChecked, value);
                (DeleteLocalDataCommand as Command).ChangeCanExecute();
            }
        }

        private bool _deleteSmallMediumsizePhotosChecked;
        public bool DeleteSmallMediumsizePhotosChecked
        {
            get { return _deleteSmallMediumsizePhotosChecked; }
            set
            {
                SetProperty(ref _deleteSmallMediumsizePhotosChecked, value);
                (DeleteLocalDataCommand as Command).ChangeCanExecute();
            }
        }

        public bool SendCoordinatesAllowed
        {
            get { return Settings.SendCoordinatesAllowed; }
            set
            {
                Settings.SendCoordinatesAllowed = value;
                OnPropertyChanged("SendCoordinatesAllowed");
            }
        }

        public bool NotificationActivated
        {
            get { return Settings.NotificationActivated; }
            set
            {
                Settings.NotificationActivated = value;
                OnPropertyChanged("NotificationActivated");
            }
        }

        public bool NotificationEachNewsfeedAllowed
        {
            get { return Settings.NotificationEachNewsfeedAllowed; }
            set
            {
                Settings.NotificationEachNewsfeedAllowed = value;
                OnPropertyChanged("NotificationEachNewsfeedAllowed");
            }
        }

        public bool NotificationShowMessageContent
        {
            get { return Settings.NotificationShowMessageContent; }
            set
            {
                Settings.NotificationShowMessageContent = value;
                OnPropertyChanged("NotificationShowMessageContent");
            }
        }

        public int NotificationFreshnessTime
        {
            get { return Settings.NotificationFreshnessTime; }
            set
            {
                Settings.NotificationFreshnessTime = value;
                OnPropertyChanged("NotificationFreshnessTime");
                OnPropertyChanged("NotificationFreshnessTimeHint");
            }
        }

        public string NotificationFreshnessTimeHint
        {
            get { return String.Format(AppResources.textblockNotificationFreshnessTimeHint, NotificationFreshnessTime); }
        }

#region Commands

        private ICommand _testConnectionCommand;
        public ICommand TestConnectionCommand => _testConnectionCommand ?? (_testConnectionCommand = new Command(TestConnection, CanTestConnection));
        private bool CanTestConnection()
        {
            return _credentialsOk;
        }
        private async void TestConnection()
        {
            // remove eventually visible hints from last test
            TestConnectionResultVisible = false;
            // show activity indicator
            ActivityIndicatorText = AppResources.textblockConnectionTestInProgress_Text;
            ActivityIndicatorVisible = true;
            // start http request
            var http = new HttpFriendicaHelpers(FriendicaServer, FriendicaUsername, FriendicaPassword);
            var testResult = await http.GetAccountVerifyCredentialsAsync();
            ActivityIndicatorVisible = false;

            // reshow the result text, select text to display depending on the result
            TestConnectionResultVisible = true;
            switch (testResult)
            {
                case HttpFriendicaHelpers.TestConnectionResults.NotAnswered:
                    ServerActivityFailed = true;
                    LabelServerActivityFailed = AppResources.TestConnectionResultNotAnswered;
                    break;
                case HttpFriendicaHelpers.TestConnectionResults.NotAuthenticated:
                    ServerActivityFailed = true;
                    LabelServerActivityFailed = AppResources.TestConnectionResultNotAuthenticated;
                    break;
                case HttpFriendicaHelpers.TestConnectionResults.UnexpectedError:
                    ServerActivityFailed = true;
                    LabelServerActivityFailed = String.Format(AppResources.TestConnectionResultUnexpectedError, 
                                                              http.StatusCode, 
                                                              http.ErrorMessage, 
                                                              http.ErrorHResult);
                    break;
                case HttpFriendicaHelpers.TestConnectionResults.OK:
                    ServerActivityFailed = false;

                    // clear out all settings, list of contacts/groups if changed
                    if (Settings.FriendicaServer != FriendicaServer || Settings.FriendicaUsername != FriendicaUsername)
                        ResetSettingsUponChangedCredentials();

                    // save data to appsettings
                    Settings.FriendicaServer = FriendicaServer;
                    Settings.FriendicaUsername = FriendicaUsername;
                    Settings.FriendicaPassword = FriendicaPassword;
                    (ResetCredentialsCommand as Command).ChangeCanExecute();

                    SetNavigationAllowed(true);
                    break;
            }
            TestConnectionResultVisible = true;
        }

        private ICommand _resetCredentialsCommand;
        public ICommand ResetCredentialsCommand => _resetCredentialsCommand ?? (_resetCredentialsCommand = new Command(ResetCredentials, CanResetCredentials));
        private bool CanResetCredentials()
        {
            if (FriendicaServer == Settings.FriendicaServer
                && FriendicaUsername == Settings.FriendicaUsername
                && FriendicaPassword == Settings.FriendicaPassword)
            {
                return false;
            }
            else
            {
                if (FriendicaServer == "https://")
                    return false;
                else
                    return true;
            }
        }
        private async void ResetCredentials()
        {
            var answer = await Application.Current.MainPage.DisplayAlert(AppResources.ButtonSettingsResetCredentials, 
                                                                         AppResources.DialogSettingsConfirmReset, 
                                                                         AppResources.buttonYes, AppResources.buttonNo);
            if (answer)
                DefineDefaultCredentials();
        }

        private ICommand _resetSettingsCommand;
        public ICommand ResetSettingsCommand => _resetSettingsCommand ?? (_resetSettingsCommand = new Command(ResetSettings));
        private async void ResetSettings()
        {
            var answer = await Application.Current.MainPage.DisplayAlert(AppResources.ButtonSettingsResetComplete,
                                                                        AppResources.MessageDialogSettingsResetComplete,
                                                                         AppResources.buttonYes, AppResources.buttonNo);
            if (answer)
            {
                Settings.ClearAllSettings();
                DefineDefaultCredentials();
                ReloadContactData();
                ResetSettingsUponChangedCredentials();
            }

        }

        private ICommand _reloadContactsCommand;
        public ICommand ReloadContactsCommand => _reloadContactsCommand ?? (_reloadContactsCommand = new Command(ReloadContacts));
        void ReloadContacts()
        {
            ReloadContactData();
        }

        private ICommand _removeAdvertisingCommand;
        public ICommand RemoveAdvertisingCommand => _removeAdvertisingCommand ?? (_removeAdvertisingCommand = new Command(RemoveAdvertising, CanRemoveAdvertising));
        bool CanRemoveAdvertising()
        {
            // TODO: implement check if there is still advertising and if we can perform in-app-purchase (plattform dependent)
            return false;
        }

        void RemoveAdvertising()
        {
            // TODO: implement routine to remove advertising (= buy in-app-purchase => plattform dependent)
            Application.Current.MainPage.DisplayAlert("### Remove Advertising ###", "### not yet implemented ###", "OK");
        }

        private ICommand _showDeleteLocalDataDialogCommand;
        public ICommand ShowDeleteLocalDataDialogCommand => _showDeleteLocalDataDialogCommand ?? (_showDeleteLocalDataDialogCommand = new Command(ShowDeleteLocalDataDialog));
        void ShowDeleteLocalDataDialog()
        {
            DeleteLocalDatabaseChecked = true;
            DeleteFullsizePhotosChecked = true;
            DeleteSmallMediumsizePhotosChecked = true;

            IsDeleteLocalDataDialogVisible = true;
        }

        private ICommand _deleteLocalDataCommand;
        public ICommand DeleteLocalDataCommand => _deleteLocalDataCommand ?? (_deleteLocalDataCommand = new Command(DeleteLocalData, CanDeleteLocalData));
        bool CanDeleteLocalData()
        {
            if (!DeleteLocalDatabaseChecked && !DeleteSmallMediumsizePhotosChecked && !DeleteFullsizePhotosChecked)
                return false;
            else
                return true;
        }

        void DeleteLocalData()
        {
            // TODO: implement deletion of local data (later as not yet implemented database or photo storage)
            Application.Current.MainPage.DisplayAlert("### Delete local data ###", "### now will be deleting the selected data ###", "OK");
        }

        private ICommand _cancelDeleteLocalDataCommand;
        public ICommand CancelDeleteLocalDataCommand => _cancelDeleteLocalDataCommand ?? (_cancelDeleteLocalDataCommand = new Command(CancelDeleteLocalData));
        void CancelDeleteLocalData()
        {
            IsDeleteLocalDataDialogVisible = false;
        }


#endregion

        public SettingsViewModel()
        {
            Title = AppResources.PageAppSettings;
            // make setting for navigationside visible if we are using a phone device
            IsNavigationSideVisible = (App.DeviceType == Plugin.DeviceInfo.Abstractions.Idiom.Phone);
            IsDeleteLocalDataDialogVisible = false;

            DefineDefaultCredentials();

            // init HttpFriendicaContacts
            if (App.Contacts == null)
                App.Contacts = new HttpFriendicaContacts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            if (Settings.IsFriendicaLoginDefined())
                ReloadContactData();

            App.Contacts.HttpRequestFinished += (sender, e) => { CreateListViews(); };

            // reload listview elements if user changes theme (otherwise elements keep old color settings)
            Settings.AppThemeModeChanged += (sender, e) => {
                CreateListViews();
            };
        }


        private void DefineDefaultCredentials()
        {
            TestConnectionResultVisible = false;
            FriendicaServer = (Settings.FriendicaServer != "") ? Settings.FriendicaServer : "https://";
            FriendicaUsername = Settings.FriendicaUsername;
            FriendicaPassword = Settings.FriendicaPassword;
        }

        private void CheckCredentials()
        {   
            if (FriendicaServer == Settings.FriendicaServer
                && FriendicaUsername == Settings.FriendicaUsername
                && FriendicaPassword == Settings.FriendicaPassword)
            {
                SetNavigationAllowed(true);
                return;
            }
            else
            {
                if (FriendicaServer == "https://")
                    SetNavigationAllowed(true);
                else
                    SetNavigationAllowed(false);
            }

            _credentialsOk = true;
            FriendicaServerHint = FriendicaServerHints.Standard;
            FriendicaUsernameHint = FriendicaUsernameHints.Standard;
            FriendicaPasswordHint = FriendicaPasswordHints.Standard;

            // put warning if url is only using http:// as this is not very secure
            if (FriendicaServer.StartsWith("http://"))
            {
                FriendicaServerHint = FriendicaServerHints.NoHttps;
                // is only warning, no need to set _credentialsOk
            }

            // warn user if url is ending with a "/"
            if (FriendicaServer.EndsWith("/"))
            {
                FriendicaServerHint = FriendicaServerHints.EndingSlash;
                // is only warning, no need to set _credentialsOk
            }

            // does url start with https:// or at least http://
            if (!FriendicaServer.StartsWith("https://") && !FriendicaServer.StartsWith("http://"))
            {
                FriendicaServerHint = FriendicaServerHints.WrongStart;
                _credentialsOk &= false;
            }

            // is url a full url or only the predefined https://
            if (FriendicaServer == "http://" || FriendicaServer == "https://")
            {
                FriendicaServerHint = FriendicaServerHints.OnlyHttp;
                _credentialsOk &= false;
            }

            // check if server url is empty
            if (FriendicaServer == "")
            {
                FriendicaServerHint = FriendicaServerHints.Empty;
                _credentialsOk &= false;
            }

            // check if username is empty
            if (FriendicaUsername == "")
            {
                FriendicaUsernameHint = FriendicaUsernameHints.Empty;
                _credentialsOk &= false;
            }


            // check if password is empty
            if (FriendicaPassword == "")
            {
                FriendicaPasswordHint = FriendicaPasswordHints.Empty;
                _credentialsOk &= false;
            }

            (TestConnectionCommand as Command).ChangeCanExecute();
            (ResetCredentialsCommand as Command).ChangeCanExecute();  
        }

        private void ResetSettingsUponChangedCredentials()
        {
            // Settings to be reset upon changed server or username
            // TODO: implement the following step by step
                // clear out current App.Contacts to start with new credentials
                App.Contacts = new HttpFriendicaContacts(FriendicaServer, FriendicaUsername, FriendicaPassword);
                Settings.ACLPrivateSelectedContacts = String.Empty;
                Settings.ACLPrivateSelectedGroups = String.Empty;

                //Settings.LastReadNetworkPost = 0.0;
                //Settings.LastNotifiedNetworkPost = 0.0;
                //Settings.LastNotifiedMessage = 0.0;
                //App.TileCounter.CounterUnseenHome = 0;
                //StaticGlobalParameters.CounterUnseenHome = 0;
                //StaticGlobalParameters.CounterUnseenNetwork = 0;
                //StaticGlobalParameters.CounterUnseenNewsfeed = 0;
                //StaticGlobalParameters.CounterUnseenMessages = 0;
                App.Posts = null;
                App.Contacts = null;
                //// clear everything related to private messages
                //App.IsSendingNewMessage = false;
                //App.MessagesNavigatedIntoConversation = false;
                //App.MessagesVm = null;
                //App.TileCounter.CounterUnseenMessages = 0;
                //// clear everything related to photos
                //App.PhotosNavigatedIntoAlbum = false;
                //App.PhotosVm = null;
                //// delete locally stored data of the old user@server
                //await DeleteLocalDataAsync();

            // Settings to be reset upon every change of credentials
            // TODO: implement the following step by step 

            //// reload contacts from server
            ReloadContactData();

            //// reset home and network data
            //App.HomePosts = new ObservableCollection<FriendicaPostExtended>();
            //App.HomeThreads = new ObservableCollection<FriendicaThread>();
            //App.NetworkPosts = new ObservableCollection<FriendicaPostExtended>();
            //App.NetworkThreads = new ObservableCollection<FriendicaThread>();
        }


        private async void ReloadContactData()
        {
            // check if data is already loaded, if not let's load it
            if (App.Contacts == null)
                return;

            if (App.Contacts.Friends == null && App.Contacts.Groups == null)
            {
                ActivityIndicatorText = AppResources.TextSettingsLoadingContacts;
                ActivityIndicatorVisible = true;
                var result = await App.Contacts.GetStatusesFriends();
                var resultGroup = await App.Contacts.GetFriendicaGroupShow();
                ActivityIndicatorVisible = false;
                
                // now work with the results
                if (result == HttpFriendicaContacts.GetFriendicaFriendsResults.OK &&
                   resultGroup == HttpFriendicaContacts.GetFriendicaFriendsResults.OK)
                {
                    IsButtonReloadVisible = false;
                    // display an info text if there were no contacts returned by server
                    if (App.Contacts.Friends == null && App.Contacts.Groups == null)
                    {
                        IsListViewsVisible = false;
                        LabelServerActivityFailed = AppResources.textblockNoDataAvailableFriends_Text;
                        return;
                    }
                    IsListViewsVisible = true;
                    CreateListViews();
                }
                else
                {
                    // display error messages
                    ErrorMessages(result);
                    ErrorMessages(resultGroup);
                }
            }
        }

        private void ErrorMessages(HttpFriendicaContacts.GetFriendicaFriendsResults result)
        {
            switch (result)
            {
                case HttpFriendicaContacts.GetFriendicaFriendsResults.NotAnswered:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = true;
                    LabelServerActivityFailed = AppResources.messageDialogHttpStatusNone;
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.NotImplemented:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = false;
                    // show a hint if server version is too old to retrieve group data
                    LabelServerActivityFailed = AppResources.textblockNoGroupsAvailable_Text;
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.SerializationError:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = true;
                    // show error with requested URL and the returned JSON upon serialization error
                    LabelServerActivityFailed = String.Format(AppResources.TextSerializationError, 
                                                        App.Contacts._requestedUrl, App.Contacts.ReturnString);
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.UnexpectedError:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = true;
                    LabelServerActivityFailed = String.Format(AppResources.TestConnectionResultUnexpectedError,
                                                              App.Contacts.StatusCode, App.Contacts.ErrorMessage, App.Contacts.ErrorHResult);
                    break;
            }
        }

        private void CreateListViews()
        {
            SelectedContacts = GetSelectedContactsFromSettings();
            SelectedGroups = GetSelectedGroupsFromSettings();

            if (App.Contacts.Groups != null)
            {
                Groups = new ObservableCollection<SelectableData<FriendicaGroup>>();
                foreach (var group in App.Contacts.Groups)
                {
                    var selectedData = new SelectableData<FriendicaGroup>(group)
                    {
                        IsSelected = (SelectedGroups.Contains(group.GroupGid))
                    };
                    selectedData.PropertyChanged += (sender, e) => { SaveSelectedToSettings(); };
                    Groups.Add(selectedData);
                }
            }

            if (App.Contacts.Friends != null)
            {
                Friends = new ObservableCollection<SelectableData<FriendicaUser>>();
                foreach (var friend in App.Contacts.Friends)
                {
                    var selectedData = new SelectableData<FriendicaUser>(friend)
                    {
                        IsSelected = (SelectedContacts.Contains(friend.UserCid))
                    };
                    selectedData.PropertyChanged += (sender, e) => { SaveSelectedToSettings(); };
                    Friends.Add(selectedData);
                }
            }
        }

        void SaveSelectedToSettings()
        {
            var selectedContacts = Friends.Where(f => f.IsSelected).OrderBy(f => f.Data.UserCid);
            var selectedIds = "";
            foreach (var friend in selectedContacts)
            {
                selectedIds += "<" + friend.Data.UserCid + ">";
            }
            Settings.ACLPrivateSelectedContacts = selectedIds;

            var selectedGroups = Groups.Where(g => g.IsSelected).OrderBy(g => g.Data.GroupGid);
            selectedIds = "";
            foreach (var group in selectedGroups)
            {
                selectedIds += "<" + group.Data.GroupGid + ">";
            }
            Settings.ACLPrivateSelectedGroups = selectedIds;
        }

        List<int> GetSelectedContactsFromSettings()
        {
            var list = new List<int>();
            var cids = Regex.Split(Settings.ACLPrivateSelectedContacts, @"<");
            foreach (var cid in cids)
            {
                if (cid != "")
                {
                    var cidClean = cid.Replace("<", "");
                    cidClean = cid.Replace(">", "");
                    list.Add(Convert.ToInt32(cidClean));
                }
            }
            return list;                      
        }

        List<int> GetSelectedGroupsFromSettings()
        {
            var list = new List<int>();
            var gids = Regex.Split(Settings.ACLPrivateSelectedGroups, @"<");
            foreach (var gid in gids)
            {
                if (gid != "")
                {
                    var gidClean = gid.Replace("<", "");
                    gidClean = gid.Replace(">", "");
                    list.Add(Convert.ToInt32(gidClean));
                }
            }
            return list;
        }

        void ShowDeleteLocalDataDialogFullsize()
        {
            DeleteLocalDatabaseChecked = false;
            DeleteFullsizePhotosChecked = true;
            DeleteSmallMediumsizePhotosChecked = false;

            IsDeleteLocalDataDialogVisible = true;
        }
    }
}
