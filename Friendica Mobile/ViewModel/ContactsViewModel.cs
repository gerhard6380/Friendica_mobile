using System.Threading.Tasks;
using System.Windows.Input;
using Friendica_Mobile.Views;
using Friendica_Mobile.Strings;
using Xamarin.Forms;
using Friendica_Mobile.HttpRequests;
using System;

namespace Friendica_Mobile.ViewModel
{
    public class ContactsViewModel : BaseViewModel
    {
        //public string TestValue
        //{
        //    get { return App.Contacts.HttpTestValue; }
        //    set { App.Contacts.HttpTestValue = value; }
        //}

        private ICommand _setValueCommand;
        public ICommand SetValueCommand => _setValueCommand ?? (_setValueCommand = new Command(SetValue));
        private void SetValue()
        {
            //TestValue = "ContactsViewModel";
        }

        private ICommand _showSettingCommand;
        public ICommand ShowSettingCommand => _showSettingCommand ?? (_showSettingCommand = new Command(ShowSetting));
        private async void ShowSetting()
        {
            //await Application.Current.MainPage.DisplayAlert("TestValue", TestValue, "OK");
        }

        public ContactsViewModel()
        {
            if (App.Contacts == null)
                App.Contacts = new HttpFriendicaContacts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
        }




    }
}
