using Friendica_Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    public class HttpFriendicaContacts : HttpRequestsBase
    {
        private List<FriendicaUser> _friends;
        public List<FriendicaUser> Friends
        {
            get { return _friends; }
            set { _friends = value; }
        }

        private List<FriendicaUser> _forums;
        public List<FriendicaUser> Forums
        {
            get { return _forums; }
            set { _forums = value; }
        }

        private List<FriendicaGroup> _groups;
        public List<FriendicaGroup> Groups
        {
            get { return _groups; }
            set { _groups = value; }
        }


        // constructors (passing login data to the underlying base class
        public HttpFriendicaContacts(string baseUrlServer, string username, string password) : base(baseUrlServer, username, password)
        {    
            if (_isSampleMode)
            {
                // TODO: prepare sample data
            }
        }

        public HttpFriendicaContacts(string baseUrlServer, string token) : base(baseUrlServer, token)
        {
            if (_isSampleMode)
            {
                // TODO: prepare sample data
            }
        }


        public enum GetFriendicaFriendsResults { OK, NotAnswered, NotAuthenticated, NotImplemented, SerializationError, UnexpectedError }

        // calls [GET] /api/statuses/friends
        public async Task<GetFriendicaFriendsResults> GetStatusesFriends()
        {
            _requestedUrl = String.Format("{0}/api/statuses/friends", _serverBaseUrl);
            await this.GetStringAsync(_requestedUrl);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return GetFriendicaFriendsResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return GetFriendicaFriendsResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        // showing hint if result not possible due to old server version
                        if (ReturnString.Contains("error") && ReturnString.Contains("not implemented"))
                            return GetFriendicaFriendsResults.NotImplemented;
                        try 
                        {
                            var friends = JsonConvert.DeserializeObject<List<FriendicaUser>>(ReturnString);
                            Friends = friends.FindAll(u => u.UserFollowing).OrderBy(u => u.UserName).ToList();
                            Forums = friends.FindAll(u => !u.UserFollowing).OrderBy(u => u.UserName).ToList();
                            return GetFriendicaFriendsResults.OK;
                        }
                        catch { return GetFriendicaFriendsResults.SerializationError; }
                    }
                default:
                        return GetFriendicaFriendsResults.UnexpectedError;
            }
        }


        // calls [GET] /api/friendica/group_show
        public async Task<GetFriendicaFriendsResults> GetFriendicaGroupShow()
        {
            _requestedUrl = String.Format("{0}/api/friendica/group_show", _serverBaseUrl);
            await this.GetStringAsync(_requestedUrl);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return GetFriendicaFriendsResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return GetFriendicaFriendsResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        // showing hint if result not possible due to old server version
                        if (ReturnString.Contains("error") && ReturnString.Contains("not implemented"))
                            return GetFriendicaFriendsResults.NotImplemented;
                        try
                        {
                            Groups = JsonConvert.DeserializeObject<List<FriendicaGroup>>(ReturnString);
                            return GetFriendicaFriendsResults.OK;
                        }
                        catch { return GetFriendicaFriendsResults.SerializationError; }
                    }
                default:
                    return GetFriendicaFriendsResults.UnexpectedError;
            }
        }


    }
}
