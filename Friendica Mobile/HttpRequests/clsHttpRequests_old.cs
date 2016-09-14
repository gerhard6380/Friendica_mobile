using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Friendica_Mobile.HttpRequests
{
    //class RequestConfig 
    //{
    //    private bool _retryWithCredentials = false;
    //    // Input values
    //    private string _server;
    //    private string _username;
    //    private string _password;
    //    // output values
    //    private bool _serverAnswered;
    //    public bool ServerAnswered
    //    {
    //        get { return _serverAnswered; }
    //        set { _serverAnswered = value; }
    //    }

    //    public event EventHandler AuthenticationSuccessChanged;
    //    private bool _authenticationSuccess;
    //    public bool AuthenticationSuccess
    //    {
    //        get { return _authenticationSuccess; }
    //        set { _authenticationSuccess = value; OnAuthenticationSuccessChanged(); }
    //    }

    //    private bool _noAuthenticationRequired;
    //    public bool NoAuthenticationRequired
    //    {
    //        get { return _noAuthenticationRequired; }
    //        set { _noAuthenticationRequired = value; }
    //    }

    //    protected virtual void OnAuthenticationSuccessChanged()
    //    {
    //        if (AuthenticationSuccessChanged != null)
    //        {
    //            AuthenticationSuccessChanged(this, EventArgs.Empty);
    //        }
    //    }

    //    // calling method can read the Statuscode to react on the result accordingly
    //    private HttpStatusCode _statusCode;
    //    public HttpStatusCode StatusCode
    //    {
    //        get { return _statusCode; }
    //        set { _statusCode = value; }
    //    }

    //    public RequestConfig()
    //    {

    //    }

    //    public async void ChangeStatus()
    //    {
    //        await Task.Delay(TimeSpan.FromSeconds(5));
    //        if (AuthenticationSuccess)
    //            AuthenticationSuccess = false;
    //        else
    //            AuthenticationSuccess = true;
    //    }

    //    public async void RetrieveConfig(string server, string username, string password)
    //    {
    //        _server = server;
    //        _username = username;
    //        _password = password;
    //        var httpClient = new HttpClient();
    //        var headers = httpClient.DefaultRequestHeaders;
    //        headers.UserAgent.ParseAdd("ie");
    //        headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
    //        var response = new HttpResponseMessage();
    //        string responseBodyAsText;
    //        var uri = new Uri(_server + "/api/statusnet/config");
    //        try
    //        {
    //            response = await httpClient.GetAsync(uri);
    //            //response.EnsureSuccessStatusCode();
    //            responseBodyAsText = await response.Content.ReadAsStringAsync();
    //            if (response.StatusCode == HttpStatusCode.Ok)
    //            {
    //                _authenticationSuccess = true;
    //                OnAuthenticationSuccessChanged();
    //            }
    //            else
    //            {
    //                _authenticationSuccess = false;
    //                OnAuthenticationSuccessChanged();
    //            }
    //            StatusCode = response.StatusCode;
    //        }
    //        catch
    //        {
    //        }
    //    }
        //public void RequestPrepare(string server, string username, string password)
        //{
        //    _server = server;
        //    _username = username;
        //    _password = password;
        //    var request = (HttpWebRequest)WebRequest.CreateHttp(_server);
        //    request.Method = "GET";
        //    if (_retryWithCredentials)
        //        request.Credentials = new NetworkCredential(_username, _password);
        //    request.UseDefaultCredentials = false;
            
        //    request.BeginGetResponse(RequestResponse, request);
        //}

        //private void RequestResponse(IAsyncResult result)
        //{
        //    var request = result.AsyncState as HttpWebRequest;
        //    try
        //    {
        //        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
        //        this.StatusCode = response.StatusCode;
        //        if (this.StatusCode == HttpStatusCode.OK && request.Credentials == null)
        //        {
        //            this.ServerAnswered = true;
        //            this.NoAuthenticationRequired = true;
        //            //this.AuthenticationSuccess = false;
        //            _retryWithCredentials = true;
        //            RequestPrepare(_server, _username, _password);
        //        }
        //        else if (this.StatusCode == HttpStatusCode.OK)
        //        {
        //            // everything was ok = server responds and allows login
        //            this.ServerAnswered = true;
        //            this.AuthenticationSuccess = true;
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        if (request.Credentials == null)
        //        {
        //            this.ServerAnswered = true;
        //            this.NoAuthenticationRequired = false;
        //            _retryWithCredentials = true;
        //            RequestPrepare(_server, _username, _password);
        //        }
        //        else
        //        {
        //            HttpWebResponse response = (HttpWebResponse)ex.Response;
        //            this.StatusCode = response.StatusCode;
        //            if (this.StatusCode == HttpStatusCode.NotFound)
        //            {
        //                // no reaction from server, maybe wrong url
        //                this.ServerAnswered = false;
        //                this.AuthenticationSuccess = false;
        //            }
        //            if (this.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                // server responded but rejected the given credentials
        //                this.ServerAnswered = true;
        //                this.AuthenticationSuccess = false;
        //            }
        //        }
        //    }

        //}

    //}


    //    // Check Connection to WebDAV server with specified server address and credentials (username, password)
    //    public class WebDAVCheckConn
    //{
    //    private bool _retryWithCredentials = false;
    //    // Input values
    //    private string _server;
    //    private string _username;
    //    private string _password;
    //    // output values
    //    private bool _serverAnswered;
    //    private bool _noAuthenticationRequired;
    //    private bool _authenticationSuccess;
    //    private HttpStatusCode _statusCode;

    //    public bool ServerAnswered
    //    {
    //        get { return _serverAnswered; }
    //        set { _serverAnswered = value; }
    //    }

    //    public bool NoAuthenticationRequired
    //    {
    //        get { return _noAuthenticationRequired; }
    //        set { _noAuthenticationRequired = value; }
    //    }

    //    // calling method can set on EventHandler to trigger if connection has succeed or not
    //    public event System.EventHandler AuthenticationSuccessChanged;
    //    public bool AuthenticationSuccess
    //    {
    //        get { return _authenticationSuccess; }
    //        set
    //        {
    //            _authenticationSuccess = value;
    //            OnAuthenticationSuccessChanged();
    //        }
    //    }

    //    protected virtual void OnAuthenticationSuccessChanged()
    //    {
    //        if (AuthenticationSuccessChanged != null)
    //            AuthenticationSuccessChanged(this, EventArgs.Empty);
    //    }

    //    // calling method can read the Statuscode to react on the result accordingly
    //    public HttpStatusCode StatusCode
    //    {
    //        get { return _statusCode; }
    //        set { _statusCode = value; }
    //    }

    //    public WebDAVCheckConn()
    //    {
    //    }

    //    //public void RequestPrepare(string server, string username, string password)
    //    //{
    //    //    _server = server;
    //    //    _username = username;
    //    //    _password = password;
    //    //    var request = (HttpWebRequest)WebRequest.CreateHttp(_server);
    //    //    request.Method = "GET";
    //    //    if (_retryWithCredentials)
    //    //        request.Credentials = new NetworkCredential(_username, _password);
    //    //    request.UseDefaultCredentials = false;
    //    //    request.BeginGetResponse(RequestResponse, request);
    //    //}

    //    //private void RequestResponse(IAsyncResult result)
    //    //{
    //    //    var request = result.AsyncState as HttpWebRequest;
    //    //    try
    //    //    {
    //    //        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);
    //    //        this.StatusCode = response.StatusCode;
    //    //        if (this.StatusCode == HttpStatusCode.OK && request.Credentials == null)
    //    //        {
    //    //            this.ServerAnswered = true;
    //    //            this.NoAuthenticationRequired = true;
    //    //            //this.AuthenticationSuccess = false;
    //    //            _retryWithCredentials = true;
    //    //            RequestPrepare(_server, _username, _password);
    //    //        }
    //    //        else if (this.StatusCode == HttpStatusCode.OK)
    //    //        {
    //    //            // everything was ok = server responds and allows login
    //    //            this.ServerAnswered = true;
    //    //            this.AuthenticationSuccess = true;
    //    //        }
    //    //    }
    //    //    catch (WebException ex)
    //    //    {
    //    //        if (request.Credentials == null)
    //    //        {
    //    //            this.ServerAnswered = true;
    //    //            this.NoAuthenticationRequired = false;
    //    //            _retryWithCredentials = true;
    //    //            RequestPrepare(_server, _username, _password);
    //    //        }
    //    //        else
    //    //        {
    //    //            HttpWebResponse response = (HttpWebResponse)ex.Response;
    //    //            this.StatusCode = response.StatusCode;
    //    //            if (this.StatusCode == HttpStatusCode.NotFound)
    //    //            {
    //    //                // no reaction from server, maybe wrong url
    //    //                this.ServerAnswered = false;
    //    //                this.AuthenticationSuccess = false;
    //    //            }
    //    //            if (this.StatusCode == HttpStatusCode.Unauthorized)
    //    //            {
    //    //                // server responded but rejected the given credentials
    //    //                this.ServerAnswered = true;
    //    //                this.AuthenticationSuccess = false;
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //} // END OF public class WebDAVCheckConn
}
