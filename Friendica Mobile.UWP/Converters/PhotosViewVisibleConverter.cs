using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using static Friendica_Mobile.UWP.Mvvm.PhotosViewmodel;

namespace Friendica_Mobile.UWP.Converters
{
    class PhotosViewVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var stringParameter = parameter as string;
            string[] views = stringParameter.Split(new char[] { '|' });

            foreach (var view in views)
            {
                var viewLower = view.ToLower();
                if (viewLower == "fullmode" && (PhotosViewStates)value == PhotosViewStates.Fullmode)
                    return Visibility.Visible;
                else if (viewLower == "onlyalbums" && (PhotosViewStates)value == PhotosViewStates.OnlyAlbums)
                    return Visibility.Visible;
                else if (viewLower == "onlyphotos" && (PhotosViewStates)value == PhotosViewStates.OnlyPhotos)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
