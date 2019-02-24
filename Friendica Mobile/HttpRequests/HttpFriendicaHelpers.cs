using Friendica_Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    public class HttpFriendicaHelpers : HttpRequestsBase
    {
        public enum TestConnectionResults { OK, NotAnswered, NotAuthenticated, UnexpectedError }
        public enum ConfigResults { OK, NotAnswered, SerializationError, UnexpectedError }

        private FriendicaConfig _config;
        public FriendicaConfig Config
        {
            get { return _config; }
            set { _config = value; }
        }

        // constructors (passing login data to the underlying base class
        public HttpFriendicaHelpers(string baseUrlServer, string username, string password) : base(baseUrlServer, username, password)
        {}

        public HttpFriendicaHelpers(string baseUrlServer, string token) : base(baseUrlServer, token)
        {}


        // calls [GET] /api/account/verify_credentials
        public async Task<TestConnectionResults> GetAccountVerifyCredentialsAsync()
        {
            var url = String.Format("{0}/api/account/verify_credentials", _serverBaseUrl);
            await this.GetStringAsync(url);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return TestConnectionResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return TestConnectionResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    return TestConnectionResults.OK;
                default:
                        return TestConnectionResults.UnexpectedError;
            }
        }

        // calls [GET] /api/statusnet/config
        public async Task<ConfigResults> GetStatusnetConfigAsync()
        {
            var url = String.Format("{0}/api/statusnet/config", _serverBaseUrl);
            await this.GetStringAsync(url);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return ConfigResults.NotAnswered;
                case System.Net.HttpStatusCode.OK:
                    {
                        try
                        {
                            Config = JsonConvert.DeserializeObject<FriendicaConfig>(ReturnString);
                            return ConfigResults.OK;
                        }
                        catch { return ConfigResults.SerializationError; }
                    }
                default:
                    return ConfigResults.UnexpectedError;
            }
        }
    }
}
