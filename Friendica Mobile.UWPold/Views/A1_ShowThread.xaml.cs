using Friendica_Mobile.UWP.Models;
using Friendica_Mobile.UWP.Mvvm;
using Friendica_Mobile.UWP.Triggers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friendica_Mobile.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class A1_ShowThread : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public A1_ShowThread()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);
        }


        private void appBarThreadAdd_Click(object sender, RoutedEventArgs e)
        {
            // add information of thread to be displayed in new posts
            var mvvm = this.DataContext as ShowThreadViewmodel;
            Frame.Navigate(typeof(Views.A0_NewPost), mvvm.ShowThread);
        }


        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var viewer = sender as ScrollViewer;
            
            // set the button for scrolling back to the top of the page to visible after leaving the first third of the visible area
            var mvvm = this.DataContext as ShowThreadViewmodel;
            if (viewer.VerticalOffset > viewer.ActualHeight / 3)
                mvvm.ShowScrollToTop = true;
            else
                mvvm.ShowScrollToTop = false;
        }


        private void buttonScrollToTop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // scroll back to the top of the page
            var zoom = viewerShowThread.ZoomFactor;
            
            viewerShowThread.ChangeView(0,0,zoom);
        }

        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var mvvm = this.DataContext as ShowThreadViewmodel;
            mvvm.PostToShow = e.Parameter as FriendicaPostExtended;
            mvvm.SelectedPostForAction = e.Parameter as FriendicaPostExtended;
            mvvm.ButtonShowProfileClicked += Mvvm_ButtonShowProfileClicked;
            // get the calling page as this will trigger different sources for loading data from App for thread
            foreach (PageStackEntry obj in Frame.BackStack)
                mvvm.NavigationSourcePage = obj.SourcePageType.Name;
            mvvm.InitialLoad();
            base.OnNavigatedTo(e);
        }

        private async void Mvvm_ButtonShowProfileClicked(object sender, System.EventArgs e)
        {
            var mvvm = sender as ShowThreadViewmodel;
            mvvm.ButtonShowProfileClicked -= Mvvm_ButtonShowProfileClicked;

            // implement A3_Browser
            var mvvmBrowser = new BrowserViewmodel();
            string userName = "";
            if (mvvm.SelectedPostForAction.Post.PostRetweetedStatus != null)
                userName = mvvm.SelectedPostForAction.Post.PostRetweetedStatus.PostUser.User.UserName;
            else
                userName = mvvm.SelectedPostForAction.Post.PostUser.User.UserName;

            mvvmBrowser.PageTitle = userName;
            mvvmBrowser.IsVisibleHeader = true;

            if (mvvm.SelectedPostForAction.Post.PostUser.User.UserUrl == "SAMPLE")
            {
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

                string userProfile = "";
                if (mvvm.SelectedPostForAction.Post.PostRetweetedStatus != null)
                    userProfile = mvvm.SelectedPostForAction.Post.PostRetweetedStatus.PostUser.User.UserUrl;
                else
                    userProfile = mvvm.SelectedPostForAction.Post.PostUser.User.UserUrl;

                string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;

                var url = String.Format(baseUrl, userProfile, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
                mvvmBrowser.Url = userProfile;
                mvvmBrowser.Uri = new Uri(url);
            }

            Frame.Navigate(typeof(Views.A3_Browser), mvvmBrowser);
        }
    }
}
