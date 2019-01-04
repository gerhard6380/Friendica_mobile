using System.Collections.Generic;

namespace Friendica_Mobile.Models
{
    // class for holding the contents of each live tile for creating the necessary xml document
    public class LiveTileContent
    {
        // id of the post used as argument in live tile
        public string Id { get; set; }

        // url of the image of the rss feed, used as background image on medium tiles and inline image on wide or large tiles
        public string UserProfileImageSource { get; set; }

        // if the post contains an image, we can use this as a peek image flipping with the content
        public string PeekImageSource { get; set; }

        // if available we can use a title shown in white text
        public string Title { get; set; }

        // content to be shown in gray text, array to enable posting different paragraphs
        public List<string> Content { get; set; }
    }
}
