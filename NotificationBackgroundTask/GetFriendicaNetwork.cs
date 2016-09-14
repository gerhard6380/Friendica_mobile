using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BackgroundTasks
{
    class GetFriendicaNetwork
    {
        AppSettings appSettings;
        private string _url;
        private string _username;
        private string _password;

        // event showing the finalization of the request
        public event EventHandler RequestFinished;

        protected virtual void OnRequestFinishedChanged()
        {
            if (RequestFinished != null)
                RequestFinished(this, EventArgs.Empty);
        }

        // Output values
        private Windows.Web.Http.HttpStatusCode _statusCode;
        public Windows.Web.Http.HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        private string _returnString;
        public string ReturnString
        {
            get { return _returnString; }
        }

        private bool _isSuccessStatusCode;
        public bool IsSuccessStateCode
        {
            get { return _isSuccessStatusCode; }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        private int _errorHresult;
        public int ErrorHResult
        {
            get { return _errorHresult; }
        }


        public event EventHandler FriendicaNetworkLoaded;
        protected virtual void OnFriendicaNetworkLoaded()
        {
            if (FriendicaNetworkLoaded != null)
                FriendicaNetworkLoaded(this, EventArgs.Empty);
        }

        public GetFriendicaNetwork(AppSettings settings)
        {
            this.appSettings = settings;
        }

        public void GetFriendicaNetworkInitial(int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&timestamp={2}",
                appSettings.FriendicaServer,
                count,
                DateTime.Now.ToString());
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
        }

        public void GetFriendicaNetworkNextEntries(double maxId, int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&max_id={2}&timestamp={3}",
                appSettings.FriendicaServer,
                count,
                maxId,
                DateTime.Now.ToString());
            this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
        }

        public void GetFriendicaNetworkNewEntries(double minId, int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&since_id={2}&timestamp={3}",
                appSettings.FriendicaServer,
                count,
                minId,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
            GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }

        private void GetFriendicaNetwork_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaNetworkLoaded();
        }

        public async void GetString(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;

            var filter = new HttpBaseProtocolFilter();
            filter.ServerCredential = new Windows.Security.Credentials.PasswordCredential(_url, _username, _password);
            filter.AllowUI = false;

            var httpClient = new HttpClient(filter);
            var headers = httpClient.DefaultRequestHeaders;
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var response = new HttpResponseMessage();
            var uri = new Uri(_url);
            try
            {
                response = await httpClient.GetAsync(uri);
                _statusCode = response.StatusCode;
                _returnString = await response.Content.ReadAsStringAsync();
                _isSuccessStatusCode = response.IsSuccessStatusCode;
                OnRequestFinishedChanged();
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                _errorHresult = ex.HResult;
                OnRequestFinishedChanged();
            }
        }

    }
}
