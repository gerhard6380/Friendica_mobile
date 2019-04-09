using System.ComponentModel;
using Android.Content;
using Friendica_Mobile.Droid.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace Friendica_Mobile.Droid.Helpers
{
    public class CustomEntryRenderer : EntryRenderer
    {
        public CustomEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;
            
            e.NewElement.PropertyChanged += NewElement_PropertyChanged;
            Control.SetTextColor(Element.IsEnabled ? Element.TextColor.ToAndroid() : Android.Graphics.Color.Gray);
        }

        void NewElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Control != null && e.PropertyName == "IsEnabled")
            {
                var entry = sender as Entry;
                Control.SetTextColor(Element.IsEnabled ? Element.TextColor.ToAndroid() : Android.Graphics.Color.Gray);
            }
        }
    }
}
