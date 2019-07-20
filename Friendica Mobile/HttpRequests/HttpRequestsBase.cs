using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    public abstract class HttpRequestsBase
    {
        // set to true if we are in sample mode, hence no http requests needed
        internal bool _isSampleMode;

        // boundary value for multipart requests
        private readonly string _boundary = "----------" + DateTime.Now.Ticks.ToString();
         
        // Input values
        internal string _serverBaseUrl;
        internal string _username;
        internal string _password;
        internal string _token;

        internal string _requestedUrl;

        // event showing the finalization of the request
        public event EventHandler HttpRequestFinished;

        protected virtual void OnHttpRequestFinishedChanged()
        {
            HttpRequestFinished?.Invoke(this, EventArgs.Empty);
        }

        // Output values
        private HttpStatusCode _statusCode;
        public HttpStatusCode StatusCode
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
        

        protected HttpRequestsBase(string serverBaseUrl, string username, string password)
        {
            _serverBaseUrl = serverBaseUrl;
            _username = username;
            _password = password;
            _isSampleMode = (serverBaseUrl == "" || serverBaseUrl == "https://" || serverBaseUrl == "http://" || serverBaseUrl == null);
        }

        protected HttpRequestsBase(string serverBaseUrl, string token)
        {
            _serverBaseUrl = serverBaseUrl;
            _token = token;
            _isSampleMode = (serverBaseUrl == "" || serverBaseUrl == "https://" || serverBaseUrl == "http://" || serverBaseUrl == null);
        }


        // call a GET method without basic authentication 
        public virtual async Task<string> GetStringWithoutCredentials(string url)
        {
            var httpClient = new HttpClient();
            var headers = httpClient.DefaultRequestHeaders;
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var response = new HttpResponseMessage();
            var uri = new Uri(url);
            try
            {
                response = await httpClient.GetAsync(uri);
                _statusCode = response.StatusCode;
                _returnString = await response.Content.ReadAsStringAsync();
                _isSuccessStatusCode = response.IsSuccessStatusCode;
                OnHttpRequestFinishedChanged();
                return _returnString;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                _errorHresult = ex.HResult;
                OnHttpRequestFinishedChanged();
                return string.Empty;
            }
        }


        // call a GET method with basic authentication
        public virtual async Task<string> GetStringAsync(string url)
        {
            // exit method if we are in Sample mode
            if (_isSampleMode)
                return string.Empty;

            var config = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username, _password)
            };
            var httpClient = new HttpClient(config);

            var headers = httpClient.DefaultRequestHeaders;
            // add the access token if there is no username and password
            if (_username == null && _password == null)
            {
                if (_token != null && _token != "")
                {
                    if (_token.Contains("\""))
                        _token = _token.Replace("\"", "");
                    headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                }
            }
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var response = new HttpResponseMessage();
            var uri = new Uri(url);
            try
            {
                response = await httpClient.GetAsync(uri);
                _statusCode = response.StatusCode;
                _returnString = await response.Content.ReadAsStringAsync();
                _isSuccessStatusCode = response.IsSuccessStatusCode;
                OnHttpRequestFinishedChanged();
                return _returnString;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                _errorHresult = ex.HResult;
                OnHttpRequestFinishedChanged();
                return string.Empty;
            }
        }


        // call a DELETE method with basic authentication
        public virtual async Task<string> DeleteStringAsync(string url)
        {
            // exit method if we are in Sample mode
            if (_isSampleMode)
                return string.Empty;

            var config = new HttpClientHandler();
            if (_username != null && _password != null)
            {
                config.Credentials = new NetworkCredential(_username, _password);
            }

            var httpClient = new HttpClient(config);

            var headers = httpClient.DefaultRequestHeaders;
            // add the access token if there is no username and password
            if (_username == null && _password == null)
            {
                if (_token != null && _token != "")
                {
                    if (_token.Contains("\""))
                        _token = _token.Replace("\"", "");
                    headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                }
            }
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var response = new HttpResponseMessage();
            var uri = new Uri(url);
            try
            {
                response = await httpClient.DeleteAsync(uri);
                _statusCode = response.StatusCode;
                _returnString = await response.Content.ReadAsStringAsync();
                if (_statusCode != HttpStatusCode.OK)
                {
                    _errorMessage = JsonConvert.DeserializeObject<HttpErrorMessage>(ReturnString).Message;
                }
                _isSuccessStatusCode = response.IsSuccessStatusCode;
                OnHttpRequestFinishedChanged();
                return _returnString;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                _errorHresult = ex.HResult;
                OnHttpRequestFinishedChanged();
                return string.Empty;
            }
        }


        // call a POST method with basic authentication
        public virtual async Task<string> PostStringAsync(string url, string content)
        {
            // exit method if we are in Sample mode
            if (_isSampleMode)
                return string.Empty;
            
            var config = new HttpClientHandler();
            if (_username != null && _password != null)
            {
                config.Credentials = new NetworkCredential(_username, _password);
            }

            using (var httpClient = new HttpClient(config))
            {
                var headers = httpClient.DefaultRequestHeaders;
                // add the access token if there is no username and password
                if (_username == null && _password == null)
                {
                    if (_token != null && _token != "")
                    {
                        if (_token.Contains("\""))
                            _token = _token.Replace("\"", "");
                        headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
                    }
                }
                headers.UserAgent.ParseAdd("ie");
                headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                var uri = new Uri(url);
                try
                {
                    var multipartContent = new MultipartFormDataContent(_boundary);
                    multipartContent.Add(new StringContent(content), "json");
                    var response = await httpClient.PostAsync(uri, multipartContent);
                    _statusCode = response.StatusCode;
                    _returnString = await response.Content.ReadAsStringAsync();
                    // TODO: rework Error Message reading
                    if (_statusCode != HttpStatusCode.OK)
                    {
                        _errorMessage = JsonConvert.DeserializeObject<HttpErrorMessage>(ReturnString).Message;
                    }
                    _isSuccessStatusCode = response.IsSuccessStatusCode;
                    OnHttpRequestFinishedChanged();
                    return _returnString;
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    _errorHresult = ex.HResult;
                    OnHttpRequestFinishedChanged();
                    return string.Empty;
                }
            }
        }


        // call a POST method with multipart content and basic authentication
        public virtual async Task<string> PostMultipartAsync(string url, 
            Dictionary<string, object> data, string contentType = "image/jpeg", string filename = "image.jpg")
        {
            // exit method if we are in Sample mode
            if (_isSampleMode)
                return string.Empty;

            var config = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username, _password)
            };

            using (var httpClient = new HttpClient(config))
            {
                var headers = httpClient.DefaultRequestHeaders;
                headers.UserAgent.ParseAdd("ie");
                headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                var uri = new Uri(url);

                try
                {
                    var multipartContent = new MultipartFormDataContent(_boundary);

                    if (data != null)
                    {
                        foreach (var entry in data)
                        {
                            if (entry.Value is byte[])
                            {
                                // currently only one byte array per request is possible, so contentType and filename can be outside of Dictionary
                                byte[] bytes = entry.Value as byte[];
                                var array = new ByteArrayContent(bytes);
                                array.Headers.Add("Content-Type", contentType);
                                multipartContent.Add(array, entry.Key, filename);
                            }
                            else
                            {
                                if (entry.Value is string value)
                                    multipartContent.Add(new StringContent(value), entry.Key);
                            }
                        }
                    }

                    var response = await httpClient.PostAsync(uri, multipartContent);
                    _statusCode = response.StatusCode;
                    _returnString = await response.Content.ReadAsStringAsync();
                    _isSuccessStatusCode = response.IsSuccessStatusCode;
                    OnHttpRequestFinishedChanged();
                    return _returnString;
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    _errorHresult = ex.HResult;
                    OnHttpRequestFinishedChanged();
                    return string.Empty;
                }
            }
        }

    }
}
