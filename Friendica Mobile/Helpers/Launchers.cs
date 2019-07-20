using System;
using Friendica_Mobile.Strings;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    public static class Launchers
    {        
        public static void OpenUrlWithZrl(string url, bool openInDemoMode = false)
        {
            //string baseUrl = "https://friendica.hasecom.at/profile/friendicamobile{0}";
            var profile = "";
            if (Settings.IsFriendicaLoginDefined())
            {
                // show support page with zrl link of user
                profile = "?zrl=" + Uri.EscapeUriString(Settings.FriendicaServer) + "/profile/" + Uri.EscapeUriString(Settings.FriendicaUsername);
                var extendedUrl = new Uri(url + profile);
                Device.OpenUri(extendedUrl);
            }
            else
            {
                if (openInDemoMode)
                {
                    Application.Current.MainPage.DisplayAlert(AppResources.pageTitleContacts_Text,
                                                              AppResources.MessageDialogNoSettingsNoProfileUrl, "OK");
                }
                else
                    Device.OpenUri(new Uri(url));

            }
        }
    }
}
