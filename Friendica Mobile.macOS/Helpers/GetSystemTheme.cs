using System;
using System.Reflection;
using Friendica_Mobile;
using Xamarin.Forms;

[assembly: Dependency(typeof(Friendica_Mobile.macOS.GetSystemTheme))]
namespace Friendica_Mobile.macOS
{
    public class GetSystemTheme : IGetSystemTheme
    {
        public App.ApplicationTheme GetSelectedTheme()
        {
            // depending on the system we will try to get the system default theme (UWP, macos?)
            // on macos we cannot react on the change of system mode when app is open, restart is needed
            if (Settings.AppThemeUseSystemTheme)
            {
                if (AppKit.NSAppearance.CurrentAppearance.Name == AppKit.NSAppearance.NameDarkAqua)
                    return App.ApplicationTheme.Dark;
                else
                    return App.ApplicationTheme.Light;
            }

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
