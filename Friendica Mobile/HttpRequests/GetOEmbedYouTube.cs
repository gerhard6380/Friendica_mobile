using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.HttpRequests
{
    class GetOEmbedYoutube : clsHttpRequests
    {
        public event EventHandler OEmbedYoutubeLoaded;
        protected virtual void OnOEmbedYoutubeLoaded()
        {
            if (OEmbedYoutubeLoaded != null)
                OEmbedYoutubeLoaded(this, EventArgs.Empty);
        }
        
        public GetOEmbedYoutube()
        {

        }

        public async Task<OEmbedYoutube> GetOEmbedYoutubeByUrl(string url)
        {
            if (url != null)
            {
                url = System.Net.WebUtility.UrlEncode(url);  
                var urlEmbed = string.Format("http://www.youtube.com/oembed?url={0}&format=json", url);
                var resultString = await this.GetStringWithoutCredentials(urlEmbed);
                var oEmbed = new OEmbedYoutube(resultString);
                RequestFinished += GetOEmbedYoutube_RequestFinished;
                return oEmbed;
            }
            else
                return null;
        }

        private void GetOEmbedYoutube_RequestFinished(object sender, EventArgs e)
        {
            OnOEmbedYoutubeLoaded();
        }
    }
}
