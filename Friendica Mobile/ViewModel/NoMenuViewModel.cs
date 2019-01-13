using System.Windows.Input;
using Friendica_Mobile.Styles;
using Xamarin.Forms;
using Friendica_Mobile.Views;

namespace Friendica_Mobile.ViewModel
{
    // manages commands to be executed from menu entries defined in Shell.xaml
    public class NoMenuViewModel : BaseViewModel
    {
        private string _test;
        public string Test
        {
            get { return _test; }
            set
            {
                _test = value;
                OnPropertyChanged("Test");
            }
        }

        private ICommand _toDetail1Command;
        public ICommand ToDetail1Command => _toDetail1Command ?? (_toDetail1Command = new Command(ToDetail1, CanToDetail1));

        private bool CanToDetail1()
        {
            return true;
        }

        private void ToDetail1()
        {
            NavigateTo(new Detail3());
        }

        public NoMenuViewModel()
        {
            Test = "TEST 1";
        }
  
    }
}