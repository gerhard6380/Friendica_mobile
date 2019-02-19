using Friendica_Mobile.UWP.Mvvm;
using System;
using Windows.UI.Xaml.Data;

namespace Friendica_Mobile.UWP.Converters
{
    public class EnumDisplayConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.GetType() == typeof(System.Double))
            {
                return null;
            }

            var text = (string)value;
            var _parameter = text.Split(new char[] { '|' });


            if (_parameter[0] == "FriendicaGender")
            {
                var _enumViewmodel = new FriendicaEnumGenderViewmodel();
                return _enumViewmodel.GetDisplayValue(_parameter[1].ToString());
            }
            else if (_parameter[0] == "FriendicaMarital")
            {
                var _enumViewmodel = new FriendicaEnumMaritalViewmodel();
                return _enumViewmodel.GetDisplayValue(_parameter[1].ToString());
            }
            else if (_parameter[0] == "Country")
            {
                var _enumViewmodel = new FriendicaEnumCountryViewmodel();
                return _enumViewmodel.GetDisplayValue(_parameter[1].ToString());
            }
            else if (_parameter[0] == "Region")
            {
                if ((string)parameter == "Field2")
                {
                    var _enumViewmodel = new FriendicaEnumRegionViewmodel(_parameter[1]);
                    return _enumViewmodel.GetDisplayValue(_parameter[2].ToString());
                }
                else
                {
                    var _enumViewmodel = new FriendicaEnumCountryViewmodel();
                    return _enumViewmodel.GetDisplayValue(_parameter[1].ToString());
                }
            }
            else if (_parameter[0] == "FriendicaSexual")
            {
                var _enumViewmodel = new FriendicaEnumSexualViewmodel();
                return _enumViewmodel.GetDisplayValue(_parameter[1].ToString());
            }
            // todo: add other classes
            else
                return null;


        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;
            else
            {
                return ((string)parameter == "Field2" ? "Region|" : "Country|") + value.ToString();
            }
        }

    }
}
