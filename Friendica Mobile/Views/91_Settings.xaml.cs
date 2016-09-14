using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
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
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);
        }


        //protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        //{
        //    if (pageMvvm.TestConnectionInProgress)
        //        e.Cancel = true;
        //    if (pageMvvm.FriendicaServer != App.Settings.FriendicaServer || 
        //        pageMvvm.FriendicaUsername != App.Settings.FriendicaUsername || 
        //        pageMvvm.FriendicaPassword != App.Settings.FriendicaPassword)
        //    {
        //        //Shell rootFrame = Window.Current.Content as Shell;
        //        //rootFrame.cancelNavigation = true;
        //        var dialog = new MessageDialog("Einstellungen wurden ohne Test verändert. Wollen Sie trotzdem die Seite verlassen?", "Sure?");
        //        dialog.Commands.Add(new UICommand("Ja"));
        //        dialog.Commands.Add(new UICommand("Nein, ich teste."));

        //        var result = await dialog.ShowAsync();
        //        if (result.Label == "Ja")
        //            e.Cancel = false;
        //        else
        //            e.Cancel = true;
        //    }

        //    base.OnNavigatingFrom(e);
        //}

        private void textboxFriendicaServer_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // switch to next textbox if user hits enter
            if (e.Key == Windows.System.VirtualKey.Enter)
                textboxFriendicaUsername.Focus(FocusState.Keyboard); 
        }

        private void textboxFriendicaUsername_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // switch to next textbox if user hits enter
            if (e.Key == Windows.System.VirtualKey.Enter)
                textboxFriendicaPassword.Focus(FocusState.Keyboard);
        }

        private void textboxFriendicaPassword_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // switch to button if user hits enter
            if (e.Key == Windows.System.VirtualKey.Enter)
                buttonTestConnection.Focus(FocusState.Keyboard);
        }

       
        private void listBoxPrivateSelectedContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as SettingsViewmodel;
            var listview = sender as ListBox;
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

        private void listBoxPrivateSelectedGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as SettingsViewmodel;
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

    }
}
