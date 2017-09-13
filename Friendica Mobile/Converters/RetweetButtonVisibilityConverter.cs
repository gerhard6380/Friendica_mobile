using Friendica_Mobile.PCL.Viewmodels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class RetweetButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var parent = App.GetNameOfCurrentView();
            // don't show the retweet button if we are not on Newsfeed page
            // means that we don't want to see the button on NewEntry or ShowThread
            // in future we will offer the retweet option on Network as well, probably not all posts should get this (retweeting own posts useful?)
            if ((PostTypes)value == PostTypes.Newsfeed && parent == "Newsfeed")
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
