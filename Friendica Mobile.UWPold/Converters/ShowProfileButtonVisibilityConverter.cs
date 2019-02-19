using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class ShowProfileButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var parent = App.GetNameOfCurrentView();
            // don't show the elements if we are in NewPost view
            // don't show profile page element in home, as postings are mostly from user itself and comments from his/her friends
            if (parent == "A0_NewPost" || parent == "Home"|| parent == "Newsfeed")
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
