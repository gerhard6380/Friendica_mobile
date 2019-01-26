using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Collections.Generic;

namespace Friendica_Mobile.PCL
{
    public enum AppState { Running, NotRunning };

    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        public static event EventHandler AppThemeModeChanged;
		public static event EventHandler NavigationSideChanged;

        // used to set back all settings to null when user wants to do so with a reset button
        public static void ClearAllSettings()
        {
            AppThemeUseSystemTheme = AppThemeUseSystemThemeDefault;
            AppThemeDarkModeEnabled = AppThemeDarkModeEnabledDefault;
			NavigationOnRightSide = NavigationOnRightSideDefault;
            // TODO: insert all other settings
        }

        #region Setting Constants

        // user defined settings
        private const string AppThemeUseSystemThemeKey = "AppThemeUseSystemTheme";
        private static readonly bool AppThemeUseSystemThemeDefault = false;

        private const string AppThemeDarkModeEnabledKey = "AppThemeDarkModeEnabled";
        private static readonly bool AppThemeDarkModeEnabledDefault = false;

		private const string NavigationOnRightSideKey = "NavigationOnRightSide";
		private static readonly bool NavigationOnRightSideDefault = true;

        private const string FriendicaServerKey = "FriendicaServer";
        private static readonly string FriendicaServerDefault = string.Empty;

        private const string FriendicaUsernameKey = "FriendicaUsername";
        private static readonly string FriendicaUsernameDefault = string.Empty;

        private const string FriendicaPasswordKey = "FriendicaPassword";
        private static readonly string FriendicaPasswordDefault = string.Empty;

        private const string NavigationSideKey = "NavigationSide";
        private static readonly string NavigationSideDefault = "RightToLeft";

        private const string StartPageKey = "StartPage";
        private static readonly string StartPageDefault = "Network";

        private const string ACLPublicPostKey = "ACLPublicPost";
        private static readonly bool ACLPublicPostDefault = false;

        private const string ACLPrivatePostKey = "ACLPrivatePost";
        private static readonly bool ACLPrivatePostDefault = false;

        private const string ACLPrivateSelectedContactsKey = "ACLPrivateSelectedContacts";
        private static readonly string ACLPrivateSelectedContactsDefault = string.Empty;

        private const string ACLPrivateSelectedGroupsKey = "ACLPrivateSelectedGroups";
        private static readonly string ACLPrivateSelectedGroupsDefault = string.Empty;

        private const string SaveLocalAllowedKey = "SaveLocalAllowed";
        private static readonly string SaveLocalAllowedDefault = "true";

        private const string SendCoordinatesAllowedKey = "SendCoordinatesAllowed";
        private static readonly string SendCoordinatesAllowedDefault = "true";

        private const string SaveFullsizePhotosAllowedKey = "SaveFullsizePhotosAllowed";
        private static readonly bool SaveFullsizePhotosAllowedDefault = false;

        private const string NotificationActivatedKey = "NotificationActivated";
        private static readonly bool NotificationActivatedDefault = true;

        private const string NotificationShowMessageContentKey = "NotificationShowMessageContent";
        private static readonly bool NotificationShowMessageContentDefault = true;

        private const string NotificationFreshnessTimeKey = "NotificationFreshnessTime";
        private static readonly int NotificationFreshnessTimeDefault = 15;

        private const string NotificationEachNewsfeedAllowedKey = "NotificationEachNewsfeedAllowed";
        private static readonly bool NotificationEachNewsfeedAllowedDefault = true;

        // app defined settings
        private const string CurrentAppStateKey = "CurrentAppState";
        private static readonly string CurrentAppStateDefault = "NotRunning";

        private const string LastReadNetworkPostKey = "LastReadNetworkPost";
        private static readonly string LastReadNetworkPostDefault = string.Empty;

        private const string LastNotifiedNetworkPostKey = "LastNotifiedNetworkPost";
        private static readonly string LastNotifiedNetworkPostDefault = string.Empty;

        private const string LastNotifiedMessageKey = "LastNotifiedMessage";
        private static readonly string LastNotifiedMessageDefault = string.Empty;

        private const string UnseenNewsfeedItemsKey = "UnseenNewsfeedItems";
        private static readonly string UnseenNewsfeedItemsDefault = string.Empty;

    #endregion

        public static bool AppThemeUseSystemTheme
        {
            get { return AppSettings.GetValueOrDefault(AppThemeUseSystemThemeKey, AppThemeUseSystemThemeDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(AppThemeUseSystemThemeKey, value);
                AppThemeModeChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool AppThemeDarkModeEnabled
        {
            get { return AppSettings.GetValueOrDefault(AppThemeDarkModeEnabledKey, AppThemeDarkModeEnabledDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(AppThemeDarkModeEnabledKey, value);
                AppThemeModeChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool NavigationOnRightSide
		{
			get { return AppSettings.GetValueOrDefault(NavigationOnRightSideKey, NavigationOnRightSideDefault); }
			set 
			{
				AppSettings.AddOrUpdateValue(NavigationOnRightSideKey, value);
				NavigationSideChanged?.Invoke(null, EventArgs.Empty);
			}
		}


        // url of Friendica server to which user wants to connect
        public static string FriendicaServer
        {
            get { return AppSettings.GetValueOrDefault(FriendicaServerKey, FriendicaServerDefault); }
            set { AppSettings.AddOrUpdateValue(FriendicaServerKey, value); }
        }

        
        // username to use for basic authentication login on specified server
        public static string FriendicaUsername
        {
            get { return AppSettings.GetValueOrDefault(FriendicaUsernameKey, FriendicaUsernameDefault); }
            set { AppSettings.AddOrUpdateValue(FriendicaUsernameKey, value); }
        }

        
        // password to use for basic authentication login on specified server
        public static string FriendicaPassword
        {
            get { return AppSettings.GetValueOrDefault(FriendicaPasswordKey, FriendicaPasswordDefault); }
            set { AppSettings.AddOrUpdateValue(FriendicaPasswordKey, value); }
        }


        // user can decide if navigation buttons shall be displayed right or left on mobile devices
        // this to support left handed users, having the phone in der left hand; on desktop the navigation is always on left side
        public static string NavigationSide
        {
            get { return AppSettings.GetValueOrDefault(NavigationSideKey, NavigationSideDefault); }
            set { AppSettings.AddOrUpdateValue(NavigationSideKey, value); }
        }

        
        // user can decide to which page the app navigates on start, the same page will set the navigation stack back
        // in order to leave the app with exit on back button (UWP Mobile only)
        public static string StartPage
        {
            get { return AppSettings.GetValueOrDefault(StartPageKey, StartPageDefault); }
            set { AppSettings.AddOrUpdateValue(StartPageKey, value); }
        }

        
        // user can set default ACL rights for new postings - set new posts as public or not
        public static bool ACLPublicPost
        {
            get { return AppSettings.GetValueOrDefault(ACLPublicPostKey, ACLPublicPostDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPublicPostKey, value); }
        }


        // user can set default ACL rights for new postings - set new posts as private or not
        public static bool ACLPrivatePost
        {
            get { return AppSettings.GetValueOrDefault(ACLPrivatePostKey, ACLPrivatePostDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPrivatePostKey, value); }
        }


        // user can set default ACL rights for new postings - select contacts for private posts
        public static string ACLPrivateSelectedContacts
        {
            get { return AppSettings.GetValueOrDefault(ACLPrivateSelectedContactsKey, ACLPrivateSelectedContactsDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPrivateSelectedContactsKey, value); }
        }


        // user can set default ACL rights for new postings - select contacts for private posts
        public static string ACLPrivateSelectedGroups
        {
            get { return AppSettings.GetValueOrDefault(ACLPrivateSelectedGroupsKey, ACLPrivateSelectedGroupsDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPrivateSelectedGroupsKey, value); }
        }


        // user defines whether app may save Friendica content (Users, Posts) in the local database or on local device
        // before switching to PCL settings class this was made as string type, we keep type but return now boolean
        public static bool SaveLocalAllowed
        {
            get { var setting = AppSettings.GetValueOrDefault(SaveLocalAllowedKey, SaveLocalAllowedDefault);
                if (setting == "true")
                    return true;
                else if (setting == "false")
                    return false;
                else
                    return true;
            }
            set { AppSettings.AddOrUpdateValue(SaveLocalAllowedKey, value.ToString()); }
        }


        // user defines whether GPS coordinates are by default sent on each post (can be overwritten on posting)
        // before switching to PCL settings class this was made as string type, we keep type but return now boolean
        public static bool SendCoordinatesAllowed
        {
            get
            {
                var setting = AppSettings.GetValueOrDefault(SendCoordinatesAllowedKey, SendCoordinatesAllowedDefault);
                if (setting == "true")
                    return true;
                else if (setting == "false")
                    return false;
                else
                    return true;
            }
            set { AppSettings.AddOrUpdateValue(SendCoordinatesAllowedKey, value.ToString()); }
        }


        // user can define if fullsize photos should be save on the device (disable by default to save disk space on device)
        public static bool SaveFullsizePhotosAllowed
        {
            get { return AppSettings.GetValueOrDefault(SaveFullsizePhotosAllowedKey, SaveFullsizePhotosAllowedDefault); }
            set { AppSettings.AddOrUpdateValue(SaveFullsizePhotosAllowedKey, value); }
        }


        // user can enable if the app should send notifications to the action center on new postings (either in Settings or in menu - quicklink)
        // this is not setting the allowance in Windows Settings (user need to allow the app there as well)
        public static bool NotificationActivated
        {
            get { return AppSettings.GetValueOrDefault(NotificationActivatedKey, NotificationActivatedDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationActivatedKey, value); }
        }


        // user can enable if the app should add content of private message to notifications to the action 
        // center or not (to avoid strangers to read PM on lock screen)
        public static bool NotificationShowMessageContent
        {
            get { return AppSettings.GetValueOrDefault(NotificationShowMessageContentKey, NotificationShowMessageContentDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationShowMessageContentKey, value); }
        }


        // user can determine in which period the app shall recheck for new postings (minimum value is 15 min by Windows default)
        public static int NotificationFreshnessTime
        {
            get { return AppSettings.GetValueOrDefault(NotificationFreshnessTimeKey, NotificationFreshnessTimeDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationFreshnessTimeKey, value); }
        }

        // user can tell the app if he wants to get notifications for each newsfeed item or only on summary notification
        public static bool NotificationEachNewsfeedAllowed
        {
            get { return AppSettings.GetValueOrDefault(NotificationEachNewsfeedAllowedKey, NotificationEachNewsfeedAllowedDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationEachNewsfeedAllowedKey, value); }
        }


        // app state to determine when app is in foreground
        public static AppState CurrentAppState
        {
            get { var state = AppSettings.GetValueOrDefault(CurrentAppStateKey, CurrentAppStateDefault);
                if (state == "Running")
                    return AppState.Running;
                else if (state == "NotRunning")
                    return AppState.NotRunning;
                else
                    return AppState.NotRunning;
            }
            set { AppSettings.AddOrUpdateValue(CurrentAppStateKey, value.ToString()); }
        }

        // Store id of last post read by user on this device
        // keep storing as string for backward compatability
        public static double LastReadNetworkPost
        {
            get { var setting = AppSettings.GetValueOrDefault(LastReadNetworkPostKey, LastReadNetworkPostDefault);
                try { return Convert.ToDouble(setting); }
                catch { return 0.0; }
            }
            set { AppSettings.AddOrUpdateValue(LastReadNetworkPostKey, value.ToString());
                LastNotifiedNetworkPost = value;  // synchronize LastNotified with LastRead if LastRead is changed
            }
        }


        // Store id of last notified post (toast notification) sent to action center on this device
        // keep storing as string for backward compatability
        public static double LastNotifiedNetworkPost
        {
            get
            {
                var setting = AppSettings.GetValueOrDefault(LastNotifiedNetworkPostKey, LastReadNetworkPostDefault);
                try { return Convert.ToDouble(setting); }
                catch { return 0.0; }
            }
            set
            {
                AppSettings.AddOrUpdateValue(LastNotifiedNetworkPostKey, value.ToString());
            }
        }


        // Store id of last notified message (toast notification) sent to action center on this device
        public static double LastNotifiedMessage
        {
            get
            {
                var setting = AppSettings.GetValueOrDefault(LastNotifiedMessageKey, LastNotifiedMessageDefault);
                try { return Convert.ToDouble(setting); }
                catch { return 0.0; }
            }
            set
            {
                AppSettings.AddOrUpdateValue(LastNotifiedMessageKey, value.ToString());
            }
        }


        // store the id's of the unseen newsfeed posts for marking them as unseen until user manually set the seen flag
        public static List<int> UnseenNewsfeedItems
        {
            get
            {
                var setting = AppSettings.GetValueOrDefault(UnseenNewsfeedItemsKey, UnseenNewsfeedItemsDefault);
                if (setting == null)
                    return new List<int>();
                else
                    return JsonConvert.DeserializeObject<List<int>>(setting);
            }
            set
            {
                try
                {
                    var serializedList = JsonConvert.SerializeObject(value);
                    AppSettings.AddOrUpdateValue(UnseenNewsfeedItemsKey, serializedList);
                }
                catch
                {
                    AppSettings.AddOrUpdateValue(UnseenNewsfeedItemsKey, "");
                }
            }
        }

    }
}
