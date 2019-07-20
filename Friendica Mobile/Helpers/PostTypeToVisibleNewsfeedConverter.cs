using System;
using Friendica_Mobile.PCL.Viewmodels;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    /// <summary>
    /// we have some buttons in FriendicaPosts which do not make sense on newsfeeds - show Profile (only showing the RSS XML) 
    /// and AddComment (not useful as no other person could see them, user must use retweet for this)
    /// so we return true (IsVisible) only if user generated content is displayed in FriendicaPost
    /// </summary>
    public class PostTypeToVisibleNewsfeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((PostTypes)value == PostTypes.UserGenerated);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
