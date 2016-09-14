using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;

namespace BackgroundTasks
{
    class PostFriendicaMessage
    {
        ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
        AppSettings appSettings = new AppSettings();
        private string _url;
        private string _username;
        private string _password;
        private readonly string _boundary = "----------" + DateTime.Now.Ticks.ToString();


        public event EventHandler FriendicaMessageSent;

        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public FriendicaMessageNew NewMessage;

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


        public PostFriendicaMessage()
        {
        }


        public void PostFriendicaMessageNew(FriendicaMessageNew newMessage)
        {
            NewMessage = newMessage;
            var data = PrepareData();

            var url = String.Format("{0}/api/direct_messages/new",
                appSettings.FriendicaServer);
            this.PostMultipart(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword, data);
        }


        private Dictionary<string, object> PrepareData()
        {
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("user_id", NewMessage.NewMessageUserUrl);
            Parameters.Add("text", NewMessage.NewMessageText);
            Parameters.Add("replyto", NewMessage.NewMessageReplyTo);
            Parameters.Add("title", NewMessage.NewMessageTitle);
            return Parameters;
        }

        public async void PostMultipart(string url, string username, string password, IEnumerable<KeyValuePair<string, object>> data)
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
                    FriendicaMessageSent?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    _isSuccessStatusCode = false;
                    _errorMessage = ex.Message;
                    _errorHresult = ex.HResult;
                    FriendicaMessageSent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

    }
}
