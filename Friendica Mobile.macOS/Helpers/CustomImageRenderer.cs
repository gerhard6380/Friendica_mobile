using System.ComponentModel;
using AppKit;
using Friendica_Mobile.macOS;
using Friendica_Mobile.Styles;
using SeeberXamarin.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(CustomImage), typeof(CustomImageRenderer))]
namespace Friendica_Mobile.macOS
{
    
    public class CustomImageRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            var image = Element as CustomImage;
            if (image.Tooltip != "errorimage")
                Control.ToolTip = image.Tooltip;
        }

    }
}
