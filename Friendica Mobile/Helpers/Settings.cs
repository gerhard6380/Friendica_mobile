using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;


namespace Friendica_Mobile
{
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

        // used to set back all settings to null when user is clicking "Unregister Application in LoginPage.xaml" button
        public static void ClearAllSettings()
        {
            FriendicaServer = FriendicaServerDefault;
            FriendicaUsername = FriendicaUsernameDefault;
            FriendicaPassword = FriendicaPasswordDefault;
            AppThemeUseSystemTheme = AppThemeUseSystemThemeDefault;
            AppThemeDarkModeEnabled = AppThemeDarkModeEnabledDefault;
            UseDefaultAccentColor = UseDefaultAccentColorDefault;
            SelectedAccentColor = SelectedAccentColorDefault;
            NavigationOnRightSide = NavigationOnRightSideDefault;
            StartPage = StartPageDefault;
            ACLPublicPost = ACLPublicPostDefault;
            ACLPrivatePost = ACLPrivatePostDefault;
            ACLPrivateSelectedContacts = ACLPrivateSelectedContactsDefault;
            ACLPrivateSelectedGroups = ACLPrivateSelectedGroupsDefault;
            SaveLocalAllowed = SaveLocalAllowedDefault;
            SaveFullsizePhotosAllowed = SaveFullsizePhotosAllowedDefault;
            SendCoordinatesAllowed = SendCoordinatesAllowedDefault;
            NotificationActivated = NotificationActivatedDefault;
            NotificationEachNewsfeedAllowed = NotificationEachNewsfeedAllowedDefault;
            NotificationShowMessageContent = NotificationShowMessageContentDefault;
            NotificationFreshnessTime = NotificationFreshnessTimeDefault;
            LastReadNetworkPost = LastReadNetworkPostDefault;
            UnseenNewsfeedItems = new List<int>();
        }

        /// <summary>
        /// Is the friendica login defined. Can be used to switch between sample demo mode and normal mode.
        /// </summary>
        /// <returns><c>true</c>, if friendica login credentials is defined, <c>false</c> otherwise.</returns>
        public static bool IsFriendicaLoginDefined()
        {
            if (FriendicaServer == "" || FriendicaServer == "http://" || FriendicaServer == "https://" || FriendicaServer == null)
                return false;
            if (FriendicaUsername == "")
                return false;
            if (FriendicaPassword == "")
                return false;
            return true;
        }

        #region Setting Constants

        private const string FriendicaServerKey = "FriendicaServer";
        private static readonly string FriendicaServerDefault = String.Empty;
        private const string FriendicaUsernameKey = "FriendicaUsername";
        private static readonly string FriendicaUsernameDefault = String.Empty;
        private const string FriendicaPasswordKey = "FriendicaPassword";
        private static readonly string FriendicaPasswordDefault = String.Empty;
        private const string AppThemeUseSystemThemeKey = "AppThemeUseSystemTheme";
		private static readonly bool AppThemeUseSystemThemeDefault = false;
        private const string AppThemeDarkModeEnabledKey = "AppThemeDarkModeEnabled";
        private static readonly bool AppThemeDarkModeEnabledDefault = false;
        private const string UseDefaultAccentColorKey = "UseDefaultAccentColor";
        private static readonly bool UseDefaultAccentColorDefault = true;
        private const string SelectedAccentColorKey = "SelectedAccentColor";
        private static readonly int SelectedAccentColorDefault;
        private const string NavigationOnRightSideKey = "NavigationOnRightSide";
        private static readonly bool NavigationOnRightSideDefault = true;
        private const string StartPageKey = "StartPage";
        private static readonly string StartPageDefault = "-1";
        private const string ACLPublicPostKey = "ACLPublicPost";
        private static readonly bool ACLPublicPostDefault = false;
        private const string ACLPrivatePostKey = "ACLPrivatePost";
        private static readonly bool ACLPrivatePostDefault = false;
        private const string ACLPrivateSelectedContactsKey = "ACLPrivateSelectedContacts";
        private static readonly string ACLPrivateSelectedContactsDefault = String.Empty;
        private const string ACLPrivateSelectedGroupsKey = "ACLPrivateSelectedGroups";
        private static readonly string ACLPrivateSelectedGroupsDefault = String.Empty;
        private const string SaveLocalAllowedKey = "SaveLocalAllowed";
        private static readonly bool SaveLocalAllowedDefault = true;
        private const string SaveFullsizePhotosAllowedKey = "SaveFullsizePhotosAllowed";
        private static readonly bool SaveFullsizePhotosAllowedDefault = false;
        private const string SendCoordinatesAllowedKey = "SendCoordinatesAllowed";
        private static readonly bool SendCoordinatesAllowedDefault = true;
        private const string NotificationActivatedKey = "NotificationActivated";
        private static readonly bool NotificationActivatedDefault = true;
        private const string NotificationEachNewsfeedAllowedKey = "NotificationEachNewsfeedAllowed";
        private static readonly bool NotificationEachNewsfeedAllowedDefault = true;
        private const string NotificationShowMessageContentKey = "NotificationShowMessageContent";
        private static readonly bool NotificationShowMessageContentDefault = true;
        private const string NotificationFreshnessTimeKey = "NotificationFreshnessTime";
        private static readonly int NotificationFreshnessTimeDefault = 15;
        private const string LastReadNetworkPostKey = "LastReadNetworkPost";
        private static readonly int LastReadNetworkPostDefault = 0;
        private const string UnseenNewsfeedItemsKey = "UnseenNewsfeedItems";
        private static readonly string UnseenNewsfeedItemsDefault = String.Empty;


		#endregion


        public static string FriendicaServer
        {
            get { return AppSettings.GetValueOrDefault(FriendicaServerKey, FriendicaServerDefault); }
            set { AppSettings.AddOrUpdateValue(FriendicaServerKey, value); }
        }

        public static string FriendicaUsername
        {
            get { return AppSettings.GetValueOrDefault(FriendicaUsernameKey, FriendicaUsernameDefault); }
            set { AppSettings.AddOrUpdateValue(FriendicaUsernameKey, value); }
        }

        public static string FriendicaPassword
        {
            get { return AppSettings.GetValueOrDefault(FriendicaPasswordKey, FriendicaPasswordDefault); }
            set { AppSettings.AddOrUpdateValue(FriendicaPasswordKey, value); }
        }

        public static bool AppThemeUseSystemTheme
		{
			get { return AppSettings.GetValueOrDefault(AppThemeUseSystemThemeKey, AppThemeUseSystemThemeDefault); }
			set { AppSettings.AddOrUpdateValue(AppThemeUseSystemThemeKey, value);
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

        public static bool UseDefaultAccentColor
        {
            get { return AppSettings.GetValueOrDefault(UseDefaultAccentColorKey, UseDefaultAccentColorDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(UseDefaultAccentColorKey, value);
                AppThemeModeChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static int SelectedAccentColor
        {
            get { return AppSettings.GetValueOrDefault(SelectedAccentColorKey, SelectedAccentColorDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(SelectedAccentColorKey, value);
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

        public static string StartPage
        {
            // needs to be string for backward compatibility, will be converted in SettingsViewmodel to int
            get { return AppSettings.GetValueOrDefault(StartPageKey, StartPageDefault); }
            set { AppSettings.AddOrUpdateValue(StartPageKey, value); }
        }

        public static bool ACLPublicPost
        {
            get { return AppSettings.GetValueOrDefault(ACLPublicPostKey, ACLPublicPostDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPublicPostKey, value); }
        }

        public static bool ACLPrivatePost
        {
            get { return AppSettings.GetValueOrDefault(ACLPrivatePostKey, ACLPrivatePostDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPrivatePostKey, value); }
        }

        public static string ACLPrivateSelectedContacts
        {
            get { return AppSettings.GetValueOrDefault(ACLPrivateSelectedContactsKey, ACLPrivateSelectedContactsDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPrivateSelectedContactsKey, value); }
        }

        public static string ACLPrivateSelectedGroups
        {
            get { return AppSettings.GetValueOrDefault(ACLPrivateSelectedGroupsKey, ACLPrivateSelectedGroupsDefault); }
            set { AppSettings.AddOrUpdateValue(ACLPrivateSelectedGroupsKey, value); }
        }

        public static bool SaveLocalAllowed
        {
            // old UWP version has stored this setting as string, therefore we need a conversion to bool for backward compatibility
            get 
            { 
                var str = AppSettings.GetValueOrDefault(SaveLocalAllowedKey, SaveLocalAllowedDefault.ToString());
                switch (str.ToLower()) { case "true": return true; case "false": return false; default: return true; }
            }
            set { AppSettings.AddOrUpdateValue(SaveLocalAllowedKey, value.ToString()); }
        }

        public static bool SaveFullsizePhotosAllowed
        {
            // defaults to false because setting this to true could need a lot of disk space on device
            get { return AppSettings.GetValueOrDefault(SaveFullsizePhotosAllowedKey, SaveFullsizePhotosAllowedDefault); }
            set { AppSettings.AddOrUpdateValue(SaveFullsizePhotosAllowedKey, value); }
        }

        public static bool SendCoordinatesAllowed
        {
            // old UWP version has stored this setting as string, therefore we need a conversion to bool for backward compatibility
            get
            {
                var str = AppSettings.GetValueOrDefault(SendCoordinatesAllowedKey, SendCoordinatesAllowedDefault.ToString());
                switch (str.ToLower()) { case "true": return true; case "false": return false; default: return true; }
            }
            set { AppSettings.AddOrUpdateValue(SendCoordinatesAllowedKey, value.ToString()); }
        }

        public static bool NotificationActivated
        {
            get { return AppSettings.GetValueOrDefault(NotificationActivatedKey, NotificationActivatedDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationActivatedKey, value); }
        }

        public static bool NotificationEachNewsfeedAllowed
        {
            get { return AppSettings.GetValueOrDefault(NotificationEachNewsfeedAllowedKey, NotificationEachNewsfeedAllowedDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationEachNewsfeedAllowedKey, value); }
        }

        public static bool NotificationShowMessageContent
        {
            get { return AppSettings.GetValueOrDefault(NotificationShowMessageContentKey, NotificationShowMessageContentDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationShowMessageContentKey, value); }
        }

        public static int NotificationFreshnessTime
        {
            get { return AppSettings.GetValueOrDefault(NotificationFreshnessTimeKey, NotificationFreshnessTimeDefault); }
            set { AppSettings.AddOrUpdateValue(NotificationFreshnessTimeKey, value); }
        }

        public static int LastReadNetworkPost
        {
            get { return AppSettings.GetValueOrDefault(LastReadNetworkPostKey, LastReadNetworkPostDefault); }
            set { AppSettings.AddOrUpdateValue(LastReadNetworkPostKey, value); }
        }

        // store the id's of the unseen newsfeed posts for marking them as unseen until user manually set the seen flag
        public static List<int> UnseenNewsfeedItems
        {
            get
            {
                var setting = AppSettings.GetValueOrDefault(UnseenNewsfeedItemsKey, UnseenNewsfeedItemsDefault);
                if (string.IsNullOrEmpty(setting))
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
                    AppSettings.AddOrUpdateValue(UnseenNewsfeedItemsKey, null);
                }
            }
        }
    }

}