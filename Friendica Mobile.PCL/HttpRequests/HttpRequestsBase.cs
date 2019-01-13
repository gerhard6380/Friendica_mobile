using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.HttpRequests
{
    public class HttpRequestsBase
    {
        // boundary value for multipart requests
        private readonly string _boundary = "----------" + DateTime.Now.Ticks.ToString();

        // Input values
        private string _url;
        private string _username;
        private string _password;

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
        

        public HttpRequestsBase()
        {
        }


        // call a GET method without basic authentication 
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
        public virtual async Task<string> GetStringAsync(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;

            var config = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username, _password)
            };
            var httpClient = new HttpClient(config);

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
        public virtual async Task<string> DeleteStringAsync(string url, string username, string password)
        {
            _url = url;
            _username = username;
            _password = password;

            var config = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username, _password)
            };
            var httpClient = new HttpClient(config);

            var headers = httpClient.DefaultRequestHeaders;
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var response = new HttpResponseMessage();
            var uri = new Uri(_url);
            try
            {
                response = await httpClient.DeleteAsync(uri);
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


        // call a POST method with basic authentication
        public virtual async Task<string> PostStringAsync(string url, string username, string password, string content)
        {
            _url = url;
            _username = username;
            _password = password;

            var config = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username, _password)
            };

            using (var httpClient = new HttpClient(config))
            {
                var headers = httpClient.DefaultRequestHeaders;
                headers.UserAgent.ParseAdd("ie");
                headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                var uri = new Uri(_url);
                try
                {
                    var multipartContent = new MultipartFormDataContent(_boundary)
                    {
                        { new StringContent(content), "json" }
                    };
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


        // call a POST method with multipart content and basic authentication
        public virtual async Task<string> PostMultipartAsync(string url, string username, string password, 
            Dictionary<string, object> data, string contentType = "image/jpeg", string filename = "image.jpg")
        {
            _url = url;
            _username = username;
            _password = password;

            var config = new HttpClientHandler()
            {
                Credentials = new NetworkCredential(_username, _password)
            };

            using (var httpClient = new HttpClient(config))
            {
                var headers = httpClient.DefaultRequestHeaders;
                headers.UserAgent.ParseAdd("ie");
                headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                var uri = new Uri(_url);

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
