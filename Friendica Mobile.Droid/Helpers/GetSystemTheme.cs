using System;
using System.Reflection;
using Xamarin.Forms;

[assembly: Dependency(typeof(Friendica_Mobile.Droid.GetSystemTheme))]
namespace Friendica_Mobile.Droid
{
    public class GetSystemTheme : IGetSystemTheme
    {
        public App.ApplicationTheme GetSelectedTheme()
        {
            // depending on the system we will try to get the system default theme (UWP, macos?)
            // we do not have a default theme on android at the moment (NOV-2018)

            // secondly we will check the user settings, as user can define a preferred theme within the app
            if (!Settings.AppThemeUseSystemTheme)
                return (Settings.AppThemeDarkModeEnabled) ? App.ApplicationTheme.Dark : App.ApplicationTheme.Light;

            // as a fallback the system will define Light theme as a default value
            return App.ApplicationTheme.Light;
        }

        public string GetAppVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
