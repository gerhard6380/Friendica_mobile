using System;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class BoolStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value as string == "true")
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            if ((bool)value)
                return "true";
            else
                return "false";
        }
    }
}
