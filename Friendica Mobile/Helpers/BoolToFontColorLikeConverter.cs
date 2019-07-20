using System;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    public class BoolToFontColorLikeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ?  Color.DeepSkyBlue : Color.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value;
        }
    }
}
