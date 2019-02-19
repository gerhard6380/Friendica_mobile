using SeeberXamarin.Controls;
using Friendica_Mobile.UWP;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Slider), typeof(CustomSliderRenderer))]
namespace Friendica_Mobile.UWP
{
    public class CustomSliderRenderer : SliderRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Slider> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // remove text "on" "off", because for Android and Ios we will add separate labels next to the switches
                // change the requested theme according to the defined app theme 
                Settings.AppThemeModeChanged += (sender, args) => { SetTheme(); };
                SetTheme();
            }
        }


        private void SetTheme()
        {
            if (Control != null)
            {
                if (Friendica_Mobile.App.SelectedTheme == Friendica_Mobile.App.ApplicationTheme.Light)
                    Control.RequestedTheme = Windows.UI.Xaml.ElementTheme.Light;
                else
                    Control.RequestedTheme = Windows.UI.Xaml.ElementTheme.Dark;
            }
        }

    }
}