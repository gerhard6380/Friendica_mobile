using Friendica_Mobile.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class A3_Browser : Page
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public A3_Browser()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(BrowserViewmodel))
                    this.DataContext = e.Parameter as BrowserViewmodel;
            }

            var mvvm = this.DataContext as BrowserViewmodel;
            mvvm.IsLoading = true;
            base.OnNavigatedTo(e);
        }

        private async void webviewBrowser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            var mvvm = this.DataContext as BrowserViewmodel;

            if (args.IsSuccess)
            {
                mvvm.IsLoading = false;
            }
            else
            {
                if (args.WebErrorStatus == Windows.Web.WebErrorStatus.CertificateIsInvalid && args.Uri.ToString().Contains("https://friendika.openmindspace.org/profile/friendicadevelopers"))
                {
                    Uri localStreamUri = webviewBrowser.BuildLocalStreamUri("MyTag", "/");
                    BadHttpsStreamResolver resolver = new BadHttpsStreamResolver(args.Uri, localStreamUri);
                    webviewBrowser.NavigateToLocalStreamUri(localStreamUri, resolver);
                }
                else if (args.WebErrorStatus == Windows.Web.WebErrorStatus.CertificateIsInvalid)
                {
                    string errorMsg = loader.GetString("messageDialogBrowserInvalidCertificate");
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(1, 1);
                    if (dialog.Result == 0)
                    {
                        Uri localStreamUri = webviewBrowser.BuildLocalStreamUri("MyTag", "/");
                        BadHttpsStreamResolver resolver = new BadHttpsStreamResolver(args.Uri, localStreamUri);
                        webviewBrowser.NavigateToLocalStreamUri(localStreamUri, resolver);
                    }
                    else
                        mvvm.IsLoading = false;
                }
                else if (args.WebErrorStatus == WebErrorStatus.NotFound && args.Uri.ToString().StartsWith("http://"))
                {
                    var httpsUri = new Uri(args.Uri.ToString().Replace("http://", "https://"));
                    mvvm.Url = httpsUri.AbsoluteUri.Replace(httpsUri.Query, "");
                    mvvm.Uri = httpsUri;
                    webviewBrowser.Navigate(httpsUri);
                }
                else
                {
                    string errorMsg = loader.GetString("messageDialogBrowserHttpError");
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                    mvvm.IsLoading = false;
                }
            }
        }
    }


    public sealed class BadHttpsStreamResolver : IUriToStreamResolver
    {
        private readonly string baseUri;
        private readonly string localStreamUri;
        private readonly HttpClient hc;

        public BadHttpsStreamResolver(Uri baseUri, Uri localStreamUri)
        {
            this.baseUri = baseUri.ToString();
            this.localStreamUri = localStreamUri.ToString();
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            // specify here which certificate errors should we ignore
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            hc = new HttpClient(filter);
            var headers = hc.DefaultRequestHeaders;
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }

        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            // TODO better uri validation and conversion
            Uri targetUri = new Uri(uri.ToString().Replace(localStreamUri, baseUri));
            return GetInputStream(targetUri).AsAsyncOperation();
        }

        public async Task<IInputStream> GetInputStream(Uri targetUri)
        {
            try
            {
                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, targetUri);
                //HttpResponseMessage response = await hc.SendRequestAsync(request);
                HttpResponseMessage response = await hc.GetAsync(targetUri);
                IInputStream stream = await response.Content.ReadAsInputStreamAsync();
                return stream;
            }
            catch
            {
                return null;
            }
        }
    }

}
