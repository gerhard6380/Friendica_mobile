using System;
using Plugin.MediaManager;
using Xamarin.Forms;

namespace Friendica_Mobile.Views
{
    public partial class MediaFullscreen : ContentPage
    { 
        public MediaFullscreen(string mediaurl, TimeSpan currentPosition)
        {
            InitializeComponent();

            // disable status bar to get the full space
            DependencyService.Get<IStatusBar>().HideStatusBar();

            // clear the queue and add the url again as otherwise the playback is not working
            CrossMediaManager.Current.MediaQueue.Clear();
            MediaPlayer.MediaUrl = mediaurl;

            // jumping to the previous position is not working, playback starts at beginning again
            // this is buggy at all, user needs to hit pause and play to start the fullscreen playback
            //CrossMediaManager.Current.MediaFileChanged += (sender, e) =>
            //{
            //    CrossMediaManager.Current.Seek(currentPosition);
            //};
        }

        protected override void OnDisappearing()
        {
            // on returning we can display the status bar again
            DependencyService.Get<IStatusBar>().ShowStatusBar();
        }
    }
}
