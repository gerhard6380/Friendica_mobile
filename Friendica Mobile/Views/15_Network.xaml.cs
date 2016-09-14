using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Network : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public Network()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);
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

        private void buttonGotoSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.Settings));
        }

        private void appBarNetworkAdd_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.A0_NewPost));
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var viewer = sender as ScrollViewer;
            
            // check if we are reaching the last 10% of the total extent --> trigger to start loading new entries
            var atBottom = viewer.VerticalOffset > (viewer.ScrollableHeight - viewer.ActualHeight);

            // set the button for scrolling back to the top of the page to visible after leaving the first third of the visible area
            var mvvm = this.DataContext as NetworkViewmodel;
            if (viewer.VerticalOffset > viewer.ActualHeight / 3)
                mvvm.ShowScrollToTop = true;
            else
                mvvm.ShowScrollToTop = false;

            // start loading the next tranch of entries
            if (atBottom)
            {
                var context = this.DataContext as NetworkViewmodel;
                if (context.NetworkPosts.Count > 0)
                    context.LoadNextEntries();
            }
        }


        private void buttonScrollToTop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // scroll back to the top of the page
            var zoom = viewerNetwork.ZoomFactor;
            viewerNetwork.ChangeView(0,0,zoom);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // save all posts from Network to App.NetworkPosts
            var mvvm = this.DataContext as NetworkViewmodel;
            mvvm.ButtonAddCommentClicked -= Mvvm_ButtonAddCommentClicked;
            mvvm.ButtonShowThreadClicked -= Mvvm_ButtonShowThreadClicked;
            mvvm.ButtonShowProfileClicked -= Mvvm_ButtonShowProfileClicked;

            // set entries to read if navigating away
            mvvm.SetNewEntriesToSeen();
            mvvm.SaveToApp();

            base.OnNavigatedFrom(e);
        }

        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var mvvm = this.DataContext as NetworkViewmodel;
            this.Loaded += Network_Loaded;
            base.OnNavigatedTo(e);
        }


        private void Network_Loaded(object sender, RoutedEventArgs e)
        {
            var mvvm = this.DataContext as NetworkViewmodel;
            mvvm.ButtonShowThreadClicked += Mvvm_ButtonShowThreadClicked;
            mvvm.ButtonAddCommentClicked += Mvvm_ButtonAddCommentClicked;
            mvvm.ButtonShowProfileClicked += Mvvm_ButtonShowProfileClicked;

            if (mvvm.NetworkPosts == null)
                mvvm.NetworkPosts = new ObservableCollection<FriendicaPostExtended>();
            if (mvvm.NetworkThreads == null)
                mvvm.NetworkThreads = new ObservableCollection<FriendicaThread>();

            if (App.NetworkPosts != null || App.NetworkThreads != null)
            {
                if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
                {
                    mvvm.IsRefreshing = true;
                    mvvm.ReloadFromAppSamples();
                }
                else
                {
                    mvvm.IsRefreshing = true;
                    mvvm.ReloadFromApp();
                }
            }
            else
            {
                mvvm.LoadInitial();
            }
        }


        private void Mvvm_ButtonAddCommentClicked(object sender, EventArgs e)
        {            
            var mvvm = sender as NetworkViewmodel;
            mvvm.ButtonAddCommentClicked -= Mvvm_ButtonAddCommentClicked;
            mvvm.ButtonShowThreadClicked -= Mvvm_ButtonShowThreadClicked;
            mvvm.ButtonShowProfileClicked -= Mvvm_ButtonShowProfileClicked;
            // implement A0_NewPost
            Frame.Navigate(typeof(Views.A0_NewPost), mvvm.PostToShowThread);
        }

        private void Mvvm_ButtonShowThreadClicked(object sender, EventArgs e)
        {
            var mvvm = sender as NetworkViewmodel;
            mvvm.ButtonAddCommentClicked -= Mvvm_ButtonAddCommentClicked;
            mvvm.ButtonShowThreadClicked -= Mvvm_ButtonShowThreadClicked;
            mvvm.ButtonShowProfileClicked -= Mvvm_ButtonShowProfileClicked;
            // implement A1_ShowThreadPage
            Frame.Navigate(typeof(Views.A1_ShowThread), mvvm.PostToShowThread);
        }

        private async void Mvvm_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            var mvvm = sender as NetworkViewmodel;
            mvvm.ButtonAddCommentClicked -= Mvvm_ButtonAddCommentClicked;
            mvvm.ButtonShowThreadClicked -= Mvvm_ButtonShowThreadClicked;
            mvvm.ButtonShowProfileClicked -= Mvvm_ButtonShowProfileClicked;

            // implement A3_Browser
            var mvvmBrowser = new BrowserViewmodel();
            string userName = "";
            if (mvvm.PostToShowThread.Post.PostRetweetedStatus != null)
                userName = mvvm.PostToShowThread.Post.PostRetweetedStatus.PostUser.User.UserName;
            else
                userName = mvvm.PostToShowThread.Post.PostUser.User.UserName;

            mvvmBrowser.PageTitle = userName;
            mvvmBrowser.IsVisibleHeader = true;

            if (mvvm.PostToShowThread.Post.PostUser.User.UserUrl == "SAMPLE")
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
                if (mvvm.PostToShowThread.Post.PostRetweetedStatus != null)
                    userProfile = mvvm.PostToShowThread.Post.PostRetweetedStatus.PostUser.User.UserUrl;
                else
                    userProfile = mvvm.PostToShowThread.Post.PostUser.User.UserUrl;

                string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;

                var url = String.Format(baseUrl, userProfile, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
                mvvmBrowser.Url = userProfile;
                mvvmBrowser.Uri = new Uri(url);
            }

            Frame.Navigate(typeof(Views.A3_Browser), mvvmBrowser);
        }

    }

    public class BitmapItem
    {
        public ImageSource Source { get; set; }
    }


}
