using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class BoolToNewEntryThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
                return new Thickness(4);
            else
                return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
