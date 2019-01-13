using System.Threading.Tasks;
using System.Windows.Input;
using Friendica_Mobile.Strings;
using Friendica_Mobile.Views;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace Friendica_Mobile.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        public bool IsDarkModeEnabled
        {
            get { return Settings.AppThemeDarkModeEnabled; }
            set
            {
                Settings.AppThemeDarkModeEnabled = value;
                OnPropertyChanged("LabelAppThemeMode");
            }
        }

        public string LabelAppThemeMode
        {
            get
            {
                return (Settings.AppThemeDarkModeEnabled) ? AppResources.LabelSettingsAppThemeDarkMode : AppResources.LabelSettingsAppThemeLightMode;
            }
        }

        public bool IsUsingSystemTheme
        {
            get { return Settings.AppThemeUseSystemTheme; }
            set { Settings.AppThemeUseSystemTheme = value; }
        }

        private bool _isNavigationSideVisible;
        public bool IsNavigationSideVisible
        {
            get { return _isNavigationSideVisible; }
            set { SetProperty(ref _isNavigationSideVisible, value); }
        }

        public bool IsNavigationOnRightSide
        {
            get { return Settings.NavigationOnRightSide; }
            set
            {
                Settings.NavigationOnRightSide = value;
                OnPropertyChanged("LabelNavigationSide");
            }
        }

        public string LabelNavigationSide
        {
            get { return (Settings.NavigationOnRightSide) ? AppResources.LabelSettingsNavigationSideRight : AppResources.LabelSettingsNavigationSideLeft; }
        }



        private ICommand _toDetail3Command;
        public ICommand ToDetail3Command => _toDetail3Command ?? (_toDetail3Command = new Command(ToDetail3));

        private void ToDetail3()
        {
            NavigateTo(new Detail3());
        }

        private ICommand _activateSpinningCommand;
        public ICommand ActivateSpinningCommand => _activateSpinningCommand ?? (_activateSpinningCommand = new Command(ActivateSpinning));

        private async void ActivateSpinning()
        {
            SetIsNavigationRunning(true);
            await Task.Delay(3000);
            SetIsNavigationRunning(false);
        }

        private ICommand _isNavigationAllowedCommand;
        public ICommand IsNavigationAllowedCommand => _isNavigationAllowedCommand ?? (_isNavigationAllowedCommand = new Command(NavigationAllowed));

        private void NavigationAllowed()
        {
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Shell;
            var vm = shell.BindingContext as ShellViewModel;

            SetNavigationAllowed(!vm.NavigationAllowed);
        }

        public SettingsViewModel()
        {
            Title = AppResources.PageAppSettings;
            // make setting for navigationside visible if we are using a phone device
            IsNavigationSideVisible = (App.DeviceType == DeviceIdiom.Phone);
        }

    }
}
