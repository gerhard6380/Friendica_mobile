using Friendica_Mobile.UWP.HttpRequests;
using Friendica_Mobile.UWP.Models;
using Friendica_Mobile.UWP.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace Friendica_Mobile.UWP.Mvvm
{
    class AboutViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        // show or hide loading indicator and change status of buttons
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        // Indicator for no Settings declaring that the data are samples
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set { _noSettings = value;
                OnPropertyChanged("NoSettings"); }
        }

        // app version 
        private string _appVersion;
        public string AppVersion
        {
            get { return _appVersion; }
            set { _appVersion = value;
                OnPropertyChanged("AppVersion"); }
        }

        // Friendica server
        public string FriendicaServer
        {
            get { return App.Settings.FriendicaServer; }
        }

        // Friendica Platform 
        private string _friendicaPlatform;
        public string FriendicaPlatform
        {
            get { return _friendicaPlatform; }
            set
            {   _friendicaPlatform = value;
                OnPropertyChanged("FriendicaPlatform"); }
        }

        // Friendica version 
        private string _friendicaVersion;
        public string FriendicaVersion
        {
            get { return _friendicaVersion; }
            set
            {   _friendicaVersion = value;
                OnPropertyChanged("FriendicaVersion"); }
        }

        // DFRN protocol version 
        private string _dfrnProtocolVersion;
        public string DfrnProtocolVersion
        {
            get { return _dfrnProtocolVersion; }
            set
            {   _dfrnProtocolVersion = value;
                OnPropertyChanged("DfrnProtocolVersion"); }
        }

        // db update version 
        private double _dbUpdateVersion;
        public double DbUpdateVersion
        {
            get { return _dbUpdateVersion; }
            set
            {   _dbUpdateVersion = value;
                OnPropertyChanged("DbUpdateVersion"); }
        }


        public AboutViewmodel()
        {
        }


        public void LoadConfig()
        {
            AppVersion = GetAppVersion();

            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
|| App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                NoSettings = true;
            }
            else
            {
                NoSettings = false;
                // load data from server
                var getConfig = new GetFriendicaConfig();
                getConfig.FriendicaConfigLoaded += GetConfig_FriendicaConfigLoaded;
                getConfig.LoadConfig();
            }
        }

        private async void GetConfig_FriendicaConfigLoaded(object sender, EventArgs e)
        {
            var getConfig = sender as GetFriendicaConfig;
            if (getConfig.StatusCode == HttpStatusCode.Ok)
            {
                var config = new FriendicaConfig(getConfig.ReturnString);
                FriendicaPlatform = config.ConfigSite.SiteFriendica.FriendicaPlatform;
                FriendicaVersion = config.ConfigSite.SiteFriendica.FriendicaVersion;
                DfrnProtocolVersion = config.ConfigSite.SiteFriendica.DfrnProtocolVersion;
                DbUpdateVersion = config.ConfigSite.SiteFriendica.DbUpdateVersion;

                // group results and save lists to App.xaml.cs for storing during app runtime (no reload if switching between pages)
                //SaveToApp();
            }
            else
            {
                // es gab einen Fehler beim Abrufen der Daten
                // "Fehler beim Laden der Daten. (Fehlermeldung: xxx) \n\n Möchten Sie es erneut versuchen?"
                string errorMsg;
                if (getConfig.StatusCode == HttpStatusCode.None)
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), loader.GetString("messageDialogHttpStatusNone"));
                else
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), getConfig.StatusCode.ToString());
                var dialog = new MessageDialog(errorMsg);
                //var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if ((int)result.Id == 0)
                    LoadConfig();
                else
                {
                }
            }
        }

        public static string GetAppVersion()
        {

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

        }
        //private void SaveToApp()
        //{
        //    App.ContactsFriends = Friends;
        //    App.ContactsForums = Forums;
        //    App.IsLoadedContacts = true;
        //}

        //private void LoadFromApp()
        //{
        //    Friends = App.ContactsFriends;
        //    Forums = App.ContactsForums;
        //    PrepareSources();
        //}
    }
}