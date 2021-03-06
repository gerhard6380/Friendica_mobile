﻿using System;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    class NetworkModeToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value as string == "Chronological")
                return 0;
            if (value as string == "Threads")
                return 1;
            else
                throw new ArgumentOutOfRangeException();
        }

        public object ConvertBack(object value, Type targetValue, object parameter, string language)
        {
            if ((int)value == 0)
                return "Chronological";
            else if ((int)value == 1)
                return "Threads";
            else
                throw new ArgumentOutOfRangeException();
        }
    }
}
