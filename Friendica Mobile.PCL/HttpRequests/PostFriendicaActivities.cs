using Friendica_Mobile.PCL.Viewmodels;
using System;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.HttpRequests
{
    public class PostFriendicaActivities : HttpRequestsBase
    {
        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public double PostId;
        public FriendicaActivity Activity;

        public PostFriendicaActivities()
        {
        }


        public async Task PostFriendicaActivityAsync(double id, FriendicaActivity activity)
        {
            PostId = id;
            Activity = activity;
            var url = String.Format("{0}/api/friendica/activity/{1}?id={2}",
                Settings.FriendicaServer, 
                Activity,
                PostId);
            await this.PostStringAsync(url, Settings.FriendicaUsername, Settings.FriendicaPassword, "");
            return;
        }
    }
}
