using Friendica_Mobile.UWP.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friendica_Mobile.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Help : Page
    {
        public Help()
        {
            this.InitializeComponent();
        }

        private void buttonLinkAppSupportpage_Click(object sender, RoutedEventArgs e)
        {
            var mvvm = new BrowserViewmodel();
            mvvm.PageTitle = "Friendica Mobile Support";
            mvvm.IsVisibleHeader = false;

            // build link to the support forum incl. zrl link to the own profile
            string baseUrl = "https://friendica.hasecom.at/profile/friendicamobile?zrl={0}&timestamp={1}";
            string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;
            var url = String.Format(baseUrl, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
            mvvm.Uri = new Uri(url);

            Frame.Navigate(typeof(Views.A3_Browser), mvvm);
        }

        private void buttonLinkFriendicaSupport_Click(object sender, RoutedEventArgs e)
        {
            var mvvm = new BrowserViewmodel();
            mvvm.PageTitle = "Friendica Support";
            mvvm.IsVisibleHeader = false;

            // build link to the support forum incl. zrl link to the own profile
            string baseUrl = "https://forum.friendi.ca/profile/helpers?zrl={0}&timestamp={1}";
            string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;
            var url = String.Format(baseUrl, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
            mvvm.Uri = new Uri(url);

            Frame.Navigate(typeof(Views.A3_Browser), mvvm);

        }

        private async void buttonLinkFriendicaDevelopers_Click(object sender, RoutedEventArgs e)
        {
            // build link to the support forum incl. zrl link to the own profile
            string baseUrl = "https://forum.friendi.ca/profile/developers?zrl={0}&timestamp={1}";
            string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;
            var url = String.Format(baseUrl, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));

            // TODO: due to invalid certificate and missing CSS and images in workaround we go to normal link instead
            var success = await Launcher.LaunchUriAsync(new Uri(url));

            //var mvvm = new BrowserViewmodel();
            //mvvm.PageTitle = "Friendica Developers";
            //mvvm.IsVisibleHeader = false;

            //mvvm.Uri = new Uri(url);

            //Frame.Navigate(typeof(Views.A3_Browser), mvvm);

        }
    }
}
