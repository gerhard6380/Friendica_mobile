using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Friendica_Mobile.Converters
{
    class MessagesNewToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var paramString = parameter as string;
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Styles/MainStyles.xaml", UriKind.RelativeOrAbsolute);
            var accentBrush = (Brush)myResourceDictionary["AccentBrush"];
            var defaultButtonBrush = (Brush)myResourceDictionary["ButtonBorderThemeBrush"];
            var defaultBrush = (Brush)myResourceDictionary["SystemControlForegroundBaseHighBrush"];

            if ((bool)value)
                return accentBrush;
            else
            {
                if (paramString == "button")
                    return defaultButtonBrush;
                else
                    return defaultBrush;
            }
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
