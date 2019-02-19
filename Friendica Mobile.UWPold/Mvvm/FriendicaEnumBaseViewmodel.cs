using Friendica_Mobile.UWP.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.UWP.Mvvm
{
    public abstract class FriendicaEnumBaseViewmodel
    {
        private Type _type;
        public Type Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _emptyType;
        public string EmptyType
        {
            get { return _emptyType; }
            set { _emptyType = value; }
        }

        private string _unknownType;
        public string UnknownType
        {
            get { return _unknownType; }
            set { _unknownType = value; }
        }

        private string _parameter;
        public string Parameter
        {
            get { return _parameter; }
            set { _parameter = value; }
        }

        private IDictionary _displayValues;
        private IDictionary _reverseValues;

        public void PrepareDictionaries()
        {
            if (_type.Name == "FriendicaCountry")
            {
                _displayValues = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).GetGenericTypeDefinition().MakeGenericType(typeof(string), typeof(string)));
                _reverseValues = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).GetGenericTypeDefinition().MakeGenericType(typeof(string), typeof(string)));

                var allCountries = new FriendicaCountry();
                foreach (var country in allCountries.ListCountries)
                {
                    if (country == "")
                    {
                        _displayValues.Add(country, " ");
                        _reverseValues.Add(" ", country);
                    }
                    else
                    {
                        _displayValues.Add(country, country);
                        _reverseValues.Add(country, country);
                    }
                }
            }
            else if (_type.Name == "FriendicaEnumRegionViewmodel")
            {
                _displayValues = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).GetGenericTypeDefinition().MakeGenericType(typeof(string), typeof(string)));
                _reverseValues = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).GetGenericTypeDefinition().MakeGenericType(typeof(string), typeof(string)));

                var allCountries = new FriendicaCountry();
                var allStates = allCountries.GetStates(Parameter);

                foreach (var state in allStates)
                {
                    if (state == "")
                    {
                        _displayValues.Add(state, " ");
                        _reverseValues.Add(" ", state);
                    }
                    else
                    {
                        _displayValues.Add(state, state);
                        _reverseValues.Add(state, state);
                    }
                }
            }
            else
            {
                Type displayValuesType = typeof(Dictionary<,>).GetGenericTypeDefinition().MakeGenericType(_type, typeof(string));
                _displayValues = (IDictionary)Activator.CreateInstance(displayValuesType);
                _reverseValues = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).GetGenericTypeDefinition().MakeGenericType(typeof(string), _type));

                var fields = _type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    DisplayStringAttribute[] a = (DisplayStringAttribute[])field.GetCustomAttributes(typeof(DisplayStringAttribute), false);

                    string displayString = GetDisplayStringValue(a);
                    object enumValue = field.GetValue(null);

                    if (displayString != null)
                    {
                        _displayValues.Add(enumValue, displayString);
                        _reverseValues.Add(displayString, enumValue);
                    }
                }
            }
        }


        public ReadOnlyCollection<string> DisplayNames
        {
            get
            {
                return new List<string>((IEnumerable<string>)_displayValues.Values).AsReadOnly();
            }
        }

        private object GetEnumElement(string type)
        {
            foreach (DictionaryEntry value in _displayValues)
            {
                if (value.Key.ToString() == type)
                    return value.Key;
            }
            return null;
        }

        private string SearchAllLanguagesForEnumName(string searchstring)
        {
            var fields = _type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                DisplayStringAttribute[] a = (DisplayStringAttribute[])field.GetCustomAttributes(typeof(DisplayStringAttribute), false);
                if (a == null || a.Length == 0) return null;
                DisplayStringAttribute dsa = a[0];
                if (dsa.English == searchstring || dsa.French == searchstring ||
                    dsa.German == searchstring || dsa.Italian == searchstring ||
                    dsa.Portuguese == searchstring || dsa.Spanish == searchstring)
                    return field.Name;
            }
            return null;
        }

        public string GetDisplayValue(string searchstring)
        {
            if (searchstring == "" || searchstring == "0")
            {
                return _displayValues[GetEnumElement(EmptyType)].ToString();
            }
            else
            {
                var currentLang = _reverseValues[searchstring];
                if (currentLang != null)
                    return searchstring;
                else
                {
                    var searchAll = SearchAllLanguagesForEnumName(searchstring);
                    if (searchAll != null)
                        return _displayValues[GetEnumElement(searchAll)].ToString();
                    else
                        return _displayValues[GetEnumElement(UnknownType)].ToString();
                }
            }
        }

        public string GetReverseValue(string searchstring)
        {
            var test = _reverseValues[searchstring].ToString();
            return test;
        }


        private string GetDisplayStringValue(DisplayStringAttribute[] a)
        {
            if (a == null || a.Length == 0) return null;
            DisplayStringAttribute dsa = a[0];

            var language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            switch (language)
            {
                case "en":
                    return dsa.English;
                case "de":
                    return dsa.German;
                case "es":
                    return dsa.Spanish;
                case "fr":
                    return dsa.French;
                case "it":
                    return dsa.Italian;
                case "pt":
                    return dsa.Portuguese;
                default:
                    return dsa.Current;
            }
        }

        private object SearchForEnumElement(string databaseString)
        {
            //_type = typeof(FriendicaGenderValues);
            object enumValue = null;

            var fields = _type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                foreach (DisplayStringAttribute att in attributes)
                {
                    var list = att.ReturnListOfAllLanguages();
                    if (list.Contains(databaseString))
                    {
                        enumValue = field.GetValue(null);
                        break;
                    }
                }
            }
            return enumValue;
        }

    }
}
