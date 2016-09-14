using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace Friendica_Mobile.Mvvm
{
    class BrowserViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        private bool _isVisibleHeader;
        public bool IsVisibleHeader
        {
            get { return _isVisibleHeader; }
            set { _isVisibleHeader = value;
                OnPropertyChanged("IsVisibleHeader"); }
        }

        private string _pageTitle;
        public string PageTitle
        {
            get { return _pageTitle; }
            set { _pageTitle = value;
                OnPropertyChanged("PageTitle"); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value;
                OnPropertyChanged("IsLoading"); }
        }

        private string _url;
        public string Url
        {
            get { return _url; }
            set { _url = value;
                OnPropertyChanged("Url"); }
        }

        private Uri _uri;
        public Uri Uri
        {
            get { return _uri; }
            set { _uri = value;
                OnPropertyChanged("Uri"); }
        }


        // button for user to start the page in the standard browser of the device
        Mvvm.Command _openInBrowserCommand;
        public Mvvm.Command OpenInBrowserCommand { get { return _openInBrowserCommand ?? (_openInBrowserCommand = new Mvvm.Command(ExecuteOpenInBrowser)); } }
        private async void ExecuteOpenInBrowser()
        {
            var success = await Launcher.LaunchUriAsync(Uri);
        }


        public BrowserViewmodel()
        {            
        }


    }
}