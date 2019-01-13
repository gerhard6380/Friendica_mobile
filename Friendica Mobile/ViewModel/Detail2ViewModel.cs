using System.Threading.Tasks;
using System.Windows.Input;
using Friendica_Mobile.Views;
using Friendica_Mobile.Strings;
using Xamarin.Forms;

namespace Friendica_Mobile.ViewModel
{
    public class Detail2ViewModel : BaseViewModel
    {
        private bool _testEnabled = true;
        public bool TestEnabled
        {
            get { return _testEnabled; }
            set { SetProperty(ref _testEnabled, value); }
        }

        private int _settingsNewCounter;
        public int SettingsNewCounter
        {
            get { return _settingsNewCounter; }
            set { SetProperty(ref _settingsNewCounter, value); }
        }


        private ICommand _testCommand;
        public ICommand TestCommand => _testCommand ?? (_testCommand = new Command(Test));

        private void Test()
        {
            TestEnabled = !TestEnabled;
            //NavigateTo(new Detail3());
        }



        public Detail2ViewModel()
        {
            Title = "Detail2";
        }

    }
}
