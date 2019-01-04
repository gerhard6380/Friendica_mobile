using Friendica_Mobile.UWP.Mvvm;
using Friendica_Mobile;
using Friendica_Mobile.Strings;
using Friendica_Mobile.Viewmodels;
using Friendica_Mobile.UWP.Triggers;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Friendica_Mobile.UWP.Views
{
    public sealed partial class Network : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public Network()
        {
            if (App.NetworkVm == null)
                App.NetworkVm = new Viewmodels.NetworkViewmodel();
            this.DataContext = App.NetworkVm;

            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // initial setting of the width of the listview to the setting
            listviewNetwork.Width = App.Settings.ShellWidth;

            // initial setting of the bottomappbar if necessary
            bottomAppBar.Margin = App.Settings.BottomAppBarMargin;

            // react on changes of the counter with changing color of top button
            StaticGlobalParameters.CounterNetworkChanged += StaticGlobalParameters_CounterNetworkChanged;
            // react on changes of screen size
            App.Settings.PropertyChanged += Settings_PropertyChanged;
            App.SendingNewPostChanged += App_SendingNewPostChanged;
        }

        private void App_SendingNewPostChanged(object sender, System.EventArgs e)
        {
            var mvvm = this.DataContext as Viewmodels.NetworkViewmodel;
            mvvm.IsSendingNewPost = App.IsSendingNewPost;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShellWidth")
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                    listviewNetwork.Width = App.Settings.ShellWidth - 84;
                else
                    listviewNetwork.Width = App.Settings.ShellWidth;

                bottomAppBar.Margin = App.Settings.BottomAppBarMargin;
            }

            if (e.PropertyName == "OrientationDevice")
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.DesktopLandscape || App.Settings.OrientationDevice == OrientationDeviceFamily.DesktopPortrait)
                    gridDropArea.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    gridDropArea.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }


        private void StaticGlobalParameters_CounterNetworkChanged(object sender, System.EventArgs e)
        {
            // red button if there are new entries, white if not
            if (sender != null)
            {
                if ((int)sender > 0)
                    buttonScrollToTopFontIcon.Foreground = new SolidColorBrush(Colors.Red);
                else
                    buttonScrollToTopFontIcon.Foreground = new SolidColorBrush(Colors.White);
            }
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var mvvm = this.DataContext as Viewmodels.NetworkViewmodel;
            mvvm.ButtonAddCommentClicked += Mvvm_ButtonAddCommentClicked;
            mvvm.ButtonShowProfileClicked += Mvvm_ButtonShowProfileClicked;
            mvvm.ButtonRetweetClicked += Mvvm_ButtonRetweetClicked;
            mvvm.ButtonAddNewEntryClicked += Mvvm_ButtonAddNewEntryClicked;
            mvvm.ButtonShowMapClicked += Mvvm_ButtonShowMapClicked;
            mvvm.UserShowProfileClicked += Mvvm_UserShowProfileClicked;

            if (mvvm.NetworkThreadsContainer != null)
            {
                mvvm.NetworkThreads = new System.Collections.ObjectModel.ObservableCollection<FriendicaThread>();
                foreach (var thread in mvvm.NetworkThreadsContainer)
                {
                    mvvm.NetworkThreads.Add(thread);
                    listviewNetwork.UpdateLayout();
                }
                mvvm.UpdateCounters();
                mvvm.NetworkThreadsContainer = null;
            }

            await mvvm.LoadInitial();
        }


        private async void Mvvm_UserShowProfileClicked(object sender, EventArgs e)
        {
            var user = sender as Friendica_Mobile.Models.FriendicaUser;

            // implement A3_Browser
            var mvvmBrowser = new BrowserViewmodel();
            mvvmBrowser.PageTitle = user.User.UserName;
            mvvmBrowser.IsVisibleHeader = true;

            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://" ||
                App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
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
                var userProfile = user.User.UserUrl;

                string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;

                var url = String.Format(baseUrl, userProfile, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
                mvvmBrowser.Url = userProfile;
                mvvmBrowser.Uri = new Uri(url);
            }

            Frame.Navigate(typeof(Views.A3_Browser), mvvmBrowser);

        }

        private async void Mvvm_ButtonShowMapClicked(object sender, EventArgs e)
        {
            var post = sender as Friendica_Mobile.Models.FriendicaPost;
            var geo = post.Post.PostGeo;
            // bing expects the location name with %20 for spaces and other escapes
            var location = Uri.EscapeDataString(post.Post.PostLocation);
            // bing expects coordinates with a point instead of a comma
            var latitude = geo.FriendicaGeoCoordinates[0].ToString();
            latitude = latitude.Replace(",", ".");
            var longitude = geo.FriendicaGeoCoordinates[1].ToString();
            longitude = longitude.Replace(",", ".");

            // point to exact coordinates if available, otherwise use Location query
            string url;
            if (geo.FriendicaGeoType == "Point" && geo.FriendicaGeoCoordinates.Count > 0)
                url = String.Format("bingmaps:?collection=point.{0}_{1}_{2}", latitude, longitude, location);
            //url = String.Format("bingmaps:?cp={0}~{1}", geo.FriendicaGeoCoordinates[0], geo.FriendicaGeoCoordinates[1]);
            else
                url = String.Format("bingmaps:?where={0}", location);

            // launch bing maps application
            await Launcher.LaunchUriAsync(new Uri(url));
        }

        private async void Mvvm_ButtonShowProfileClicked(object sender, System.EventArgs e)
        {
            // implement A3_Browser
            var post = sender as Friendica_Mobile.Models.FriendicaPost;
            var mvvmBrowser = new BrowserViewmodel();
            string userName = "";
            if (post.Post.PostRetweetedStatus != null && post.Post.PostRetweetedStatus.PostUser != null)
                userName = post.Post.PostRetweetedStatus.PostUser.UserName;
            else
                userName = post.Post.PostUser.UserName;

            mvvmBrowser.PageTitle = userName;
            mvvmBrowser.IsVisibleHeader = true;

            if (post.Post.PostUser.UserUrl == "SAMPLE")
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
                if (post.Post.PostRetweetedStatus != null && post.Post.PostRetweetedStatus.PostUser != null)
                    userProfile = post.Post.PostRetweetedStatus.PostUser.UserUrl;
                else
                    userProfile = post.Post.PostUser.UserUrl;

                string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;

                var url = String.Format(baseUrl, userProfile, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
                mvvmBrowser.Url = userProfile;
                mvvmBrowser.Uri = new Uri(url);
            }

            Frame.Navigate(typeof(Views.A3_Browser), mvvmBrowser);
        }

        private void Mvvm_ButtonAddCommentClicked(object sender, System.EventArgs e)
        {
            // implement A0_NewPost with the currently clicked item
            Frame.Navigate(typeof(Views.A0_NewPost), sender);
        }

        private void Mvvm_ButtonAddNewEntryClicked(object sender, System.EventArgs e)
        {
            // implement navigation to A0_NewPost without a parameter = new post
            Frame.Navigate(typeof(Views.A0_NewPost));
        }


        private void Mvvm_ButtonRetweetClicked(object sender, System.EventArgs e)
        {
            // implement A0_NewPost
            Frame.Navigate(typeof(Views.A0_NewPost), sender);
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            var mvvm = this.DataContext as Viewmodels.NetworkViewmodel;
            mvvm.ButtonAddCommentClicked -= Mvvm_ButtonAddCommentClicked;
            mvvm.ButtonShowProfileClicked -= Mvvm_ButtonShowProfileClicked;
            mvvm.ButtonRetweetClicked -= Mvvm_ButtonRetweetClicked;
            mvvm.ButtonAddNewEntryClicked -= Mvvm_ButtonAddNewEntryClicked;
            mvvm.ButtonShowMapClicked -= Mvvm_ButtonShowMapClicked;
            mvvm.UserShowProfileClicked -= Mvvm_UserShowProfileClicked;

            // remove entries from NewsfeedThreads when navigating away, otherwise we will get a layout loop when returning 
            mvvm.NetworkThreadsContainer = new System.Collections.Generic.List<FriendicaThread>();
            foreach (var thread in mvvm.NetworkThreads)
                mvvm.NetworkThreadsContainer.Add(thread);
            mvvm.NetworkThreads = null;
        }


        private async void ViewerNetwork_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // set the button for scrolling back to the top of the page to visible after leaving the first third of the visible area
            if (viewerNetwork.VerticalOffset > viewerNetwork.ActualHeight / 3)
                buttonScrollToTop.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                buttonScrollToTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // check if we are reaching the last 10% of the total extent --> trigger to start loading new entries
            var atBottom = viewerNetwork.VerticalOffset > (viewerNetwork.ScrollableHeight - viewerNetwork.ActualHeight);

            // start loading the next tranch of entries
            if (atBottom)
            {
                var mvvm = this.DataContext as Viewmodels.NetworkViewmodel;
                if (mvvm.NetworkThreads.Count > 0 && !mvvm.IsLoadingNext)
                    await mvvm.LoadNext();
            }
        }


        private void ButtonGotoSettings_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // navigate to the Settings page
            Frame.Navigate(typeof(Views.Settings));
        }


        private void ButtonScrollToTop_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // scroll back to the top of the page
            var zoom = viewerNetwork.ZoomFactor;
            viewerNetwork.ChangeView(0, 0, zoom);
        }



        private void listviewNetwork_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var listview = sender as ListView;
            listview.UpdateLayout();
        }


        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            App.PasteClipboardContent(e.DataView);
        }


        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            var hasText = e.DataView.Contains(StandardDataFormats.Text);
            var hasStorageitems = e.DataView.Contains(StandardDataFormats.StorageItems);
            bool hasContent = hasText || hasStorageitems;
            e.AcceptedOperation = hasContent ? DataPackageOperation.Copy : DataPackageOperation.None;
            if (hasContent)
            {
                e.DragUIOverride.Caption = loader.GetString("dragCaption");
                e.DragUIOverride.IsGlyphVisible = true;
                e.DragUIOverride.IsContentVisible = true;
            }
        }

    }
}