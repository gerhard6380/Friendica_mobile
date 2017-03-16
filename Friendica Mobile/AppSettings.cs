using System;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile
{
    public enum OrientationDeviceFamily { MobileLandscape, MobilePortrait, MobileContinuum, DesktopLandscape, DesktopPortrait };

    public class AppSettings : BindableClass
    {
        public AppSettings()
        {
            // set to right on first start as most of the users are right-handed, only relevant for mobile devices, desktop always left-side
            var localSettings = ApplicationData.Current.LocalSettings;
            this.LastReadNetworkPost = Convert.ToDouble(localSettings.Values["LastReadNetworkPost"]);
            this.LastNotifiedNetworkPost = Convert.ToDouble(localSettings.Values["LastNotifiedNetworkPost"]);
            this.LastNotifiedMessage = Convert.ToInt32(localSettings.Values["LastNotifiedMessage"]);

            this._navigationSide = (string)localSettings.Values["NavigationSide"];
            if (this.NavigationSide == null)
                this.NavigationSide = "RightToLeft";
            this._startPage = (string)localSettings.Values["StartPage"];
            if (this.StartPage == null)
                this.StartPage = "Network";
            // initialize NetworkMode with "Chronological"
            this._networkMode = (string)localSettings.Values["NetworkMode"];
            if (this.NetworkMode == null)
                this.NetworkMode = "Chronological";

            this._friendicaServer = (string)localSettings.Values["FriendicaServer"];
            this._friendicaUsername = (string)localSettings.Values["FriendicaUsername"];
            this._friendicaPassword = (string)localSettings.Values["FriendicaPassword"];
            try { this._aclPublicPost = (bool)localSettings.Values["ACLPublicPost"]; } catch { this._aclPublicPost = false; }
            try { this._aclPrivatePost = (bool)localSettings.Values["ACLPrivatePost"]; } catch { this._aclPrivatePost = false; }
            this._aclPrivateSelectedContacts = (string)localSettings.Values["ACLPrivateSelectedContacts"];
            this._aclPrivateSelectedGroups = (string)localSettings.Values["ACLPrivateSelectedGroups"];
            this._saveLocalAllowed = (string)localSettings.Values["SaveLocalAllowed"];
            try { this._saveFullsizePhotosAllowed = (bool)localSettings.Values["SaveFullsizePhotosAllowed"]; } catch { this._saveFullsizePhotosAllowed = true; }
            this._sendCoordinatesAllowed = (string)localSettings.Values["SendCoordinatesAllowed"];
            if (SaveLocalAllowed == null)
                SaveLocalAllowed = "true";
            if (SendCoordinatesAllowed == null)
                SendCoordinatesAllowed = "true";
            try { this._notificationActivated = (bool)localSettings.Values["NotificationActivated"]; } catch { NotificationActivated = true; }
            try { this._notificationShowMessageContent = (bool)localSettings.Values["NotificationShowMessageContent"]; } catch { NotificationShowMessageContent = true; }
            try { this._notificationFreshnessTime = (int)localSettings.Values["NotificationFreshnessTime"]; } catch { NotificationFreshnessTime = 15; }
        }

        #region backgroundSettings
        // store width of window, will be used for the calculation of the remaining windows after deducting place of advertising
        // value can change during session if user change screen size (Desktop: resizing, Mobile: turn from/to Landscape); not included in Settings page
        private double _shellWidth;
        public double ShellWidth
        {
            get { return _shellWidth; }
            set
            {
                _shellWidth = value;
                OnPropertyChanged("ShellWidth");
            }
        }

        private double _shellHeight;
        public double ShellHeight
        {
            get { return _shellHeight; }
            set { _shellHeight = value;
                OnPropertyChanged("ShellHeight"); }
        }


        // store current orientation of the device; value can change during session if user turns to/from Landscape on Mobile; 
        // not included in Settings page
        private ApplicationViewOrientation _orientation;
        public ApplicationViewOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                _orientation = value;
                BottomAppBarMargin = SetBottomAppBarMargin();
                OnPropertyChanged("Orientation");
            }
        }


        // store current DeviceFamily (not changing during session); not included in Settings page
        private string _deviceFamily;
        public string DeviceFamily
        {
            get { return _deviceFamily; }
            set
            {
                _deviceFamily = value;
                OnPropertyChanged("DeviceFamily");
            }
        }


        // combined information from Orientation and DeviceFamily; used for determining the correct VisualState
        // not included in Settings page
        private OrientationDeviceFamily _orientationDevice;
        public OrientationDeviceFamily OrientationDevice
        {
            get { return _orientationDevice; }
            set
            {
                _orientationDevice = value;
                // trigger new setting of margin
                BottomAppBarMargin = SetBottomAppBarMargin();
                OnPropertyChanged("OrientationDevice");
            }
        }

        private Thickness SetBottomAppBarMargin()
        {
            if (Orientation == ApplicationViewOrientation.Landscape)
            {
                return new Thickness(48, 0, 0, 0);
            }
            else
            {
                //if (_isTrial)
                if (!PaidForRemovingAds)
                {
                    if (NavigationSide == "LeftToRight")
                        return new Thickness(48, 0, 0, 80);
                    else
                        return new Thickness(0, 0, 48, 80);
                }
                else
                {
                    if (NavigationSide == "LeftToRight")
                        return new Thickness(48, 0, 0, 0);
                    else
                        return new Thickness(0, 0, 48, 0);
                }
            }
        }

        public void SetBottomAppBarMarginInputPane(bool paneIsShowing)
        {
            if (Orientation == ApplicationViewOrientation.Landscape)
            {
                BottomAppBarMargin = new Thickness(48, 0, 0, 0);
            }
            else
            {
                //if (_isTrial)
                if (!PaidForRemovingAds)
                {
                    var bottom = (paneIsShowing) ? 0 : 80;
                    if (NavigationSide == "LeftToRight")
                        BottomAppBarMargin = new Thickness(48, 0, 0, bottom);
                    else
                        BottomAppBarMargin = new Thickness(0, 0, 48, bottom);
                }
                else
                {
                    if (NavigationSide == "LeftToRight")
                        BottomAppBarMargin = new Thickness(48, 0, 0, 0);
                    else
                        BottomAppBarMargin = new Thickness(0, 0, 48, 0);
                }
            }
        }

        // used from ContinuumTrigger, should store the current setup mode (mouse or gesture input) on continuum devices; 
        // currently not used; not included in Settings page
        private string _uiMode;
        public string UIMode
        {
            get { return _uiMode; }
            set
            {
                _uiMode = value;
                OnPropertyChanged("UIMode");
            }
        }


        // save the state of the app's license in Appsettings - either Trial or not
        //private bool _isTrial;
        //public bool IsTrial
        //{
        //    get { return _isTrial;  }
        //    set
        //    {
        //        _isTrial = value;
        //        // trigger new Margin setting
        //        BottomAppBarMargin = SetBottomAppBarMargin();
        //        OnPropertyChanged("IsTrial");
        //    }
        //}

        // save if user has bought the in-app "RemoveAdvertising"
        private bool _paidForRemovingAds;
        public bool PaidForRemovingAds
        {
            get { return _paidForRemovingAds; }
            set { _paidForRemovingAds = value;
                // trigger new Margin setting
                BottomAppBarMargin = SetBottomAppBarMargin();
                OnPropertyChanged("PaidForRemovingAds");
            }
        }

        // set Margin for BottomAppBar in MobileDevices
        private Thickness _bottomAppBarMargin;
        public Thickness BottomAppBarMargin
        {
            get { return _bottomAppBarMargin; }
            set { _bottomAppBarMargin = value;
                OnPropertyChanged("BottomAppBarMargin"); }
        }

        // Store id of last post read by user on this device
        private double _lastReadNetworkPost;
        public double LastReadNetworkPost
        {
            get { return _lastReadNetworkPost; }
            set { ApplicationData.Current.LocalSettings.Values["LastReadNetworkPost"] = value.ToString();
                _lastReadNetworkPost = value;
                LastNotifiedNetworkPost = value;   // synchronize LastNotified with LastRead if LastRead is changed
                OnPropertyChanged("LastReadNetworkPost"); }
        }

        // Store id of last notified post (toast notification) sent to action center on this device
        private double _lastNotifiedNetworkPost;
        public double LastNotifiedNetworkPost
        {
            get { return _lastNotifiedNetworkPost; }
            set { ApplicationData.Current.LocalSettings.Values["LastNotifiedNetworkPost"] = value.ToString();
                _lastNotifiedNetworkPost = value; }
        }

        // Store id of last notified message (toast notification) sent to action center on this device
        private double _lastNotifiedMessage;
        public double LastNotifiedMessage
        {
            get { return _lastNotifiedMessage; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["LastNotifiedMessage"] = value.ToString();
                _lastNotifiedMessage = value;
            }
        }


        #endregion

        #region UserSettings
        // user can decide if navigation buttons shall be displayed right or left on mobile devices
        // this to support left handed users, having the phone in der left hand; on desktop the navigation is always on left side
        private string _navigationSide;
        public string NavigationSide
        {
            get
            {
                return _navigationSide;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["NavigationSide"] = value;
                _navigationSide = value;
                // trigger new Margin setting
                BottomAppBarMargin = SetBottomAppBarMargin();
                OnPropertyChanged("NavigationSide");
            }
        }


        // user can decide to which page the app navigates on start, the same page will set the navigation stack back
        // in order to leave the app with exit on back button (phone only)
        private string _startPage;
        public string StartPage
        {
            get { return _startPage; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["StartPage"] = value;
                _startPage = value;
                OnPropertyChanged("StartPage");
            }
        }


        // user can decide which mode on network page should be displayed (chronological order or sorted by threads)
        private string _networkMode;
        public string NetworkMode
        {
            get { return _networkMode; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["NetworkMode"] = value;
                _networkMode = value;
                OnPropertyChanged("NetworkMode");
            }
        }

        // user defines the url of the server, is only stored in AppSettings after checks are performed and connection test succeeded
        private string _friendicaServer;
        public string FriendicaServer
        {
            get
            {
                return _friendicaServer;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["FriendicaServer"] = value;
                _friendicaServer = value;
                OnPropertyChanged("FriendicaServer");
            }
        }


        // user defines his username or mailaddress for the server, is only stored in AppSettings after checks are performed and connection test succeeded
        private string _friendicaUsername;
        public string FriendicaUsername
        {
            get
            {
                return _friendicaUsername;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["FriendicaUsername"] = value;
                _friendicaUsername = value;
                OnPropertyChanged("FriendicaUsername");
            }
        }


        // user defines his password for the server, is only stored in AppSettings after checks are performed and connection test succeeded
        private string _friendicaPassword;
        public string FriendicaPassword
        {
            get
            {
                return _friendicaPassword;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["FriendicaPassword"] = value;
                _friendicaPassword = value;
                OnPropertyChanged("FriendicaPassword");
            }
        }


        // user can set default ACL rights for new postings - set new posts as public or not
        private bool _aclPublicPost;
        public bool ACLPublicPost
        {
            get { return _aclPublicPost; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["ACLPublicPost"] = value;
                _aclPublicPost = value;
                OnPropertyChanged("ACLPublicPost");
            }
        }


        // user can set default ACL rights for new postings - set new posts as private or not
        private bool _aclPrivatePost;
        public bool ACLPrivatePost
        {
            get { return _aclPrivatePost; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["ACLPrivatePost"] = value;
                _aclPrivatePost = value;
                OnPropertyChanged("ACLPrivatePost");
            }
        }


        // user can set default ACL rights for new postings - select contacts for private posts
        private string _aclPrivateSelectedContacts;
        public string ACLPrivateSelectedContacts
        {
            get { return _aclPrivateSelectedContacts; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["ACLPrivateSelectedContacts"] = value;
                _aclPrivateSelectedContacts = value;
                OnPropertyChanged("ACLPrivateSelectedContacts");
            }
        }


        // user can set default ACL rights for new postings - select contacts for private posts
        private string _aclPrivateSelectedGroups;
        public string ACLPrivateSelectedGroups
        {
            get { return _aclPrivateSelectedGroups; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["ACLPrivateSelectedGroups"] = value;
                _aclPrivateSelectedGroups = value;
                OnPropertyChanged("ACLPrivateSelectedGroups");
            }
        }


        // user defines whether app may save Friendica content (Users, Posts) in the local database or on local device
        private string _saveLocalAllowed;
        public string SaveLocalAllowed
        {
            get
            {
                return _saveLocalAllowed;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["SaveLocalAllowed"] = value;
                _saveLocalAllowed = value;
                OnPropertyChanged("SaveLocalAllowed");
            }
        }

        private bool _saveFullsizePhotosAllowed;
        public bool SaveFullsizePhotosAllowed
        {
            get { return _saveFullsizePhotosAllowed; }
            set {
                ApplicationData.Current.LocalSettings.Values["SaveFullsizePhotosAllowed"] = value;
                _saveFullsizePhotosAllowed = value;
                OnPropertyChanged("SaveFullsizePhotosAllowed"); }
        }


        // user defines whether GPS coordinates are by default sent on each post (can be overwritten on posting)
        private string _sendCoordinatesAllowed;
        public string SendCoordinatesAllowed
        {
            get { return _sendCoordinatesAllowed; }
            set { ApplicationData.Current.LocalSettings.Values["SendCoordinatesAllowed"] = value;
                _sendCoordinatesAllowed = value;
                OnPropertyChanged("SendCoordinatesAllowed"); }
        }


        // user can enable if the app should send notifications to the action center on new postings (either in Settings or in menu - quicklink)
        // this is not setting the allowance in Windows Settings (user need to allow the app there as well)
        private bool _notificationActivated;
        public bool NotificationActivated
        {
            get { return _notificationActivated; }
            set { ApplicationData.Current.LocalSettings.Values["NotificationActivated"] = value;
                _notificationActivated = value;
                if (App.BackgroundTasks != null)
                {
                    if (value)
                        App.BackgroundTasks.ActivateNotifications();
                    else
                        App.BackgroundTasks.DeactivateNotifications();
                }
                OnPropertyChanged("NotificationActivated"); }
        }

        // user can enable if the app should add content of private message to notifications to the action 
        // center or not (to avoid strangers to read PM on lock screen)
        private bool _notificationShowMessageContent;
        public bool NotificationShowMessageContent
        {
            get { return _notificationShowMessageContent; }
            set
            {
                ApplicationData.Current.LocalSettings.Values["NotificationShowMessageContent"] = value;
                _notificationShowMessageContent = value;
            }
        }

        // user can determine in which period the app shall recheck for new postings (minimum value is 15 min by Windows default)
        private int _notificationFreshnessTime;
        public int NotificationFreshnessTime
        {
            get { return _notificationFreshnessTime; }
            set { ApplicationData.Current.LocalSettings.Values["NotificationFreshnessTime"] = value;
                _notificationFreshnessTime = value; }
        }

        #endregion
    }
}
