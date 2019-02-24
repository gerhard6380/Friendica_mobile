using System;
using System.Reflection;
using Xamarin.Forms;

[assembly: Dependency(typeof(Friendica_Mobile.UWP.GetSystemTheme))]
namespace Friendica_Mobile.UWP
{
    public class GetSystemTheme : IGetSystemTheme
    {
        public Friendica_Mobile.App.ApplicationTheme GetSelectedTheme()
        {
            // depending on the system we will try to get the system default theme (UWP, macos?)
            var systemTheme = DetectSystemTheme();

            // secondly we will check the user settings, as user can define a preferred theme within the app
            if (!Settings.AppThemeUseSystemTheme)
                return (Settings.AppThemeDarkModeEnabled) ? Friendica_Mobile.App.ApplicationTheme.Dark : Friendica_Mobile.App.ApplicationTheme.Light;
            
            // as a fallback the system will define Light theme as a default value
            return systemTheme;
        }

        private Friendica_Mobile.App.ApplicationTheme DetectSystemTheme()
        {
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var backgroundColor = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
            var black = Windows.UI.Color.FromArgb(255, 0, 0, 0);
            if (Equals(black, backgroundColor))
                return Friendica_Mobile.App.ApplicationTheme.Dark;
            else
                return Friendica_Mobile.App.ApplicationTheme.Light;
        }

        public string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}