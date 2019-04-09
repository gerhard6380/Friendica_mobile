using Friendica_Mobile.UWP;
using SeeberXamarin.Controls;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(FramelessButton), typeof(FramelessButtonRenderer))]
namespace Friendica_Mobile.UWP
{

    public class FramelessButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            // implement tooltip shown if user places mouse cursor over button
            var tooltip = (e.NewElement as FramelessButton).Tooltip;
            if (tooltip != null && tooltip != "")
            {
                ToolTipService.SetToolTip(Control, new ToolTip { Content = tooltip });
            }
        }

    }
}
