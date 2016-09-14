using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class ListViewWidthMessagesConverter : IValueConverter
    {
        public enum MessagesViewStates { Fullmode, OnlyConversations, OnlyMessages };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (double)value - 16;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
