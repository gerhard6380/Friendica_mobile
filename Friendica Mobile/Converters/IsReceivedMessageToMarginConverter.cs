using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class IsReceivedMessageToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((string)parameter == "DateText")
            {
                // value = IsReceivedMessage
                if ((bool)value)
                    return new Thickness(0, 0, 4, 4);
                else
                    return new Thickness(0, 0, -20, 4);
            }
            else if ((string)parameter == "Content")
            {
                if ((bool)value)
                    return new Thickness(8, 0, 4, 0);
                else
                    return new Thickness(8, 0, -20, 0);
            }
            else
            {
                // if no parameter then we place the colored background, value = IsReceivedMessage
                if ((bool)value)
                    return new Thickness(48, 0, 0, 0);
                else
                    return new Thickness(0, 0, 64, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
