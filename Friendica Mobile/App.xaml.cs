using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Friendica_Mobile.Views;
using Friendica_Mobile.ViewModel;
using Friendica_Mobile.Styles;
using Plugin.DeviceInfo;
using Plugin.DeviceInfo.Abstractions;
using Friendica_Mobile.Strings;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Friendica_Mobile
{
    public partial class App : Application
    {
        public static HttpRequests.HttpFriendicaContacts Contacts;

        // selectable themes
        public enum ApplicationTheme { Dark, Light }

        // property to store the current device type
        public static Idiom DeviceType;

        // property to store the theme using a interface as we want to query system on UWP, macos
        private static ApplicationTheme _selectedTheme;
        public static ApplicationTheme SelectedTheme
        {
            get { return _selectedTheme; }
            set { _selectedTheme = value; }
        }

        // save current width/height to be triggered from customnavigationbutton/customnavigationcounter
        private static double _shellWidth;
        public static double ShellWidth
        { 
            get { return _shellWidth; }
            set
            {
                _shellWidth = value;;
            }
        }
        private static double _shellHeight;
        public static double ShellHeight
        { 
            get { return _shellHeight; }
            set
            {
                _shellHeight = value;
                ShellSizeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static event EventHandler ShellSizeChanged;

        public App()
        {
            // get the current system theme from the OS
            Settings.AppThemeModeChanged += (sender, e) => {
                SelectedTheme = DependencyService.Get<IGetSystemTheme>().GetSelectedTheme();
                DefineResources();
            };
            SelectedTheme = DependencyService.Get<IGetSystemTheme>().GetSelectedTheme();
            DefineResources();

            InitializeComponent();

            // get type of device as we want to react differently on phones, tablets or desktops
            DeviceType = CrossDeviceInfo.Current.Idiom;

            // set explicitely the ci on macos otherwise we have no correct localization on AppResource.xyz calls in c# code
            if (Device.RuntimePlatform == Device.macOS)
            {
                var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
                AppResources.Culture = ci;
            }

            MainPage = MasterDetailControl.Create<Views.CustomShell, ShellViewModel>();

            // navigate to first page
            // TODO: user can define which page should be the first one
            var nav = Application.Current.MainPage as NavigationPage;
            var shell = nav.RootPage as Views.CustomShell;
            var vm = shell.BindingContext as ShellViewModel;
            vm.Detail = new Views.About();
        }

        public static void DefineResources()
        {
            var isDark = (SelectedTheme == ApplicationTheme.Dark);
            Current.Resources["BackgroundColor"] = (isDark) ? Color.Black : Color.White;
            Current.Resources["MainTextColor"] = (isDark) ? Color.White : Color.Black;
            Current.Resources["NavigationBackgroundColor"] = (isDark) ? Color.FromHex("#201F1D") : Color.LightGray;
            Current.Resources["CommandBarBackgroundColor"] = (isDark) ? Color.FromHex("#282724") : Color.WhiteSmoke;
            Current.Resources["NavigationTextColor"] = (isDark) ? Color.White : Color.Black;
            Current.Resources["EntryBackgroundColor"] = (isDark) ? Color.FromHex("#999999") : Color.White;
            Current.Resources["EntryDisabledBackgroundColor"] = (isDark) ? Color.FromHex("#333333") : Color.WhiteSmoke;
            Current.Resources["ButtonBackgroundColor"] = (isDark) ? Color.FromHex("#333333") : Color.FromHex("#E6E6E6");
            if (Device.RuntimePlatform == Device.iOS)
                Current.Resources["ButtonTextColor"] = Color.FromHex("#007AFF");
            else
                Current.Resources["ButtonTextColor"] = (isDark) ? Color.White : Color.Black;
            Current.Resources["ListViewBackgroundColor"] = (isDark) ? Color.FromHex("#0D0D0D") : Color.WhiteSmoke;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
