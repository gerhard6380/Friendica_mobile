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
                // some links might not show the usual www.youtube.com/watch?v=... style, so we need to reformat them
                if (url.Contains("https://www.youtube.com/embed/"))
                {
                    var uri = new Uri(url, UriKind.RelativeOrAbsolute);
                    string video = "";
                    foreach (var part in uri.Segments)
                    {
                        if (part == "/" || part == "embed/")
                            continue;
                        else
                            video = part;
                    }
                    url = String.Format("https://www.youtube.com/watch?v={0}", video);
                }
                // url is already coded correctly
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
