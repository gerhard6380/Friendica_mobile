using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Contacts : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public Contacts()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // the bottomappbar might cover important things on Phone devices if keyboard is shown - collapse the appbar thanl
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Collapsed;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Visible;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            pivotContacts.SelectedIndex = App.ContactsSelectedPivot;

            var mvvm = this.DataContext as ContactsViewmodel;
            this.Loaded += Contacts_Loaded;
            mvvm.SourceFriendsLoaded += Mvvm_SourceFriendsLoaded;
            mvvm.SourceForumsLoaded += Mvvm_SourceForumsLoaded;
            mvvm.ChangeGroupFired += Mvvm_ChangeGroupFired;
            mvvm.ButtonShowProfileClicked += Mvvm_ButtonShowProfileClicked;
            base.OnNavigatedTo(e);
        }

        private async void Mvvm_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            var mvvm = sender as ContactsViewmodel;
            //mvvm.ButtonShowProfileClicked -= Mvvm_ButtonShowProfileClicked;

            // implement A3_Browser
            var mvvmBrowser = new BrowserViewmodel();
            var userName = mvvm.SelectedUser.User.UserName;
            mvvmBrowser.PageTitle = userName;
            mvvmBrowser.IsVisibleHeader = true;

            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
|| App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                // when no settings we have only sample contacts which have no real profile page
                string errorMsg;
                errorMsg = loader.GetString("messageDialogBrowserNoProfilePage");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);

                // we are in sample data test modus - no profile of the testusers to display, show support page
                mvvmBrowser.Url = "http://mozartweg.dyndns.org/friendica/profile/friendicamobile";
                mvvmBrowser.Uri = new Uri(mvvmBrowser.Url);
            }
            else
            {
                // build link to the profile of the author incl. zrl link to the own profile
                string baseUrl = "{0}?zrl={1}&timestamp={2}";
                var userProfile = mvvm.SelectedUser.User.UserUrl;

                string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;

                var url = String.Format(baseUrl, userProfile, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
                mvvmBrowser.Url = userProfile;
                mvvmBrowser.Uri = new Uri(url);
            }

            Frame.Navigate(typeof(Views.A3_Browser), mvvmBrowser);
        }

        private void Mvvm_ChangeGroupFired(object sender, EventArgs e)
        {
            var group = sender as ContactsViewmodel;
            Frame.Navigate(typeof(Views.A2_ManageGroup), group.ChangingGroup);
        }

        private void Mvvm_SourceForumsLoaded(object sender, EventArgs e)
        {
            // prepare group headings for character overview
            (semanticZoomForums.ZoomedOutView as ListViewBase).ItemsSource = cvsContactsForums.View.CollectionGroups;
        }

        private void Mvvm_SourceFriendsLoaded(object sender, EventArgs e)
        {
            // prepare group headings for character overview
            (semanticZoomFriends.ZoomedOutView as ListViewBase).ItemsSource = cvsContactsFriends.View.CollectionGroups;
        }

        private void Contacts_Loaded(object sender, RoutedEventArgs e)
        {
            var mvvm = this.DataContext as ContactsViewmodel;

            // in App.xaml.cs we already load contacts in behind, if these data is alread available, we can reload data just from App
            if (App.IsLoadedContacts)
            {
                // check if no settings, otherwise we will load from App.Friends and hint on no settings vanishes
                if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
                    mvvm.InitialLoad();
                else
                    mvvm.ReloadContacts();
            }
            else
                mvvm.InitialLoad();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Wechsel auf Seite "ManageGroup"
            // User soll dort die Gruppe pflegen können (sobald Änderungen gemacht wurden, Navigation away triggern - Shell.xaml)
            // Daten in einer FriendicaGroup halten und wie hier posten
            // wenn erfolgreich, dann hier einen Reload auslösen
            Frame.Navigate(typeof(Views.A2_ManageGroup));
        }

        private void GetGroup_FriendicaGroupsLoaded(object sender, EventArgs e)
        {
            var getGroup = sender as GetFriendicaGroups;
            var result = getGroup.ReturnString;
            var mvvm = this.DataContext as ContactsViewmodel;
            mvvm.ReloadContacts();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            App.ContactsSelectedPivot = pivot.SelectedIndex;
        }
    }
}
