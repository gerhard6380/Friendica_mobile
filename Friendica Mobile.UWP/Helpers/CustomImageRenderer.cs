using Friendica_Mobile.UWP;
using SeeberXamarin.Controls;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(CustomImage), typeof(CustomImageRenderer))]
namespace Friendica_Mobile.UWP
{
    public class CustomImageRenderer : ImageRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Image> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // display alternative text from html image tag as a tooltip if user hovers over the image
                var element = Element as CustomImage;
                if (element.Tooltip != "errorimage")
                {
                    ToolTip toolTip = new ToolTip();
                    toolTip.Content = element.Tooltip;
                    ToolTipService.SetToolTip(Control, toolTip);
                }
            }
        }

    }
}