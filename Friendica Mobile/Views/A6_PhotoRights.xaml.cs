using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace Friendica_Mobile.Views
{
    public sealed partial class A6_PhotoRights : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();


        public A6_PhotoRights()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // load eventhandlers needed for the page
            var mvvm = this.DataContext as PhotoRightsViewmodel;
            mvvm.SourceContactsLoaded += Mvvm_SourceContactsLoaded;
            mvvm.OnSelectedContactsChanged += Mvvm_OnSelectedContactsChanged;

            // initial load data from App.PhotosVm, need to be separately called in order to ensure that SourceContactsLoaded eventhandler works
            mvvm.LoadPhotoRights();
        }


        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var mvvm = this.DataContext as PhotoRightsViewmodel;

            // check if private access has at least one group or contact defined otherwise cancel navigation
            if (mvvm.IsPrivatePhoto && mvvm.SelectedGroups.Count == 0 && mvvm.SelectedContacts.Count == 0)
            {
                // cancel navigating away and give message to user
                e.Cancel = true;
                string errorMsg = loader.GetString("messageDialogPhotoRightsNoContactSelected");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }

            // replace rights in App.PhotoVm accordingly to new settings
            if (!e.Cancel)
            {
                // saves changed rigts back to App.PhotoVm for further use
                mvvm.SaveChangedRights();

                // set HideNavigationElements back to normal
                App.Settings.HideNavigationElements = false;
            }

            base.OnNavigatingFrom(e);
        }


        private void Mvvm_OnSelectedContactsChanged(object sender, System.EventArgs e)
        {
            // re-select items in view by code after setting the selections back to original by code
            var items = listviewPrivateSelectedContacts.Items;
            var mvvm = sender as PhotoRightsViewmodel;
            foreach (var contact in mvvm.SelectedContacts)
            {
                var index = items.IndexOf(contact);
                listviewPrivateSelectedContacts.SelectRange(new ItemIndexRange(index, 1));
            }
        }


        private void Mvvm_SourceContactsLoaded(object sender, System.EventArgs e)
        {
            // prepare group headings for character overview
            (semanticZoomContacts.ZoomedOutView as ListViewBase).ItemsSource = cvsContacts.View.CollectionGroups;
        }


        private void listBoxPrivateSelectedGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as PhotoRightsViewmodel;
            var listview = sender as ListBox;
            foreach (FriendicaGroup group in e.AddedItems)
            {
                var isAlreadySelected = mvvm.SelectedGroups.Count(g => g.GroupGid == group.GroupGid);
                if (isAlreadySelected == 0)
                    mvvm.SelectedGroups.Add(group);
            }

            foreach (FriendicaGroup group in e.RemovedItems)
                mvvm.SelectedGroups.Remove(group);

            // initialize set sequence
            mvvm.SelectedGroups = mvvm.SelectedGroups;
        }


        private void listviewPrivateSelectedContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as PhotoRightsViewmodel;
            var listview = sender as GridView;
            foreach (FriendicaUserExtended contact in e.AddedItems)
            {
                var isAlreadySelected = mvvm.SelectedContacts.Count(c => c.User.UserCid == contact.User.UserCid);
                if (isAlreadySelected == 0)
                    mvvm.SelectedContacts.Add(contact);
            }

            foreach (FriendicaUserExtended contact in e.RemovedItems)
                mvvm.SelectedContacts.Remove(contact);

            // initialize set sequence
            mvvm.SelectedContacts = mvvm.SelectedContacts;
        }


        private void hyperlinkButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // show flyout with the whole hint text in mobile portrait and landscape modes (not in continuum)
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
