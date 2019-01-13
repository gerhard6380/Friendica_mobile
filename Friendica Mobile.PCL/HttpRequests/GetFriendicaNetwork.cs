using Friendica_Mobile.PCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.HttpRequests
{
    class GetFriendicaNetwork : HttpRequestsBase
    {
        // containing the raw returns from server converted into classes
        private List<JsonFriendicaPost> _postsRaw;
        // containing the extended classes (commands, events, derived from BindableClass to enable XAML binding)
        public List<FriendicaPost> Posts;

        public GetFriendicaNetwork()
        {
            _postsRaw = new List<JsonFriendicaPost>();
            Posts = new List<FriendicaPost>();
        }


        // method to retrieve the first posts from server (default = 20)
        public async Task GetNetworkInitialAsync(int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&timestamp={2}",
                Settings.FriendicaServer,
                count,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            ConvertReturnString();
            return;
        }

        
        // method to retrieve the next group of posts from server (default = next 20)
        public async Task GetNetworkNextAsync(double maxId, int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&max_id={2}&timestamp={3}",
                Settings.FriendicaServer,
                count,
                maxId,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            ConvertReturnString();
            return;
        }


        // method to retrieve newer posts than already retrieved (if available)
        public async Task GetNetworkNewAsync(double minId, int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&since_id={2}&timestamp={3}",
                Settings.FriendicaServer,
                count,
                minId,
                DateTime.Now.ToString());
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            ConvertReturnString();
            return;
        }


        public async Task GetFriendicaThreadByIdAsync(double id)
        {
            var url = String.Format("{0}/api/statuses/show.json?id={1}&conversation=1",
                Settings.FriendicaServer,
                id);
            await this.GetStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword);
            ConvertReturnString();
            return;
        }


        private void ConvertReturnString()
        {
            if (ReturnString != null)
            {
                // convert the returned string into a list of objects
                try
                {
                    _postsRaw = JsonConvert.DeserializeObject<List<JsonFriendicaPost>>(ReturnString);
                }
                catch
                {
                    // TODO: issue with "User not found" resulting in 400 Bad Request not solved yet

                    _postsRaw = new List<JsonFriendicaPost>();
                }

                foreach (var post in _postsRaw)
                {
                    Posts.Add(new FriendicaPost(post));
                }
            }
        }

    }
}
