using SeeberXamarin.Controls;
using Friendica_Mobile.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomSwitch), typeof(CustomSwitchRenderer))]
namespace Friendica_Mobile.UWP
{
    public class CustomSwitchRenderer : SwitchRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // remove text "on" "off", because for Android and Ios we will add separate labels next to the switches
                // change the requested theme according to the defined app theme 
                Settings.AppThemeModeChanged += (sender, args) => { SetTheme(); };
                Control.OnContent = "";
                Control.OffContent = "";
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