using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class MobileDeviceOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object paramter, string language)
        {
            if (value as string == "Mobile")
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type taretValue, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
