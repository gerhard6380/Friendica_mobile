using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    class GetFriendicaFriends : clsHttpRequests
    {
        public event EventHandler FriendicaFriendsLoaded;
        protected virtual void OnFriendicaFriendsLoaded()
        {
            if (FriendicaFriendsLoaded != null)
                FriendicaFriendsLoaded(this, EventArgs.Empty);
        }

        public GetFriendicaFriends()
        {
        }

        public async void GetFriendicaFriendsList()
        {
            var url = String.Format("{0}/api/statuses/friends.json?timestamp={1}",
                App.Settings.FriendicaServer,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaFriends_RequestFinished;
            await this.GetString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword);

        }


        private void GetFriendicaFriends_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaFriendsLoaded();
        }
    }
}
