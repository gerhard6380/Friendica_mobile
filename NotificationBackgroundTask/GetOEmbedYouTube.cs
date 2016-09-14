using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace BackgroundTasks
{
    class GetOEmbedYoutube
    {
        AppSettings appSettings = new AppSettings();
        private string _url;

        public event EventHandler OEmbedYoutubeLoaded;
        protected virtual void OnOEmbedYoutubeLoaded()
        {
            if (OEmbedYoutubeLoaded != null)
                OEmbedYoutubeLoaded(this, EventArgs.Empty);
        }

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


        public GetOEmbedYoutube()
        {

        }

        public async Task<OEmbedYoutube> GetOEmbedYoutubeByUrl(string url)
        {
            if (url != null)
            {
                url = System.Net.WebUtility.UrlEncode(url);  
                var urlEmbed = string.Format("http://www.youtube.com/oembed?url={0}&format=json", url);
                var resultString = await this.GetStringWithoutCredentials(urlEmbed);
                var oEmbed = new OEmbedYoutube(resultString);
                RequestFinished += GetOEmbedYoutube_RequestFinished;
                return oEmbed;
            }
            else
                return null;
        }

        public virtual async Task<string> GetStringWithoutCredentials(string url)
        {
            _url = url;
            var httpClient = new HttpClient();
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
                return _returnString;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                _errorHresult = ex.HResult;
                OnRequestFinishedChanged();
                return _errorMessage;
            }
        }


        private void GetOEmbedYoutube_RequestFinished(object sender, EventArgs e)
        {
            OnOEmbedYoutubeLoaded();
        }
    }
}
