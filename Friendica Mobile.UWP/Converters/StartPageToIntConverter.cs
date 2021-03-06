﻿using System;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class StartPageToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value as string == "Home")
                return 0;
            if (value as string == "Network")
                return 1;
            if (value as string == "Newsfeed")
                return 2;
            else
                throw new ArgumentOutOfRangeException();
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            if ((int)value == 0)
                return "Home";
            else if ((int)value == 1)
                return "Network";
            else if ((int)value == 2)
                return "Newsfeed";
            else
                throw new ArgumentOutOfRangeException();
        }
    }
}
