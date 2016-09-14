using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class AddCommentButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var parent = App.GetNameOfCurrentView();
            // don't show the elements if we are in NewPost view
            if (parent == "A0_NewPost" || parent == "A1_ShowThread")
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
