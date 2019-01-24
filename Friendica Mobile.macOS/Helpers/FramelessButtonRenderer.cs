using System.ComponentModel;
using AppKit;
using Friendica_Mobile.macOS;
using Friendica_Mobile.Styles;
using SeeberXamarin.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(FramelessButton), typeof(FramelessButtonRenderer))]
namespace Friendica_Mobile.macOS
{

    public class FramelessButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            Control.Bordered = false;
        }

    }
}
