using Friendica_Mobile.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Entry), typeof(CustomEntryRenderer))]
namespace Friendica_Mobile.UWP
{
    public class CustomEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (this.Control != null)
            {
                e.NewElement.PropertyChanged += NewElement_PropertyChanged;
                SetStyle(e.NewElement as Entry);
            }
        }

        private void NewElement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Control != null)
                SetStyle(sender as Entry);
        }

        void SetStyle(Entry entry)
        {
            // needed to avoid light style when temporary have crazy values with all -1
            if (entry.BackgroundColor.A == -1)
                return; 

            // if design is either on dark theme enabled or on dark theme disabled
            var isDark = (entry.BackgroundColor == Color.FromRgb(51, 51, 51) || entry.BackgroundColor == Color.FromRgb(153,153, 153));

            var test = Windows.UI.Xaml.Application.Current.Resources["DefaultTextBoxDarkStyle"];

            if (isDark)
                this.Control.Style = Windows.UI.Xaml.Application.Current.Resources["DefaultTextBoxDarkStyle"] as Windows.UI.Xaml.Style;
            else
                this.Control.Style = Windows.UI.Xaml.Application.Current.Resources["DefaultTextBoxStyle"] as Windows.UI.Xaml.Style;
        }
    }
}
