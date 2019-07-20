using Friendica_Mobile.Models;
using Friendica_Mobile.Styles;
using Friendica_Mobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Friendica_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Contacts : BaseContentPage
    {
        public Contacts()
        {
            InitializeComponent();

            App.ShellSizeChanged += App_ShellSizeChanged;

            // attach to event raised if NavigationAllowed = false, but backbutton has been touched
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;
            shell.UnallowedBackNavigationDetected += Shell_UnallowedBackNavigationDetected;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;
            shell.UnallowedBackNavigationDetected -= Shell_UnallowedBackNavigationDetected;
        }

        async void Shell_UnallowedBackNavigationDetected(object sender, System.EventArgs e)
        {
            var mvvm = this.BindingContext as ContactsViewModel;
            await mvvm.CheckBeforeCancelling();
        }


        void App_ShellSizeChanged(object sender, System.EventArgs e)
        {
            // avoid too small listview if used on mobile device in landscape mode, therefore fix height on higher value (user then must scroll double)
            // UWP shows an additional title bar, therefore add more space
            if (App.ShellHeight < 400)
                FrameGroupDetails.HeightRequest = 600;
            else
            {
                var delta = (Device.RuntimePlatform == Device.UWP) ? 220 : 144;
                FrameGroupDetails.HeightRequest = App.ShellHeight - delta;
            }

            // place group editor right of group list if space is sufficient, otherwise overlay group editor
            if (App.ShellWidth < 500)
            {
                Grid.SetColumnSpan(StackLayoutGroups, 2);
                Grid.SetColumnSpan(FrameGroupDetails, 2);
                Grid.SetColumn(FrameGroupDetails, 0);
                FrameGroupDetails.Margin = new Thickness(0);
            }
            else
            {
                Grid.SetColumnSpan(StackLayoutGroups, 1);
                Grid.SetColumnSpan(FrameGroupDetails, 1);
                Grid.SetColumn(FrameGroupDetails, 1);
                FrameGroupDetails.Margin = new Thickness(0, 0, 12, 24);
            }
        }

        void Entry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry.IsEnabled)
                entry.BackgroundColor = (Color)Application.Current.Resources["EntryBackgroundColor"];
            else
                entry.BackgroundColor = (Color)Application.Current.Resources["EntryDisabledBackgroundColor"];
        }

        // needed to avoid rendering errors in ios designer
        void Handle_OnCommandBarPositionChanged(object sender, System.EventArgs e)
        {
        }
        void Handle_MoreOptionsButtonClicked(object sender, System.EventArgs e)
        {
        }

        void ListViewGroups_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            // display data of the selected group
            var group = e.Item as FriendicaGroup;
            var mvvm = this.BindingContext as ContactsViewModel;
            mvvm.GroupInEditor = group;
            mvvm.IsGroupEditorVisible = true;
            mvvm.IsGroupEditorInChangeMode = true;
            mvvm.CreateGroupEditorListView();
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

            var mvvm = this.BindingContext as ContactsViewModel;
            mvvm.SetIsGroupSaveEnabled();
        }
    }
}