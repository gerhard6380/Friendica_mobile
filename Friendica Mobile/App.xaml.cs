using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Friendica_Mobile.ViewModel;
using Friendica_Mobile.Styles;
using Plugin.DeviceInfo;
using Plugin.DeviceInfo.Abstractions;
using Friendica_Mobile.Strings;
using System.IO;
using Friendica_Mobile.Models;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Friendica_Mobile
{
    public partial class App : Application
    {
        // property for holding internal notification
        private static InternalNotification _notification;
        public static InternalNotification Notification
        {
            get { return _notification; }
            set { _notification = value;
                NotificationChanged?.Invoke(null, EventArgs.Empty); }
        }

        public static Models.PostsModel Posts;
        public static HttpRequests.HttpFriendicaContacts Contacts;
        public static Models.NewPostsAdmin NewPosts;

        // local storing when message has been shown once to the user
        public static bool NetworkNoSettingsAlreadyShownRefresh;
        public static bool HomeNoSettingsAlreadyShownRefresh;
        public static bool LikeDislikeInTestModeInfoAlreadyShown;

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
        public static event EventHandler NotificationChanged; //fired when a new internal notification should be displayed


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
            vm.Detail = new Views.Network();
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
            Current.Resources["EntryDisabledBackgroundColor"] = (isDark) ? Color.FromHex("#333333") : Color.LightGray;
            Current.Resources["ButtonBackgroundColor"] = (isDark) ? Color.FromHex("#333333") : Color.FromHex("#E6E6E6");
            if (Device.RuntimePlatform == Device.iOS)
                Current.Resources["ButtonTextColor"] = Color.FromHex("#007AFF");
            else
                Current.Resources["ButtonTextColor"] = (isDark) ? Color.White : Color.Black;
            Current.Resources["ListViewBackgroundColor"] = (isDark) ? Color.FromHex("#1A1A1A") : Color.WhiteSmoke;
            SetAccentColor();
        }

        private static void SetAccentColor()
        {
            if (Settings.UseDefaultAccentColor)
            {
                // we shall keep the standards
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        Current.Resources["AccentColor"] = Color.Accent;
                        break;
                    case Device.Android:
                        Current.Resources["AccentColor"] = Color.FromHex("#1872A2");
                        break;
                    case Device.macOS:
                        Current.Resources["AccentColor"] = Color.Accent;
                        break;
                    case Device.UWP:
                        Current.Resources["AccentColor"] = Color.Accent;
                        break;
                    default:
                        Current.Resources["AccentColor"] = Color.FromHex("#1872A2");
                        break;
                }
            }
            else
            {
                // set the color depending on the selected color
                var isDark = (SelectedTheme == ApplicationTheme.Dark);
                switch (Settings.SelectedAccentColor)
                {
                    case 0:
                        // red
                        Current.Resources["AccentColor"] = (isDark) ? Color.Firebrick : Color.Crimson;
                        break;
                    case 1:
                        // blue
                        Current.Resources["AccentColor"] = (isDark) ? Color.MediumBlue : Color.Blue;
                        break;
                    case 2:
                        // purple
                        Current.Resources["AccentColor"] = (isDark) ? Color.Purple : Color.DarkViolet;
                        break;
                    case 3:
                        // rose
                        Current.Resources["AccentColor"] = (isDark) ? Color.Orchid : Color.Violet;
                        break;
                    case 4:
                        // orange
                        Current.Resources["AccentColor"] = (isDark) ? Color.Coral : Color.Coral;
                        break;
                    case 5:
                        // yellow
                        Current.Resources["AccentColor"] = Color.FromHex("#FFBA00");
                        break;
                    case 6:
                        // green
                        Current.Resources["AccentColor"] = (isDark) ? Color.DarkGreen : Color.Green;
                        break;
                    case 7:
                        // gray
                        Current.Resources["AccentColor"] = (isDark) ? Color.SlateGray : Color.LightSlateGray;
                        break;
                    case 8:
                        // brown
                        Current.Resources["AccentColor"] = (isDark) ? Color.Sienna : Color.Peru;
                        break;
                }
            }
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
