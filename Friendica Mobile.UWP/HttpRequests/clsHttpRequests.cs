using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace Friendica_Mobile.UWP.HttpRequests
{
    public class clsHttpRequests
    {
        private readonly string _boundary = "----------" + DateTime.Now.Ticks.ToString();

        // Input values
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
        
        public clsHttpRequests()
        {
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

        public virtual async Task GetString(string url, string username, string password)
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
                _errorMessage = response.ReasonPhrase;
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

        public virtual async Task DeleteString(string url, string username, string password)
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
                response = await httpClient.DeleteAsync(uri);
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

        public virtual async Task<string> DeleteStringAsync(string url, string username, string password)
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
                response = await httpClient.DeleteAsync(uri);
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
                return "";
            }
        }


        public virtual async Task<string> GetStringAsync(string url, string username, string password)
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
                return _returnString;
            }
            catch (Exception ex)
            {
                _errorMessage = ex.Message;
                _errorHresult = ex.HResult;
                OnRequestFinishedChanged();
                return "";
            }
        }

        public virtual async void PostString(string url, string username, string password, string content)
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


        public virtual async Task<string> PostStringAsync(string url, string username, string password, string content)
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
                    OnRequestFinishedChanged();
                    return _returnString;
                }
                catch (Exception ex)
                {
                    _errorMessage = ex.Message;
                    _errorHresult = ex.HResult;
                    OnRequestFinishedChanged();
                    return "";
                }
            }
        }


        public virtual async void PostMultipart(string url, string username, string password, Dictionary<string, object> data, object newPost = null)
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

                    if (data != null)
                    {
                        foreach (var entry in data)
                        {
                            if (entry.Value is byte[])
                            {
                                byte[] bytes = entry.Value as byte[];
                                var inputstream = bytes.AsBuffer().AsStream().AsInputStream();
                                var stream = new HttpStreamContent(inputstream);
                                stream.Headers.ContentType = HttpMediaTypeHeaderValue.Parse("image/jpeg");
                                await stream.BufferAllAsync();
                                multipartContent.Add(stream, "media", "image.jpg");
                            }
                            else
                            {
                                string value = entry.Value as string;
                                if (value != null)
                                    multipartContent.Add(new HttpStringContent(value), entry.Key);
                            }
                        }
                    }

                    var response = await httpClient.PostAsync(uri, multipartContent);
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


        public virtual async Task<string> PostMultipartAsync(string url, string username, string password, Dictionary<string, object> data, string contentType = "image/jpeg", string filename = "image.jpg")
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

                    if (data != null)
                    {
                        foreach (var entry in data)
                        {
                            if (entry.Value is byte[])
                            {
                                byte[] bytes = entry.Value as byte[];
                                var inputstream = bytes.AsBuffer().AsStream().AsInputStream();
                                var stream = new HttpStreamContent(inputstream);
                                stream.Headers.ContentType = HttpMediaTypeHeaderValue.Parse(contentType);
                                await stream.BufferAllAsync();
                                multipartContent.Add(stream, entry.Key, filename);
                            }
                            else
                            {
                                string value = entry.Value as string;
                                if (value != null)
                                    multipartContent.Add(new HttpStringContent(value), entry.Key);
                            }
                        }
                    }

                    var response = await httpClient.PostAsync(uri, multipartContent);
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
                    return "";
                }
            }
        }

    }
}
