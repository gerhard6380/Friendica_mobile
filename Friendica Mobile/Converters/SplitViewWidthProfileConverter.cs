using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class SplitViewWidthProfileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((string)parameter == "Pivot")
                return (double)value - 48.0;

            double gridviewCorrection = 0.0;
            if ((string)parameter == "GridViewItem")
                gridviewCorrection = -40;

            double deviceCorrection;
            if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                deviceCorrection = 48.0;
            else
                deviceCorrection = 0.0;

            if ((double)value > 720 && (double)value != 0)
                return 300.0 + gridviewCorrection;
            else
            {
                var width = (double)value - 84.0 - deviceCorrection + gridviewCorrection;  // 84.0
                return (width <= 0) ? 0 : width;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
