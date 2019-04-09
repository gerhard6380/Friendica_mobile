using System.ComponentModel;
using AppKit;
using Friendica_Mobile.macOS.Helpers;
using Friendica_Mobile.Styles;
using SeeberXamarin.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace Friendica_Mobile.macOS.Helpers
{
    
    public class CustomEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            e.NewElement.PropertyChanged += NewElement_PropertyChanged;
            SetColors(e.NewElement);
        }

        void NewElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Control != null && e.PropertyName == "IsEnabled")
            {
                var entry = sender as Entry;
                SetColors(entry); 
            }
        }


        void SetColors(Entry entry)
        {
            // override with black text color if we have a light theme background because user could have probably
            // dark system theme leading to white as primary text color vanishing the inputs
            // disabled input automatically makes black text visible in gray
            if (entry.BackgroundColor == Color.White || entry.BackgroundColor == Color.WhiteSmoke)
            {
                Control.TextColor = NSColor.Black;
                Control.BackgroundColor = (entry.IsEnabled) ? NSColor.White : NSColor.FromRgb(245, 245, 245);
                //Control.Bordered = true;
            }
            else
            {
                //Control.Bordered = false;
                Control.BackgroundColor = (entry.IsEnabled) ? NSColor.FromRgb(153, 153, 153) : NSColor.FromRgb(51, 51, 51);
            }
        }
    }
}
