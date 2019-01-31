using AppKit;
using Friendica_Mobile.macOS;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Picker), typeof(CustomPickerRenderer))]
namespace Friendica_Mobile.macOS
{

    public class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            e.NewElement.PropertyChanged += NewElement_PropertyChanged;
            SetColors(e.NewElement);
        }

        void NewElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEnabled")
            {
                var picker = sender as Picker;
                SetColors(picker); 
            }
        }


        void SetColors(Picker picker)
        {
            // override with black text color if we have a light theme background because user could have probably
            // dark system theme leading to white as primary text color vanishing the inputs
            // disabled input automatically makes black text visible in gray
            if (picker.BackgroundColor == Color.White || picker.BackgroundColor == Color.WhiteSmoke)
            {
                Control.TextColor = NSColor.Black;
                Control.BackgroundColor = (picker.IsEnabled) ? NSColor.White : NSColor.FromRgb(245, 245, 245);;
                //Control.Bordered = true;
            }
            else
            {
                //Control.Bordered = false;
                Control.BackgroundColor = (picker.IsEnabled) ? NSColor.FromRgb(153, 153, 153) : NSColor.FromRgb(51, 51, 51);
            }
        }
    }
}
