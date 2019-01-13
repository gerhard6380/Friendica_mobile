using System;
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
            AppThemeUseSystemTheme = AppThemeUseSystemThemeDefault;
            AppThemeDarkModeEnabled = AppThemeDarkModeEnabledDefault;
            NavigationOnRightSide = NavigationOnRightSideDefault;
        }

		#region Setting Constants

		private const string AppThemeUseSystemThemeKey = "AppThemeUseSystemTheme";
		private static readonly bool AppThemeUseSystemThemeDefault = false;
        private const string AppThemeDarkModeEnabledKey = "AppThemeDarkModeEnabled";
        private static readonly bool AppThemeDarkModeEnabledDefault = false;
        private const string NavigationOnRightSideKey = "NavigationOnRightSide";
        private static readonly bool NavigationOnRightSideDefault = true;

		#endregion


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

        public static bool NavigationOnRightSide
        {
            get { return AppSettings.GetValueOrDefault(NavigationOnRightSideKey, NavigationOnRightSideDefault); }
            set
            {
                AppSettings.AddOrUpdateValue(NavigationOnRightSideKey, value);
                NavigationSideChanged?.Invoke(null, EventArgs.Empty);
            }
        }

    }
}