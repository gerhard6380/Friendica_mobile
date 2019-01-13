using System;
namespace Friendica_Mobile
{
    public interface IGetSystemTheme
    {
        // get the selected theme
        App.ApplicationTheme GetSelectedTheme();
        // depending on the system we will try to get the system default them (UWP, macos?)
        // secondly we will check the user settings, as user can define a preferred theme within the app
        // as a fallback the system will define Light theme as a default value
    }
}
