using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Friendica_Mobile.HttpRequests
{
    public class PostFriendicaActivities : clsHttpRequests
    {
        public enum FriendicaActivity { like, dislike, unlike, undislike };

        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public event EventHandler FriendicaActivitySent;
        protected virtual void OnFriendicaActivitySent()
        {
            if (FriendicaActivitySent != null)
                FriendicaActivitySent(this, EventArgs.Empty);
        }

        // Save submitted data into this class so we can access them in case of an error returned from Friendica server (see ManageGroupViewmodel.cs)
        public double PostId;
        public FriendicaActivity Activity;

        public PostFriendicaActivities()
        {
        }


        public void PostFriendicaActivity(double id, FriendicaActivity activity)
        {
            PostId = id;
            Activity = activity;
            var url = String.Format("{0}/api/friendica/activity/{1}?id={2}",
                App.Settings.FriendicaServer, 
                Activity,
                PostId);
            this.RequestFinished += PostFriendicaActivity_RequestFinished;
            this.PostString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword, "");
        }


        private void PostFriendicaActivity_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaActivitySent();
        }

    }
}
