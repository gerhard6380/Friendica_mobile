using BackgroundTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Friendica_Mobile.Models
{
    public class FriendicaPhotolist : FriendicaBaseModel
    {
        // API interface json fields
        private const string photolistIdKey = "id";
        private const string photolistAlbumKey = "album";
        private const string photolistFilenameKey = "filename";
        private const string photolistTypeKey = "type";
        private const string photolistThumbKey = "thumb";


        // properties from API return (from JSON)
        private string _photolistId;
        public string PhotolistId
        {
            get { return _photolistId; }
            set { _photolistId = value; }
        }

        private string _photolistAlbum;
        public string PhotolistAlbum
        {
            get { return _photolistAlbum; }
            set { _photolistAlbum = value; }
        }

        private string _photolistFilename;
        public string PhotolistFilename
        {
            get { return _photolistFilename; }
            set { _photolistFilename = value; }
        }

        private string _photolistType;
        public string PhotolistType
        {
            get { return _photolistType; }
            set { _photolistType = value; }
        }

        private string _photolistThumb;
        public string PhotolistThumb
        {
            get { return _photolistThumb; }
            set { _photolistThumb = value; }
        }


        // class constructors

        public FriendicaPhotolist()
        {
            PhotolistId = "";
            PhotolistAlbum = "";
            PhotolistFilename = "";
            PhotolistType = "";
            PhotolistThumb = "";
        }


        public FriendicaPhotolist(string jsonString) : this()
            {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            PhotolistId = (string)CheckAttribute(jsonObject, photolistIdKey, AttributeTypes.String);
            PhotolistAlbum = (string)CheckAttribute(jsonObject, photolistAlbumKey, AttributeTypes.String);
            PhotolistFilename = (string)CheckAttribute(jsonObject, photolistFilenameKey, AttributeTypes.String);
            PhotolistType = (string)CheckAttribute(jsonObject, photolistTypeKey, AttributeTypes.String);
            PhotolistThumb = (string)CheckAttribute(jsonObject, photolistThumbKey, AttributeTypes.String);
        }


        public FriendicaPhotolist(JsonObject jsonObject) : this()
        {
            PhotolistId = (string)CheckAttribute(jsonObject, photolistIdKey, AttributeTypes.String);
            PhotolistAlbum = (string)CheckAttribute(jsonObject, photolistAlbumKey, AttributeTypes.String);
            PhotolistFilename = (string)CheckAttribute(jsonObject, photolistFilenameKey, AttributeTypes.String);
            PhotolistType = (string)CheckAttribute(jsonObject, photolistTypeKey, AttributeTypes.String);
            PhotolistThumb = (string)CheckAttribute(jsonObject, photolistThumbKey, AttributeTypes.String);
        }

    }
}
