using Friendica_Mobile.Triggers;
using Friendica_Mobile.Mvvm;
using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Friendica_Mobile.Models;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class A4_ProfileUsers : Page
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public A4_ProfileUsers()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // load group if given through navigation
            var mvvm = this.DataContext as ProfileUsersViewmodel;
            mvvm.SourceContactsLoaded += Mvvm_SourceContactsLoaded;

            if (e.Parameter != null)
                mvvm.SelectedProfile = e.Parameter as FriendicaProfile;
            else
            {
                textblockNoUsersAvailable.Text = "Es gab einen Fehler beim Aufrufen der Benutzerliste!";
            }

            mvvm.InitialLoad();

            base.OnNavigatedTo(e);
        }

        private void Mvvm_SourceContactsLoaded(object sender, EventArgs e)
        {
            // prepare group headings for character overview
            (semanticZoomContacts.ZoomedOutView as ListViewBase).ItemsSource = cvsContacts.View.CollectionGroups;
        }

        private void listviewContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as ProfileUsersViewmodel;
            var listview = sender as GridView;
            foreach (FriendicaUserExtended contact in e.AddedItems)
            {
                var isAlreadySelected = mvvm.SelectedUsers.Count(c => c.User.UserCid == contact.User.UserCid);
                if (isAlreadySelected == 0)
                    mvvm.SelectedUsers.Add(contact);
            }
                
            foreach (FriendicaUserExtended contact in e.RemovedItems)
                mvvm.SelectedUsers.Remove(contact);

            mvvm.SaveCommand.RaiseCanExecuteChanged();
        }

        private void buttonGotoSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.Settings));
        }

    }
}
