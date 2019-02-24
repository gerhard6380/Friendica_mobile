using System.Windows.Input;
using Friendica_Mobile.Strings;
using Xamarin.Forms;
using System;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;

namespace Friendica_Mobile.ViewModel
{
    public class AboutViewModel : BaseViewModel
    {
        // inform view to change the e-mail address of admin as span is not updating through binding
        public event EventHandler OnConfigLoaded;


        public bool NoSettings
        {
            get { return !Settings.IsFriendicaLoginDefined(); }
        }

        public string FriendicaServer
        {
            get { return Settings.FriendicaServer; }
        }

        private FriendicaConfig _siteConfig;
        public FriendicaConfig SiteConfig
        {
            get { return _siteConfig; }
            set
            {
                SetProperty(ref _siteConfig, value);
                OnPropertyChanged("FriendicaVersion");
                OnPropertyChanged("DbUpdateVersion");
                OnPropertyChanged("DfrnProtocolVersion");
                OnConfigLoaded?.Invoke(this, EventArgs.Empty);
            }
        }

        public string FriendicaVersion
        {
            get { return SiteConfig?.ConfigSite.SiteFriendica.FriendicaVersion; }
        }

        public string DbUpdateVersion
        {
            get { return SiteConfig?.ConfigSite.SiteFriendica.DbUpdateVersion; }
        }

        public string DfrnProtocolVersion
        {
            get { return SiteConfig?.ConfigSite.SiteFriendica.DfrnProtocolVersion; }
        }


        public string AppVersion
        {
            get { return DependencyService.Get<IGetSystemTheme>().GetAppVersion(); }
        }


#region Commands

        private ICommand _navigateSettingsCommand;
        public ICommand  NavigateSettingsCommand => _navigateSettingsCommand ?? (_navigateSettingsCommand = new Command(NavigateSettings));
        private void NavigateSettings()
        {
            NavigateTo(new Views.Settings());
        }

        private ICommand _mailToAdminCommand;
        public ICommand MailToAdminCommand => _mailToAdminCommand ?? (_mailToAdminCommand = new Command(MailToAdmin));
        public void MailToAdmin()
        {
            // open e-mail client with the address defined as admin on Friendica server
            Device.OpenUri(new Uri($"mailto:{SiteConfig.ConfigSite.SiteEmail}"));
        }

        private ICommand _linkFriendicaWebsiteCommand;
        public ICommand LinkFriendicaWebsiteCommand => _linkFriendicaWebsiteCommand ?? (_linkFriendicaWebsiteCommand = new Command(LinkFriendicaWebsite));
        public void LinkFriendicaWebsite()
        {
            // open e-mail client with the address defined as admin on Friendica server
            Device.OpenUri(new Uri("https://friendi.ca"));
        }

        private ICommand _mailToDeveloperCommand;
        public ICommand MailToDeveloperCommand => _mailToDeveloperCommand ?? (_mailToDeveloperCommand = new Command(MailToDeveloper));
        public void MailToDeveloper()
        {
            // open e-mail client with the address defined as admin on Friendica server
            Device.OpenUri(new Uri($"mailto:friendicamobile@seeber.at"));
        }


#endregion


        public AboutViewModel()
        {
            Title = AppResources.pageTitleAbout_Text;
            if (!NoSettings)
            {
                LoadConfig();  
            }
        }

        internal async void LoadConfig()
        {
            // show activity indicator
            ActivityIndicatorText = AppResources.TextAboutLoadingConfig;
            ActivityIndicatorVisible = true;
            // start http request
            var http = new HttpFriendicaHelpers(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            var configResults = await http.GetStatusnetConfigAsync();
            ActivityIndicatorVisible = false;

            LabelServerActivityFailed = "";
            switch (configResults)
            {
                case HttpFriendicaHelpers.ConfigResults.NotAnswered:
                    LabelServerActivityFailed = AppResources.messageDialogHttpStatusNone;
                    break;
                case HttpFriendicaHelpers.ConfigResults.SerializationError:
                    // show error with requested URL and the returned JSON upon serialization error
                    LabelServerActivityFailed = String.Format(AppResources.TextSerializationError,
                                                              http._requestedUrl, http.ReturnString);
                    break;
                case HttpFriendicaHelpers.ConfigResults.UnexpectedError:
                    LabelServerActivityFailed = String.Format(AppResources.TestConnectionResultUnexpectedError,
                                                              App.Contacts.StatusCode, App.Contacts.ErrorMessage, App.Contacts.ErrorHResult);
                    break;
                case HttpFriendicaHelpers.ConfigResults.OK:
                    SiteConfig = http.Config;
                    return;
            }

            // TODO: localize
            var result = await Application.Current.MainPage.DisplayAlert(AppResources.TextAboutAlertHeaderLoading,
                                                      String.Format(AppResources.messageDialogErrorOnLoadingData, LabelServerActivityFailed),
                                                      AppResources.buttonYes, AppResources.buttonNo);
            if (result)
                LoadConfig();
        }


    }
}
