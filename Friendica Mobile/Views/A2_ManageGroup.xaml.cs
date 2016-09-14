using Friendica_Mobile.Triggers;
using Friendica_Mobile.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Friendica_Mobile.Models;
using Windows.UI.Popups;
using Windows.ApplicationModel.Resources;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class A2_ManageGroup : Page
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public A2_ManageGroup()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // load group if given through navigation
            var mvvm = this.DataContext as ManageGroupViewmodel;
            mvvm.SourceContactsLoaded += Mvvm_SourceContactsLoaded;
            mvvm.InitialLoad();

            if (e.Parameter != null)
            {
                mvvm.IsNewGroup = false;
                mvvm.GroupOld = e.Parameter as FriendicaGroup;
            }
            else
            {
                mvvm.IsNewGroup = true;
                mvvm.GroupOld = new FriendicaGroup();
            }

            base.OnNavigatedTo(e);
        }

        private void Mvvm_SourceContactsLoaded(object sender, EventArgs e)
        {
            // prepare group headings for character overview
            (semanticZoomContacts.ZoomedOutView as ListViewBase).ItemsSource = cvsContacts.View.CollectionGroups;
        }

        private void listviewContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as ManageGroupViewmodel;
            var listview = sender as GridView;
            foreach (FriendicaUserExtended contact in e.AddedItems)
            {
                var isAlreadySelected = mvvm.SelectedItems.Count(c => c.User.UserCid == contact.User.UserCid);
                if (isAlreadySelected == 0)
                    mvvm.SelectedItems.Add(contact);
            }
                
            foreach (FriendicaUserExtended contact in e.RemovedItems)
                mvvm.SelectedItems.Remove(contact);

            mvvm.SaveCommand.RaiseCanExecuteChanged();
        }

        
    }
}
