using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    /// <summary>
    /// encapsulate Xamarin.Forms.Button without any changes into a different class which enables 
    /// removing the border on macos, see FramelessButtonRenderer in macOS plattform project
    /// </summary>
    public class FramelessButton : Button
    {
        // prepare Bindable Property for Tooltip 
        public static readonly BindableProperty TooltipProperty = BindableProperty.Create("Tooltip",
                                                            typeof(string), typeof(IconButton),
                                                            "", BindingMode.OneWay);

        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

    }
}
