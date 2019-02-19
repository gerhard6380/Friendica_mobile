using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class NetworkModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var parent = App.GetNameOfCurrentView();
            bool chronologicalMode;
            if (parent == "Home")
                chronologicalMode = App.IsVisibleChronologicalHome;
            else
                chronologicalMode = App.IsVisibleChronological;

            // don't show the elements if we are in NewPost view
            if (parent == "A0_NewPost" || parent == "A1_ShowThread")
                return Visibility.Collapsed;
            else
            {
                if (chronologicalMode)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
