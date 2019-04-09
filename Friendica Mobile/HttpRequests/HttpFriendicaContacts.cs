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

        private FriendicaGroupResult _groupResult;
        public FriendicaGroupResult GroupResult
        {
            get { return _groupResult; }
            set { _groupResult = value; }
        }

        private string _errorGroupDelete;
        public string ErrorGroupDelete
        {
            get { return _errorGroupDelete; }
            set { _errorGroupDelete = value; }
        }

        // constructors (passing login data to the underlying base class
        public HttpFriendicaContacts(string baseUrlServer, string username, string password) : base(baseUrlServer, username, password)
        {    
            if (_isSampleMode)
            {
                Friends = SampleData.ContactsFriendsSamples().OrderBy(u => u.UserName).ToList();
                Forums = SampleData.ContactsForumsSamples().OrderBy(f => f.UserName).ToList();
                Groups = SampleData.ContactsGroupsSamples().OrderBy(g => g.GroupName).ToList();
            }
        }

        public HttpFriendicaContacts(string baseUrlServer, string token) : base(baseUrlServer, token)
        {
            if (_isSampleMode)
            {
                Friends = SampleData.ContactsFriendsSamples().OrderBy(u => u.UserName).ToList();
                Forums = SampleData.ContactsForumsSamples().OrderBy(f => f.UserName).ToList();
                Groups = SampleData.ContactsGroupsSamples().OrderBy(g => g.GroupName).ToList();
            }
        }


        public enum GetFriendicaFriendsResults { OK, NotAnswered, NotAuthenticated, NotImplemented, SerializationError, UnexpectedError }
        public enum PostFriendicaGroupsResults { OK, OkReactivatedGroup, NotAnswered, NotAuthenticated, NotImplemented, GroupAlreadyExisting, SerializationError, UnexpectedError }
        public enum DeleteFriendicaGroupResults { OK, NotAnswered, NotAuthenticated, ErrorReceived, NotImplemented, UnexpectedError }

        // calls [GET] /api/statuses/friends
        public async Task<GetFriendicaFriendsResults> GetStatusesFriends()
        {
            _requestedUrl = String.Format("{0}/api/statuses/friends?count=1000000", _serverBaseUrl);
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
                            var groups = JsonConvert.DeserializeObject<List<FriendicaGroup>>(ReturnString);
                            Groups = groups.OrderBy(g => g.GroupName).ToList();
                            return GetFriendicaFriendsResults.OK;
                        }
                        catch { return GetFriendicaFriendsResults.SerializationError; }
                    }
                default:
                    return GetFriendicaFriendsResults.UnexpectedError;
            }
        }


        // calls [POST] /api/friendica/group_create
        public async Task<PostFriendicaGroupsResults> PostFriendicaGroupCreateAsync(FriendicaGroup group)
        {
            var content = JsonConvert.SerializeObject(group);
            _requestedUrl = String.Format("{0}/api/friendica/group_create?name={1}", _serverBaseUrl, group.GroupName);
            await this.PostStringAsync(_requestedUrl, content);
            return AnalyseGroupsResults();
        }


        // calls [POST] /api/friendica/group_update
        public async Task<PostFriendicaGroupsResults> PostFriendicaGroupUpdateAsync(FriendicaGroup group)
        {
            var content = JsonConvert.SerializeObject(group);
            _requestedUrl = String.Format("{0}/api/friendica/group_update?gid={1}&name={2}", _serverBaseUrl, group.GroupGid, group.GroupName);
            await this.PostStringAsync(_requestedUrl, content);
            return AnalyseGroupsResults();
        }

        private PostFriendicaGroupsResults AnalyseGroupsResults()
        {
            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return PostFriendicaGroupsResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return PostFriendicaGroupsResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        // showing hint if result not possible due to old server version
                        if (ReturnString.Contains("error") && ReturnString.Contains("not implemented"))
                            return PostFriendicaGroupsResults.NotImplemented;
                        if (ReturnString.Contains("error") && ReturnString.Contains("group name already exists"))
                            return PostFriendicaGroupsResults.GroupAlreadyExisting;
                        try
                        {
                            GroupResult = JsonConvert.DeserializeObject<FriendicaGroupResult>(ReturnString);
                            if (GroupResult.GroupStatus == "reactivated")
                                return PostFriendicaGroupsResults.OkReactivatedGroup;
                            return PostFriendicaGroupsResults.OK;
                        }
                        catch { return PostFriendicaGroupsResults.SerializationError; }
                    }
                default:
                    return PostFriendicaGroupsResults.UnexpectedError;
            }
        }

        // calls [DELETE] /api/friendica/group_delete
        public async Task<DeleteFriendicaGroupResults> DeleteFriendicaGroupDeleteAsync(int gid, string name)
        {
            _requestedUrl = String.Format("{0}/api/friendica/group_delete.json?gid={1}&name={2}", _serverBaseUrl, gid.ToString(), name);
            await this.DeleteStringAsync(_requestedUrl);

            switch (StatusCode)
            {
                // include case 0 as non-existing servers could return 0 instead of BadGateway/NotFound
                case 0:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.NotFound:
                    return DeleteFriendicaGroupResults.NotAnswered;
                case System.Net.HttpStatusCode.Forbidden:
                case System.Net.HttpStatusCode.Unauthorized:
                    return DeleteFriendicaGroupResults.NotAuthenticated;
                case System.Net.HttpStatusCode.OK:
                    {
                        // showing hint if result not possible due to old server version
                        if (ReturnString.Contains("error") && ReturnString.Contains("not implemented"))
                            return DeleteFriendicaGroupResults.NotImplemented;

                        // return errormessage if other results occurred
                        if (ReturnString.Contains("error"))
                        {
                            if (ReturnString.Contains("gid or name not specified"))
                                ErrorGroupDelete = "gid or name not specified";
                            else if (ReturnString.Contains("gid not available"))
                                ErrorGroupDelete = "gid not available";
                            else if (ReturnString.Contains("wrong group name"))
                                ErrorGroupDelete = "wrong group name";
                            else if (ReturnString.Contains("other API error"))
                                ErrorGroupDelete = "other API error";
                            return DeleteFriendicaGroupResults.ErrorReceived;
                        }
                        return DeleteFriendicaGroupResults.OK;
                    }
                default:
                    return DeleteFriendicaGroupResults.UnexpectedError;
            }
        }
    }
}
