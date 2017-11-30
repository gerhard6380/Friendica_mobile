using BackgroundTasks;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.PCL;
using Friendica_Mobile.PCL.Strings;
using Friendica_Mobile.PCL.Viewmodels;
using Friendica_Mobile.Triggers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Friendica_Mobile.Views
{
    public sealed partial class Newsfeed : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public Newsfeed()
        {
            if (App.NetworkVm == null)
                App.NetworkVm = new PCL.Viewmodels.NetworkViewmodel();
            this.DataContext = App.NetworkVm;

            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // initial setting of the width of the listview to the setting
            listviewNewsfeed.Width = App.Settings.ShellWidth;

            // initial setting of the bottomappbar if necessary
            bottomAppBar.Margin = App.Settings.BottomAppBarMargin;

            // react on changes of the counter with changing color of top button
            StaticGlobalParameters.CounterNewsfeedChanged += StaticGlobalParameters_CounterNewsfeedChanged;
            // react on changes of screen size
            App.Settings.PropertyChanged += Settings_PropertyChanged;
            App.SendingNewPostChanged += App_SendingNewPostChanged;
        }

        private void App_SendingNewPostChanged(object sender, System.EventArgs e)
        {
            var mvvm = this.DataContext as PCL.Viewmodels.NetworkViewmodel;
            mvvm.IsSendingNewPost = App.IsSendingNewPost;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShellWidth")
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                    listviewNewsfeed.Width = App.Settings.ShellWidth - 84;
                else
                    listviewNewsfeed.Width = App.Settings.ShellWidth;

                bottomAppBar.Margin = App.Settings.BottomAppBarMargin;
            }
        }


        private void StaticGlobalParameters_CounterNewsfeedChanged(object sender, System.EventArgs e)
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

            var mvvm = this.DataContext as PCL.Viewmodels.NetworkViewmodel;
            mvvm.ButtonRetweetClicked += Mvvm_ButtonRetweetClicked;
            mvvm.LikeNewsfeedClicked += Mvvm_LikeNewsfeedClicked;
            mvvm.SetAllSeenClicked += Mvvm_SetAllSeenClicked;

            if (mvvm.NewsfeedThreadsContainer != null)
            {
                mvvm.NewsfeedThreads = new System.Collections.ObjectModel.ObservableCollection<FriendicaThread>();
                try
                {
                    foreach (var thread in mvvm.NewsfeedThreadsContainer)
                    {
                        // TODO: so geht's, aber es werden nur mehr neuere danach geladen...?
                        if (mvvm.NewsfeedThreadsContainer.IndexOf(thread) > 120)
                            continue;

                        mvvm.NewsfeedThreads.Add(thread);
                        listviewNewsfeed.UpdateLayout();
                    }
                }
                catch
                {
                }
                mvvm.UpdateCounters();
                mvvm.NewsfeedThreadsContainer = null;
            }

            await mvvm.LoadInitial();
        }


        private void Mvvm_SetAllSeenClicked(object sender, EventArgs e)
        {
            // clear all live tiles after user has clicked on SetAllSeen in Newsfeed.xaml
            StaticLiveTileHelper.ClearNotifications();
        }


        private async void Mvvm_LikeNewsfeedClicked(object sender, System.EventArgs e)
        {
            // trigger showing an indicator that app is posting the retweet
            App.IsSendingNewPost = true;

            // prepare the retweet
            var postOriginal = sender as PCL.Models.FriendicaPost;
            var newPost = new NewPostViewmodel()
            {
                newPost = new Models.FriendicaNewPost(),
                RetweetPost = sender as PCL.Models.FriendicaPost
            };
            newPost.GenerateRetweetContent();
            newPost.NewPostStatus = newPost.RetweetedContent;
            newPost.newPost.NewPostSource = "Friendica";
            newPost.newPost.NewPostNetwork = "dfrn";

            // send the retweet to server
            Models.FriendicaPostExtended post = await newPost.RetweetNewsfeedItem();

            // when successfull, place the like or dislike on the newly created item
            if (post != null)
            {
                if (postOriginal.IntendedActivity == FriendicaActivity.like)
                    post.UpdateActivityOnServerRetweet(HttpRequests.PostFriendicaActivities.FriendicaActivity.like);
                else if (postOriginal.IntendedActivity == FriendicaActivity.dislike)
                    post.UpdateActivityOnServerRetweet(HttpRequests.PostFriendicaActivities.FriendicaActivity.dislike);
            }

            // remove indicator as we have finished the post
            App.IsSendingNewPost = false;
        }

        private void Mvvm_ButtonRetweetClicked(object sender, System.EventArgs e)
        {
            // implement A0_NewPost
            Frame.Navigate(typeof(Views.A0_NewPost), sender);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            var mvvm = this.DataContext as PCL.Viewmodels.NetworkViewmodel;
            mvvm.ButtonRetweetClicked -= Mvvm_ButtonRetweetClicked;
            mvvm.LikeNewsfeedClicked -= Mvvm_LikeNewsfeedClicked;

            // remove entries from NewsfeedThreads when navigating away, otherwise we will get a layout loop when returning 
            mvvm.NewsfeedThreadsContainer = new System.Collections.Generic.List<FriendicaThread>();
            foreach (var thread in mvvm.NewsfeedThreads)
                mvvm.NewsfeedThreadsContainer.Add(thread);
            mvvm.NewsfeedThreads = null;
        }


        private async void ViewerNewsfeed_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            // set the button for scrolling back to the top of the page to visible after leaving the first third of the visible area
            if (viewerNewsfeed.VerticalOffset > viewerNewsfeed.ActualHeight / 3)
                buttonScrollToTop.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                buttonScrollToTop.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // check if we are reaching the last 10% of the total extent --> trigger to start loading new entries
            var atBottom = viewerNewsfeed.VerticalOffset > (viewerNewsfeed.ScrollableHeight - viewerNewsfeed.ActualHeight);

            // start loading the next tranch of entries
            if (atBottom)
            {
                var mvvm = this.DataContext as PCL.Viewmodels.NetworkViewmodel;
                if (mvvm.NewsfeedThreads != null && mvvm.NewsfeedThreads.Count > 0 && !mvvm.IsLoadingNext)
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
            var zoom = viewerNewsfeed.ZoomFactor;
            viewerNewsfeed.ChangeView(0, 0, zoom);
        }


        private void listviewNewsfeed_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var listview = sender as ListView;
            listview.UpdateLayout();
        }
    }
}