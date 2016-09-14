using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    // converting bool from toggleswitch to storable text for AppSettings class
    class ToFlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value as string == "RightToLeft")
                return FlowDirection.RightToLeft;
            else
                return FlowDirection.LeftToRight;
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}
