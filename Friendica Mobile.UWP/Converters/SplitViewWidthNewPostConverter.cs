using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class SplitViewWidthNewPostConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter != null)
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                    return (double)value - 132.0;
                else if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobilePortrait)
                    return (double)value - 56.0; // 64.0
                else if (App.Settings.OrientationDevice == OrientationDeviceFamily.DesktopPortrait)
                    return (double)value - 72.0;
                else
                {
                    double width = Double.Parse((string)parameter);
                    if (width < 1120)
                    {
                        var ratio = width / 1120;
                        return (double)value * ratio - 72.0;
                    }
                    else
                        return width - 72.0;
                }
            }
            else
            {
                if ((double)value != 0)
                {
                    if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                        return (double)value - 56.0; // 60.0
                    else
                        return (double)value - 56.0;  // 60.0
                }
                else
                    return 800.0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
