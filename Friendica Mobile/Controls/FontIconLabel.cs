using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    public class FontIconLabel : Label
    {
        public static readonly string FontIconName = "Segoe MDL2 Assets";

        public FontIconLabel()
        {
            FontFamily = FontIconName;
        }

        public FontIconLabel(string fontIconLabel = null)
        {
            FontFamily = FontIconName;
            Text = fontIconLabel;
        }
    }
}
