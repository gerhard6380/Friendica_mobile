using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class GridWidthMessagesConverter : IValueConverter
    {
        public enum MessagesViewStates { Fullmode, OnlyConversations, OnlyMessages };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // correct by 48 for menu + 16 for scrollviewer
            if ((MessagesViewStates)value == MessagesViewStates.Fullmode)
                return (App.Settings.ShellWidth - 64) / 2;
            else
            {
                // increase value by another 48 pixels on mobiles in landscape mode
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobileLandscape)
                    return App.Settings.ShellWidth - 112;
                else
                    return App.Settings.ShellWidth - 64;
              }  
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
