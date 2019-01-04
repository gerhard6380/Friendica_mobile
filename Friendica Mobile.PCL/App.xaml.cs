using Friendica_Mobile.PCL.Strings;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Friendica_Mobile.PCL
{
    public partial class App : Application
    {
        // selectable themes
        public enum ApplicationTheme { Dark, Light }

        // store if the theme is dark or light
        private ApplicationTheme _selectedTheme;
        public ApplicationTheme SelectedTheme
        {
            get { return _selectedTheme; }
            set { _selectedTheme = value;
                SelectedThemeChanged?.Invoke("SelectedTheme", EventArgs.Empty);
            }
        }
        public event EventHandler SelectedThemeChanged;

        public App(OpenUrlData urlData = null)
        {
            // get locales for translated strings (UWP has built-in recognition)
            if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android)
            {
                SelectedTheme = ApplicationTheme.Light; // Android and iOS are always using light theme
                var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
                AppResources.Culture = ci; // set the RESX for resource localization
                DependencyService.Get<ILocalize>().SetLocale(ci); // set the Thread for locale-aware methods
            }

            InitializeComponent();

            // TODO: use this block to jump directly into a specific page
            //var loginPage = new NavigationPage(new EditUserPage());
            //loginPage.BarBackgroundColor = Color.Transparent;
            //loginPage.BarTextColor = Color.Transparent;
            //MainPage = loginPage;

            // TODO: go back to correct navigation
            var shell = new NavigationPage(new Views.Settings());
            shell.BarBackgroundColor = Color.Transparent;
            shell.BarTextColor = Color.Transparent;
            MainPage = shell;
        }

        public async void LoadUrl(OpenUrlData urlData)
        {
            if (urlData != null)
            {
                if (urlData.Target == OpenUrlData.OpenPageType.LoginPageRegistryToken)
                {
                    //// we will presetting the username and token from urldata
                    //var loginPage = new LoginPage();
                    //var vm = loginPage.BindingContext as LoginViewModel;
                    //urlData.Parameters.TryGetValue("username", out string username);
                    //urlData.Parameters.TryGetValue("token", out string token);
                    //vm.Username = username;
                    //vm.RegisterToken = token;
                    //// trigger the dialog to user if he wants to set back the app
                    //if (!vm.IsFirstStart)
                    //{
                    //    vm.PageLoadedFromUrl = true;
                    //    var unregistered = await vm.CheckIfAlreadyRegistered();
                    //    if (unregistered)
                    //    {
                    //        vm.Username = username;
                    //        vm.RegisterToken = token;
                    //    }
                    //}

                    //var navPage = new NavigationPage(loginPage);
                    //navPage.BarBackgroundColor = Color.Transparent;
                    //navPage.BarTextColor = Color.Transparent;
                    //MainPage = navPage;
                }
            }
        }
    }
}