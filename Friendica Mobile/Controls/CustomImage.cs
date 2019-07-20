using System;
using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    /// <summary>
    /// used for labels showing different hints for different value states
    /// enum for different hints can be bound to this propoerty, triggers in xaml load different strings depending on the enum value
    /// </summary>
    public class CustomImage : Image
    {
        // prepare Bindable Property for the type of hint label 
        // type must be string, because at this stage we do not know which enum is used for the type
        public static readonly BindableProperty TooltipProperty = BindableProperty.Create("Tooltip",
                                                            typeof(string), typeof(CustomImage),
                                                            "", BindingMode.OneWay);

        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
    }
}
