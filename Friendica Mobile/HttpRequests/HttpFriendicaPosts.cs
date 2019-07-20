using Friendica_Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Friendica_Mobile.Models.FriendicaActivities;

namespace Friendica_Mobile.HttpRequests
{
    public class HttpFriendicaPosts : HttpRequestsBase
    {
        public List<JsonFriendicaPost> Posts { get; set; }

        // constructors (passing login data to the underlying base class
        public HttpFriendicaPosts(string baseUrlServer, string username, string password) : base(baseUrlServer, username, password)
        {
        }

        public HttpFriendicaPosts(string baseUrlServer, string token) : base(baseUrlServer, token)
        {
        }


        // enum for returning results from server - viewmodel will react on the results accordingly
        public enum GetFriendicaPostsResults { SampleMode, OK, NotAnswered, NotAuthenticated, SerializationError, UnexpectedError }


        // calls [GET] /api/statuses/home_timeline (= network posts and newsfeed items, home = user_timeline)
        public async Task<GetFriendicaPostsResults> GetNetworkInitialAsync(int count = 20)
        {
            if (_isSampleMode)
                return GetFriendicaPostsResults.SampleMode;

            //_requestedUrl = String.Format("{0}/api/statuses/home_timeline.json?count={1}&timestamp={2}",
                //_serverBaseUrl,
                //count,
                //DateTime.Now.ToString());
            // TODO: remove the following used for testing
            _requestedUrl = String.Format("{0}/api/statuses/home_timeline.json?count={1}&max_id={2}&timestamp={3}",
                _serverBaseUrl,
                1,
                283544,
                DateTime.Now.ToString());
            await this.GetStringAsync(_requestedUrl);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return GetFriendicaPostsResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return GetFriendicaPostsResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        try
                        {
                            Posts = JsonConvert.DeserializeObject<List<JsonFriendicaPost>>(ReturnString);
                            return GetFriendicaPostsResults.OK;
                        }
                        catch { return GetFriendicaPostsResults.SerializationError; }
                    }
                default:
                    return GetFriendicaPostsResults.UnexpectedError;
            }
        }


        // calls [GET] /api/conversation/show - put post id to get corresponding conversation
        public async Task<GetFriendicaPostsResults> GetFriendicaThreadByIdAsync(int id)
        {
            if (_isSampleMode)
                return GetFriendicaPostsResults.SampleMode;
            
            _requestedUrl = String.Format("{0}/api/conversation/show.json?id={1}",
                                          _serverBaseUrl,
                                          id);
            await this.GetStringAsync(_requestedUrl);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return GetFriendicaPostsResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return GetFriendicaPostsResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        try
                        {
                            Posts = JsonConvert.DeserializeObject<List<JsonFriendicaPost>>(ReturnString);
                            return GetFriendicaPostsResults.OK;
                        }
                        catch { return GetFriendicaPostsResults.SerializationError; }
                    }
                default:
                    return GetFriendicaPostsResults.UnexpectedError;
            }
        }


        // calls [GET] /api/statuses/home_timeline (= network posts and newsfeed items, home = user_timeline)
        public async Task<GetFriendicaPostsResults> GetNetworkNextAsync(double maxId, int count = 20)
        {
            if (_isSampleMode)
                return GetFriendicaPostsResults.SampleMode;

            _requestedUrl = String.Format("{0}/api/statuses/home_timeline.json?count={1}&max_id={2}&timestamp={3}",
                _serverBaseUrl,
                count,
                maxId,
                DateTime.Now.ToString());
            await this.GetStringAsync(_requestedUrl);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return GetFriendicaPostsResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return GetFriendicaPostsResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        try
                        {
                            Posts = JsonConvert.DeserializeObject<List<JsonFriendicaPost>>(ReturnString);
                            return GetFriendicaPostsResults.OK;
                        }
                        catch { return GetFriendicaPostsResults.SerializationError; }
                    }
                default:
                    return GetFriendicaPostsResults.UnexpectedError;
            }
        }


        // calls [GET] /api/statuses/home_timeline (= network posts and newsfeed items, home = user_timeline)
        public async Task<GetFriendicaPostsResults> GetNetworkNewAsync(double minId, int count = 999)
        {
            if (_isSampleMode)
                return GetFriendicaPostsResults.SampleMode;

            _requestedUrl = String.Format("{0}/api/statuses/home_timeline.json?count={1}&since_id={2}&timestamp={3}",
                _serverBaseUrl,
                count,
                minId,
                DateTime.Now.ToString());
            await this.GetStringAsync(_requestedUrl);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return GetFriendicaPostsResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return GetFriendicaPostsResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        try
                        {
                            Posts = JsonConvert.DeserializeObject<List<JsonFriendicaPost>>(ReturnString);
                            return GetFriendicaPostsResults.OK;
                        }
                        catch { return GetFriendicaPostsResults.SerializationError; }
                    }
                default:
                    return GetFriendicaPostsResults.UnexpectedError;
            }
        }


















        public enum PostFriendicaActivityResults { SampleMode, OK, NotAnswered, NotAuthenticated, UnexpectedError }


        // calls [POST] /api/friendica/activity
        public async Task<PostFriendicaActivityResults> PostFriendicaActivityAsync(double id, FriendicaActivity activity)
        {
            _requestedUrl = String.Format("{0}/api/friendica/activity/{1}?id={2}",
                                          _serverBaseUrl,
                                          activity,
                                          id);
            await this.PostStringAsync(_requestedUrl, "");
            
            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return PostFriendicaActivityResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return PostFriendicaActivityResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    return PostFriendicaActivityResults.OK;
                default:
                    return PostFriendicaActivityResults.UnexpectedError;
            }
        }




        






        private void ConvertReturnString()
        {
            //if (ReturnString != null)
            //{
            //    // convert the returned string into a list of objects
            //    try
            //    {
            //        _postsRaw = JsonConvert.DeserializeObject<List<JsonFriendicaPost>>(ReturnString);
            //    }
            //    catch
            //    {
            //        // TODO: issue with "User not found" resulting in 400 Bad Request not solved yet

            //        _postsRaw = new List<JsonFriendicaPost>();
            //    }

            //    foreach (var post in _postsRaw)
            //    {
            //        Posts.Add(new FriendicaPost(post));
            //    }
            //}
        }

    }
}
