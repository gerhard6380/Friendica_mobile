using AppKit;
using Friendica_Mobile.macOS.Helpers;
using SeeberXamarin.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(FontIconLabel), typeof(FontIconLabelRenderer))]
namespace Friendica_Mobile.macOS.Helpers
{
    
    public class FontIconLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement == null)
            {
                Control.Bordered = false;
                Control.DrawsBackground = false;
                Control.Font = NSFont.FromFontName("Segoe MDL2 Assets", 20);
                Control.Selectable = false;
            }
        }
    }
}
