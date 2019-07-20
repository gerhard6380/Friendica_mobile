using System;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    public class BoolToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ?  (Color)Application.Current.Resources["MainTextColor"] : (Color)Application.Current.Resources["BackgroundColor"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value;
        }
    }
}
