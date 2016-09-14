using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    class GetFriendicaGroups : clsHttpRequests
    {
        public event EventHandler FriendicaGroupsLoaded;
        protected virtual void OnFriendicaGroupsLoaded()
        {
            if (FriendicaGroupsLoaded != null)
                FriendicaGroupsLoaded(this, EventArgs.Empty);
        }

        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public FriendicaGroup CreateGroup;
        public FriendicaGroup UpdateGroup;

        public GetFriendicaGroups()
        {
        }

        public async void GetFriendicaGroupsList()
        {
            var url = String.Format("{0}/api/friendica/group_show.json?timestamp={1}",
                App.Settings.FriendicaServer,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaGroupsList_RequestFinished;
            await this.GetString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword);
        }


        public async void FriendicaGroupDelete(double gid, string name)
        {
            var url = String.Format("{0}/api/friendica/group_delete.json?gid={1}&name={2}&timestamp={3}",
                App.Settings.FriendicaServer,
                gid.ToString(),
                name,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaGroupsList_RequestFinished;
            await this.DeleteString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword);
        }


        public void FriendicaGroupCreate(FriendicaGroup group)
        {
            CreateGroup = group;
            var content = group.Stringify();
            var url = String.Format("{0}/api/friendica/group_create.json?name={1}&timestamp={2}",
                App.Settings.FriendicaServer,
                group.GroupName,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaGroupsList_RequestFinished;
            this.PostString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword, content);
        }


        public void FriendicaGroupUpdate(FriendicaGroup group)
        {
            UpdateGroup = group;
            var content = group.Stringify();
            var url = String.Format("{0}/api/friendica/group_update.json?gid={1}&name={2}&timestamp={3}",
                App.Settings.FriendicaServer,
                group.GroupGid,
                group.GroupName,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaGroupsList_RequestFinished;
            this.PostString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword, content);
        }


        private void GetFriendicaGroupsList_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaGroupsLoaded();
        }

    }
}
