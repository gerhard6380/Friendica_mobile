using System;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    public class IntToColorCounterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((int)value > 0) ?  Color.Red : Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value;
        }
    }
}
