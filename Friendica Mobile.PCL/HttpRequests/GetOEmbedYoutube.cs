using Friendica_Mobile.PCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.HttpRequests
{
    class GetOEmbedYoutube : HttpRequestsBase
    {
        // containing the raw returns from server converted into classes
        private JsonOEmbedYoutube _oembed;
     
        public GetOEmbedYoutube()
        {
        }


        // method to retrieve the first posts from server (default = 20)
        public async Task<JsonOEmbedYoutube> GetOEmbedYoutubeByUrlAsync(string url)
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
                url = WebUtility.UrlEncode(url);
                var urlEmbed = string.Format("http://www.youtube.com/oembed?url={0}&format=json", url);
                var resultString = await this.GetStringWithoutCredentials(urlEmbed);
                ConvertReturnString();
                return _oembed;
            }
            else
                return null;
        }

       
        private void ConvertReturnString()
        {
            if (ReturnString != null)
            {
                // convert the returned string into a list of objects
                try
                {
                    _oembed = JsonConvert.DeserializeObject<JsonOEmbedYoutube>(ReturnString);
                }
                catch (JsonReaderException)
                {
                    _oembed = null;
                }
            }
        }

    }
}
