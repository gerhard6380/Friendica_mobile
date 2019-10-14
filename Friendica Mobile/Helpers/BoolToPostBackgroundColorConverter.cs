using System;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    public class BoolToPostBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color color;
            try
            {
                color = (Color)Application.Current.Resources["AccentColor"];
            }
            catch
            {
                App.DefineResources();
                color = (Color)Application.Current.Resources["AccentColor"];
            }
            return ((bool)value) ? color.MultiplyAlpha(0.75) : color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value;
        }
    }
}
