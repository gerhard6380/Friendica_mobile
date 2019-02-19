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
    class GetFriendicaThread : clsHttpRequests
    {
        public event EventHandler FriendicaThreadLoaded;
        protected virtual void OnFriendicaThreadLoaded()
        {
            if (FriendicaThreadLoaded != null)
                FriendicaThreadLoaded(this, EventArgs.Empty);
        }

        public GetFriendicaThread()
        {
        }

        public async void GetFriendicaThreadById(double id)
        {
            var url = String.Format("{0}/api/statuses/show.json?id={1}&conversation=1",
                App.Settings.FriendicaServer,
                id);
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
            await this.GetString(url, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword);
        }

        public async Task<ObservableCollection<FriendicaThread>> GetFriendicaThreadByIdAsync(double id, string username, string password)
        {
            var url = String.Format("{0}/api/statuses/show.json?id={1}&conversation=1",
                App.Settings.FriendicaServer,
                id);
            this.RequestFinished += GetFriendicaNetwork_RequestFinished;
            Task<string> getStringTask = this.GetStringAsync(url, username, password);
            string result = await getStringTask;
            var obscoll = ConvertJsonToObsColl(result);
            var thread = new FriendicaThread();
            thread.Posts = obscoll;

            var obscollThread = new ObservableCollection<FriendicaThread>();
            obscollThread.Add(thread);
            return obscollThread;
        }

        private void GetFriendicaNetwork_RequestFinished(object sender, EventArgs e)
        {
            OnFriendicaThreadLoaded();
        }


        private ObservableCollection<FriendicaPostExtended> ConvertJsonToObsColl(string returnString)
        {
            var obscoll = new ObservableCollection<FriendicaPostExtended>();

            JsonArray resultArray = JsonArray.Parse(returnString);
            int arraySize = resultArray.Count;
            for (int i = 0; i < arraySize; i++)
            {
                IJsonValue element = resultArray.GetArray()[i];
                switch (element.ValueType)
                {
                    case JsonValueType.Object:
                        var result = new FriendicaPostExtended(element.ToString());
                        obscoll.Add(result);
                        break;
                }
            }
            return obscoll;
        }

    }
}
