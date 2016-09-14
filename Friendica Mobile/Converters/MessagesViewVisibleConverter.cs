using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.Converters
{
    class MessagesViewVisibleConverter : IValueConverter
    {
        public enum MessagesViewStates { Fullmode, OnlyConversations, OnlyMessages };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var stringParameter = parameter as string;
            string[] views = stringParameter.Split(new char[] { '|' });

            foreach (var view in views)
            {
                if (view == "fullmode" && (MessagesViewStates)value == MessagesViewStates.Fullmode)
                    return Visibility.Visible;
                else if (view == "onlyconversations" && (MessagesViewStates)value == MessagesViewStates.OnlyConversations)
                    return Visibility.Visible;
                else if (view == "onlymessages" && (MessagesViewStates)value == MessagesViewStates.OnlyMessages)
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
