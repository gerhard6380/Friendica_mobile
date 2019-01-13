using System.Windows.Input;
using Friendica_Mobile.Styles;
using Xamarin.Forms;
using Friendica_Mobile.Views;

namespace Friendica_Mobile.ViewModel
{
    // manages commands to be executed from menu entries defined in Shell.xaml
    public class ShellViewModel : MasterDetailControlViewModel
    {
        // create commands for each button
        private ICommand _toDetail1;
        public ICommand ToDetail1
        {
            get { return _toDetail1 ?? (_toDetail1 = new Command(OnToDetail1)); }
        }
        private void OnToDetail1()
        {
            NavigateTo(new Detail1());
        }


        private ICommand _toNoMenuDetail;
        public ICommand ToNoMenuDetail
        {
            get { return _toNoMenuDetail ?? (_toNoMenuDetail = new Command(OnToNoMenuDetail)); }
        }
        private void OnToNoMenuDetail()
        {
            NavigateTo(new NoMenuDetail());
        }


        private ICommand _toDetail2;
        public ICommand ToDetail2
        {
            get { return _toDetail2 ?? (_toDetail2 = new Command(OnToDetail2)); }
        }
        private void OnToDetail2()
        {
            NavigateTo(new Detail2());
        }


        private ICommand _toDetail3;
        public ICommand ToDetail3
        {
            get { return _toDetail3 ?? (_toDetail3 = new Command(OnToDetail3)); }
        }
        private void OnToDetail3()
        {
            NavigateTo(new Detail3());
        }


        private ICommand _toSettingsCommand;
        public ICommand ToSettingsCommand
        {
            get { return _toSettingsCommand ?? (_toSettingsCommand = new Command(ToSettings)); }    
        }
        private void ToSettings()
        {
            NavigateTo(new Views.Settings());
        }


        // add a counter for each of the buttons where we want to display a counter of new elements
        private int _settingsCounter;
        public int SettingsCounter
        {
            get { return _settingsCounter; }
            set { SetProperty(ref _settingsCounter, value); }
        }


        private int _detail3Counter;
        public int Detail3Counter
        {
            get { return _detail3Counter; }
            set { SetProperty(ref _detail3Counter, value); }
        }
    }
}