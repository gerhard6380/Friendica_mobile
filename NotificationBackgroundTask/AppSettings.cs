using System;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BackgroundTasks
{
    public enum OrientationDeviceFamily { MobileLandscape, MobilePortrait, DesktopLandscape, DesktopPortrait };

    public sealed class AppSettings
    {
        public AppSettings()
        {
            // set to right on first start as most of the users are right-handed, only relevant for mobile devices, desktop always left-side
            var localSettings = ApplicationData.Current.LocalSettings;
            
            this.LastReadNetworkPost = Convert.ToDouble(localSettings.Values["LastReadNetworkPost"]);
            this.LastNotifiedNetworkPost = Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["LastNotifiedNetworkPost"]);
            this.LastNotifiedMessage = Convert.ToInt32(localSettings.Values["LastNotifiedMessage"]);

            this._friendicaServer = (string)localSettings.Values["FriendicaServer"];
            this._friendicaUsername = (string)localSettings.Values["FriendicaUsername"];
            this._friendicaPassword = (string)localSettings.Values["FriendicaPassword"];
            try { this._notificationShowMessageContent = (bool)localSettings.Values["NotificationShowMessageContent"]; } catch { NotificationShowMessageContent = true; }
        }

        #region backgroundSettings
        // Store id of last post read by user on this device
        private double _lastReadNetworkPost;
        public double LastReadNetworkPost
        {
            get { return _lastReadNetworkPost; }
            set { ApplicationData.Current.LocalSettings.Values["LastReadNetworkPost"] = value.ToString();
                _lastReadNetworkPost = value;
                if (value > LastNotifiedNetworkPost)  // get of LastNotifiedNetworkPost always sends current value from LocalSettings for this comparison
                    LastNotifiedNetworkPost = value;   // synchronize LastNotified with LastRead if LastRead is changed
            }
        }

        // Store id of last notified post (toast notification) sent to action center on this device
        private double _lastNotifiedNetworkPost;
        public double LastNotifiedNetworkPost
        {
            // use for get always newest value from LocalSettings
            get { return Convert.ToDouble(ApplicationData.Current.LocalSettings.Values["LastNotifiedNetworkPost"]); }
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
            }
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


        #endregion
    }
}
