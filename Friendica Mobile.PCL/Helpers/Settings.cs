// Helpers/Settings.cs
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Friendica_Mobile.PCL
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


        #region Setting Constants

        private const string FriendicaServerKey = "FriendicaServer";
        private static readonly string FriendicaServerDefault = string.Empty;

        private const string FriendicaUsernameKey = "FriendicaUsername";
        private static readonly string FriendicaUsernameDefault = string.Empty;

        private const string FriendicaPasswordKey = "FriendicaPassword";
        private static readonly string FriendicaPasswordDefault = string.Empty;


        #endregion

        public static string FriendicaServer
        {
            get { return AppSettings.GetValueOrDefault<string>(FriendicaServerKey, FriendicaServerDefault); }
            set { AppSettings.AddOrUpdateValue<string>(FriendicaServerKey, value); }
        }


        public static string FriendicaUsername
        {
            get
            {
                return AppSettings.GetValueOrDefault<string>(FriendicaUsernameKey, FriendicaUsernameDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue<string>(FriendicaUsernameKey, value);
            }
        }

        public static string FriendicaPassword
        {
            get { return AppSettings.GetValueOrDefault<string>(FriendicaPasswordKey, FriendicaPasswordDefault); }
            set { AppSettings.AddOrUpdateValue<string>(FriendicaPasswordKey, value); }
        }

    }
}
