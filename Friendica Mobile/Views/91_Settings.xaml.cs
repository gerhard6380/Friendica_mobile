using Friendica_Mobile.Models;
using Friendica_Mobile.Strings;
using Friendica_Mobile.Styles;
using SeeberXamarin.Controls;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Friendica_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : BaseContentPage
    {
        public Settings()
        {
            InitializeComponent();
            // fill picker with selectable items
            PickerStartPage.Items.Add(AppResources.pageTitleHome_Text);
            PickerStartPage.Items.Add(AppResources.pageTitleNetwork_Text);
            PickerStartPage.Items.Add(AppResources.pageTitleNewsfeed_Text);

            App.ShellSizeChanged += App_ShellSizeChanged;
        }



        void App_ShellSizeChanged(object sender, EventArgs e)
        {
            if (App.ShellWidth < 700 || App.ShellWidth < App.ShellHeight || App.DeviceType == Plugin.DeviceInfo.Abstractions.Idiom.Phone)
            {
                StackGeneral.Orientation = StackOrientation.Vertical;
            }
            else
            {
                // TODO: -60 ist nicht ideal, da hier auch Advertising noch berücksichtigt werden müsste
                StackGeneral.Orientation = StackOrientation.Horizontal;
                var width = (StackGeneral.Width == -1) ? App.ShellWidth - 60 : StackGeneral.Width;
                StackGeneralLogin.WidthRequest = width / 2;
                StackGeneralACL.WidthRequest = width / 2;
            }
            // change flyout dialog for deleting local data based on space
            GridFlyout.WidthRequest = (App.ShellWidth < App.ShellHeight) ? 300 : 600;
                
        }

        private void EntryFriendicaServer_Completed(object sender, EventArgs e)
        {
            EntryFriendicaUsername.Focus();
        }

        private void EntryFriendicaUsername_Completed(object sender, EventArgs eventArgs)
        {
            EntryFriendicaPassword.Focus();
        }

        private void EntryFriendicaPassword_Completed(object sender, System.EventArgs e)
        {
            ButtonTestConnection.Focus();
        }

        void ListViewContacts_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            // change selection even if text is clicked/tapped by user
            if (e.SelectedItem == null || sender == null)
                return;
            
            var item = e.SelectedItem as SelectableData<FriendicaUser>;
            item.IsSelected = !item.IsSelected;
            // now we can remove the selection again to enable clicking again to deselect
            var list = sender as ListView;
            list.SelectedItem = null;
        }

        void ListViewGroups_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            // change selection even if text is clicked/tapped by user
            if (e.SelectedItem == null || sender == null)
                return;

            var item = e.SelectedItem as SelectableData<FriendicaGroup>;
            item.IsSelected = !item.IsSelected;
            // now we can remove the selection again to enable clicking again to deselect
            var list = sender as ListView;
            list.SelectedItem = null;
        }

        void Label_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // text color is not updated within the customtabviewitem
            var label = sender as Label;
            label.TextColor = (Color)Application.Current.Resources["MainTextColor"];
        }

        void Button_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // background color is not updated within the customtabviewitem
            var button = sender as CustomButton;
            button.BackgroundColor = (Color)Application.Current.Resources["ButtonBackgroundColor"];
            button.TextColor = (Color)Application.Current.Resources["ButtonTextColor"];
        }
    }
}
