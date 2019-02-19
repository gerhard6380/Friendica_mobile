using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.UWP.HttpRequests
{
    class GetFriendicaConfig : clsHttpRequests
    {
        public event EventHandler FriendicaConfigLoaded;
        protected virtual void OnFriendicaConfigLoaded()
        {
            if (FriendicaConfigLoaded != null)
                FriendicaConfigLoaded(this, EventArgs.Empty);
        }

        public GetFriendicaConfig()
        {
        }

        public async void LoadConfig()
        {
            var url = String.Format("{0}/api/statusnet/config.json?timestamp={1}",
                App.Settings.FriendicaServer,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaConfig_RequestFinished;
            await this.GetString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword);
        }

        private void GetFriendicaConfig_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaConfigLoaded();
        }
    }
}
