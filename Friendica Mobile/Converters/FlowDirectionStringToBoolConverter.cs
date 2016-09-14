using System;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class FlowDirectionStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value as string == "RightToLeft")
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            if ((bool)value)
                return "RightToLeft";
            else
                return "LeftToRight";
        }
    }
}
