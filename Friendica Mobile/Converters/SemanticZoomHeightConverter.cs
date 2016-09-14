using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class SemanticZoomHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double groupCorrection = 0.0;

            if (parameter != null)
                groupCorrection = System.Convert.ToDouble(parameter);

            double noSettingsCorrection = 0.0;
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                        || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
                noSettingsCorrection = 72;
            else
                noSettingsCorrection = 0;

            double correction = 0.0;
            if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                correction = 144.0;
            else if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobilePortrait)
            {
                //if (App.Settings.IsTrial)
                if (!App.Settings.PaidForRemovingAds)
                    correction = 312.0;
                else
                    correction = 216.0;
            }
            else if (App.Settings.OrientationDevice == OrientationDeviceFamily.DesktopPortrait)
            {
                //if (App.Settings.IsTrial)
                if (!App.Settings.PaidForRemovingAds)
                    correction = 244.0;
                else
                    correction = 192.0;
            }
            else
                correction = 192.0;

            double reducedValue = 0.0;
            if ((correction + noSettingsCorrection + groupCorrection) < (double)value)
                reducedValue = (double)value - correction - noSettingsCorrection - groupCorrection;
            return reducedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
