using SeeberXamarin.Controls;
using Friendica_Mobile.UWP;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomActivityIndicator), typeof(CustomActivityIndicatorRenderer))]
namespace Friendica_Mobile.UWP
{
    public class CustomActivityIndicatorRenderer : ViewRenderer<CustomActivityIndicator, ProgressRing>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<CustomActivityIndicator> e)
        {
            base.OnElementChanged(e);

            if (Control != null || e.NewElement == null)
				return;

            e.NewElement.PropertyChanged += NewElement_PropertyChanged;
            SetProgressRing();
        }

        private void NewElement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SetProgressRing();
        }

        private void SetProgressRing()
        {
            var progressRing = new ProgressRing
            {
                IsActive = true,
                Visibility = Windows.UI.Xaml.Visibility.Visible,
                IsEnabled = true,
                Foreground = new SolidColorBrush(Windows.UI.Colors.White)
            };
            SetNativeControl(progressRing);
        }
    }
}