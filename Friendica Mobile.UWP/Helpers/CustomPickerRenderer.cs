using Friendica_Mobile.UWP;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
namespace Friendica_Mobile.UWP
{
    public class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (this.Control != null)
            {
                e.NewElement.PropertyChanged += NewElement_PropertyChanged;
                SetStyle(e.NewElement as Picker);
            }
        }

        private void NewElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Control != null)
                SetStyle(sender as Picker);
        }

        void SetStyle(Picker picker)
        {
            // remove text header because we will need to place a separate header label for all other platforms
            if (Control != null)
                Control.Header = "";

            // needed to avoid light style when temporary have crazy values with all -1
            if (picker.BackgroundColor.A == -1)
                return; 

            // set theme as per setting in the app
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
