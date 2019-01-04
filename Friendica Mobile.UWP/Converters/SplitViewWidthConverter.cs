using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class SplitViewWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((double)value < 884 && (double)value != 0)
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                    return (double)value - 132.0;
                else
                {
                    if (App.GetNameOfCurrentView() == "A0_NewPost")
                        return (double)value - 32.0;
                    else
                        return (double)value - 84.0;
                }
            }
            else
            {
                if (App.GetNameOfCurrentView() == "A0_NewPost")
                    return (double)value;
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
