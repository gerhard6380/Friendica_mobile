using System;
using Xamarin.Forms;

namespace Friendica_Mobile
{
    public class HtmlToStackLayoutConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var html = new HtmlToXamarinViews((string)value);
            return html.ApplyHtmlToXamarinViews();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value;
        }
    }
}
