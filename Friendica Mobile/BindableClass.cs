using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Friendica_Mobile
{
    public abstract class BindableClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // function to convert <x><y><z> into a list of double values
        public List<double> ConvertContactListStringToList(string contacts)
        {
            if (contacts == null)
                return null;

            var list = new List<double>();
            if (contacts == "")
                return list;

            string[] values = Regex.Split(contacts, @"\<([^<]+)\>").Where(s => !string.IsNullOrEmpty(s)).ToArray();
            foreach (var value in values)
                list.Add(Convert.ToDouble(value));
            return list;
        }
    }
}
