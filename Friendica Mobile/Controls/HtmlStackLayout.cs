using System.Collections.Generic;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    /// <summary>
    /// encapsulate Xamarin.Forms.Button without any changes into a different class which enables 
    /// removing the border on macos, see FramelessButtonRenderer in macOS plattform project
    /// </summary>
    public class HtmlStackLayout : StackLayout
    {
        // prepare Bindable Property for HtmlContent 
        public static readonly BindableProperty HtmlContentProperty = BindableProperty.Create("HtmlContent",
                                                            typeof(List<View>), typeof(HtmlStackLayout),
                                                            null, BindingMode.OneWay, 
                                                            propertyChanged: (bindable, value, newValue) =>
                                                            {
                                                                (bindable as HtmlStackLayout).Children.Clear();
                                                                foreach (var item in newValue as List<View>)
                                                                    (bindable as HtmlStackLayout).Children.Add(item);
                                                            });                                

        public string HtmlContent
        {
            get { return (string)GetValue(HtmlContentProperty); }
            set { SetValue(HtmlContentProperty, value); }
        }

    }
}
