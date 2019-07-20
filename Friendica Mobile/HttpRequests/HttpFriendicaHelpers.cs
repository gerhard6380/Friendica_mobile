using Friendica_Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    public class HttpFriendicaHelpers : HttpRequestsBase
    {
        public enum TestConnectionResults { OK, NotAnswered, NotAuthenticated, UnexpectedError }
        public enum ConfigResults { OK, NotAnswered, SerializationError, UnexpectedError }
        public enum OEmbedResults { OK, NotAnswered, SerializationError, NoUrl, UnexpectedError }

        public FriendicaConfig Config { get; set; }
        public JsonOEmbedYoutube OEmbedYoutube { get; set; }


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

        // calls [GET] youtube oembed data
        public async Task<OEmbedResults> GetOEmbedYoutubeByUrlAsync(string url)
        {
            if (url != null)
            {
                // some links might not show the usual www.youtube.com/watch?v=... style, so we need to reformat them
                if (url.Contains("https://www.youtube.com/embed/"))
                {
                    var uri = new Uri(url);
                    string video = "";
                    foreach (var part in uri.Segments)
                    {
                        if (part == "/" || part == "embed/")
                            continue;
                        else
                            video = part;
                    }
                    url = String.Format("https://www.youtube.com/watch?v={0}", video);
                }
                // url is already coded correctly
                url = WebUtility.UrlEncode(url);
                var urlEmbed = string.Format("http://www.youtube.com/oembed?url={0}&format=json", url);

                await this.GetStringWithoutCredentials(urlEmbed);

                switch (StatusCode)
                {
                    // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                    case 0:
                    case System.Net.HttpStatusCode.BadGateway:
                    case System.Net.HttpStatusCode.NotFound:
                        return OEmbedResults.NotAnswered;
                    case System.Net.HttpStatusCode.OK:
                        {
                            try
                            {
                                OEmbedYoutube = JsonConvert.DeserializeObject<JsonOEmbedYoutube>(ReturnString);
                                return OEmbedResults.OK;
                            }
                            catch { return OEmbedResults.SerializationError; }
                        }
                    default:
                        return OEmbedResults.UnexpectedError;
                }
            }
            else
                return OEmbedResults.NoUrl;
        }
    }
}
