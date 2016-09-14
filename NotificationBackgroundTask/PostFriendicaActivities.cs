using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BackgroundTasks
{
    class PostFriendicaActivities
    {
        public enum FriendicaActivity { like, dislike, unlike, undislike };
        AppSettings appSettings = new AppSettings();

        ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();

        public event EventHandler FriendicaActivitySent;
        protected virtual void OnFriendicaActivitySent()
        {
            if (FriendicaActivitySent != null)
                FriendicaActivitySent(this, EventArgs.Empty);
        }

        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public double PostId;
        public FriendicaActivity Activity;

        // Input values
        private string _url;
        private string _username;
        private string _password;
        private readonly string _boundary = "----------" + DateTime.Now.Ticks.ToString();

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

        public PostFriendicaActivities()
        {
        }

        public void PostFriendicaActivity(double id, FriendicaActivity activity)
        {
            PostId = id;
            Activity = activity;
            var url = String.Format("{0}/api/friendica/activity/{1}?id={2}",
                appSettings.FriendicaServer, 
                Activity,
                PostId);
            this.PostString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword, "");
        }


        private void PostFriendicaActivity_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaActivitySent();
        }

        public async void PostString(string url, string username, string password, string content)
        {
            _url = url;
            _username = username;
            _password = password;

            var filter = new HttpBaseProtocolFilter();
            filter.ServerCredential = new Windows.Security.Credentials.PasswordCredential(_url, _username, _password);
            filter.AllowUI = false;

            using (var httpClient = new HttpClient(filter))
            {
                var headers = httpClient.DefaultRequestHeaders;
                headers.UserAgent.ParseAdd("ie");
                headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                var uri = new Uri(_url);
                try
                {
                    var multipartContent = new HttpMultipartFormDataContent(_boundary);
                    multipartContent.Add(new HttpStringContent(content), "json");
                    var response = await httpClient.PostAsync(uri, multipartContent);
                    _statusCode = response.StatusCode;
                    _returnString = await response.Content.ReadAsStringAsync();
                    _isSuccessStatusCode = response.IsSuccessStatusCode;
                    FriendicaActivitySent?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    _isSuccessStatusCode = false;
                    _errorMessage = ex.Message;
                    _errorHresult = ex.HResult;
                    FriendicaActivitySent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

    }
}
