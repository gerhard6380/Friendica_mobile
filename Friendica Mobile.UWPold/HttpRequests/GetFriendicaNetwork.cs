using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.UWP.HttpRequests
{
    class GetFriendicaNetwork : clsHttpRequests
    {
        AppSettings appSettings = new AppSettings();

        public event EventHandler FriendicaNetworkLoaded;
        protected virtual void OnFriendicaNetworkLoaded()
        {
            if (FriendicaNetworkLoaded != null)
                FriendicaNetworkLoaded(this, EventArgs.Empty);
        }

        public GetFriendicaNetwork()
        {
        }

        public async void GetFriendicaNetworkInitial(int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&timestamp={2}",
                appSettings.FriendicaServer,
                count,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }

        public async void GetFriendicaNetworkNextEntries(double maxId, int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&max_id={2}&timestamp={3}",
                appSettings.FriendicaServer,
                count,
                maxId,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }

        public async void GetFriendicaNetworkNewEntries(double minId, int count)
        {
            var url = String.Format("{0}/api/statuses/home_timeline.json?count={1}&since_id={2}&timestamp={3}",
                appSettings.FriendicaServer,
                count,
                minId,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }
        
        //public async Task<ObservableCollection<FriendicaPostExtended>> GetFriendicaThreadOfPostAsync(double id)
        //{
        //    var url = String.Format("{0}/api/statuses/show.json?id={1}&conversation=1&timestamp={2}",
        //        appSettings.FriendicaServer,
        //        id,
        //        DateTime.Now.ToString());
        //    this.RequestFinished += GetFriendicaNetwork_RequestFinished;
        //    Task<string> getStringTask = this.GetStringAsync(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        //    string result = await getStringTask;
        //    var obscoll = ConvertJsonToObsColl(result);
        //    return obscoll;
        //}


        private void GetFriendicaNetwork_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaNetworkLoaded();
        }

    }
}
