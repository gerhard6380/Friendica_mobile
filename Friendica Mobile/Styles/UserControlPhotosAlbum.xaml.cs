using System;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friendica_Mobile.Styles
{
    public sealed partial class UserControlPhotosAlbum : UserControl
    {
        private Timer timer;

        public UserControlPhotosAlbum()
        {
            this.InitializeComponent();

            // randomize stapling of the 3 photos, avoids changing all albums in the same direction
            SetRandomStartpoint();

            // start timer with a random intervall between 3s and 5s (randomized to avoid that not every change occurs at the same time)
            var rnd = new Random();
            var intervall = rnd.Next(12, 21) * 250;
            timer = new Timer(RotateAlbumImages, null, intervall, intervall);
        }

        private void SetRandomStartpoint()
        {
            var rnd = new Random();
            var dice = rnd.Next(1, 7);
            switch (dice)
            {
                case 1:
                    Canvas.SetZIndex(borderPhoto1, 1);
                    Canvas.SetZIndex(borderPhoto2, 2);
                    Canvas.SetZIndex(borderPhoto3, 3);
                    break;
                case 2:
                    Canvas.SetZIndex(borderPhoto1, 1);
                    Canvas.SetZIndex(borderPhoto2, 3);
                    Canvas.SetZIndex(borderPhoto3, 2);
                    break;
                case 3:
                    Canvas.SetZIndex(borderPhoto1, 2);
                    Canvas.SetZIndex(borderPhoto2, 1);
                    Canvas.SetZIndex(borderPhoto3, 3);
                    break;
                case 4:
                    Canvas.SetZIndex(borderPhoto1, 2);
                    Canvas.SetZIndex(borderPhoto2, 3);
                    Canvas.SetZIndex(borderPhoto3, 1);
                    break;
                case 5:
                    Canvas.SetZIndex(borderPhoto1, 3);
                    Canvas.SetZIndex(borderPhoto2, 1);
                    Canvas.SetZIndex(borderPhoto3, 2);
                    break;
                default:
                    Canvas.SetZIndex(borderPhoto1, 3);
                    Canvas.SetZIndex(borderPhoto2, 2);
                    Canvas.SetZIndex(borderPhoto3, 1);
                    break;
            }
        }

        private async void RotateAlbumImages(object state)
        {

            await Dispatcher.TryRunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var index1 = Canvas.GetZIndex(borderPhoto1);
                var index2 = Canvas.GetZIndex(borderPhoto2);
                var index3 = Canvas.GetZIndex(borderPhoto3);

                Canvas.SetZIndex(borderPhoto1, index3);
                Canvas.SetZIndex(borderPhoto2, index1);
                Canvas.SetZIndex(borderPhoto3, index2);
            });
        }

        private void gridUserControl_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid.Height != grid.ActualWidth)
                grid.Height = grid.ActualWidth;
        }

        private void Image_ImageFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
        {

        }

        private void Image_ImageOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
