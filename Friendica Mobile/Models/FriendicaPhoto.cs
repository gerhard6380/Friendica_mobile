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
    public class FriendicaPhoto : FriendicaBaseModel
    {
        // API interface json fields
        private const string photoIdKey = "id";
        private const string photoCreatedKey = "created";
        private const string photoEditedKey = "edited";
        private const string photoTitleKey = "title";
        private const string photoDescKey = "desc";
        private const string photoAlbumKey = "album";
        private const string photoFilenameKey = "filename";
        private const string photoTypeKey = "type";
        private const string photoHeightKey = "height";
        private const string photoWidthKey = "width";
        private const string photoProfileKey = "profile";
        private const string photoAllowCidKey = "allow_cid";
        private const string photoDenyCidKey = "deny_cid";
        private const string photoAllowGidKey = "allow_gid";
        private const string photoDenyGidKey = "deny_gid";
        private const string photoLinkKey = "link";
        private const string photoDataKey = "data";


        // properties from API return (from JSON)
        private string _photoId;
        public string PhotoId
        {
            get { return _photoId; }
            set { _photoId = value; }
        }

        private string _photoCreated;
        public string PhotoCreated
        {
            get { return _photoCreated; }
            set { _photoCreated = value; }
        }

        private string _photoEdited;
        public string PhotoEdited
        {
            get { return _photoEdited; }
            set { _photoEdited = value; }
        }

        private string _photoTitle;
        public string PhotoTitle
        {
            get { return _photoTitle; }
            set { _photoTitle = value; }
        }

        private string _photoDesc;
        public string PhotoDesc
        {
            get { return _photoDesc; }
            set { _photoDesc = value; }
        }

        private string _photoAlbum;
        public string PhotoAlbum
        {
            get { return _photoAlbum; }
            set { _photoAlbum = value; }
        }

        private string _photoFilename;
        public string PhotoFilename
        {
            get { return _photoFilename; }
            set { _photoFilename = value; }
        }

        private string _photoType;
        public string PhotoType
        {
            get { return _photoType; }
            set { _photoType = value; }
        }

        private string _photoHeight;
        public string PhotoHeight
        {
            get { return _photoHeight; }
            set { _photoHeight = value; }
        }

        private string _photoWidth;
        public string PhotoWidth
        {
            get { return _photoWidth; }
            set { _photoWidth = value; }
        }

        private string _photoProfile;
        public string PhotoProfile
        {
            get { return _photoProfile; }
            set { _photoProfile = value; }
        }

        private string _photoAllowCid;
        public string PhotoAllowCid
        {
            get { return _photoAllowCid; }
            set { _photoAllowCid = value; }
        }

        private string _photoDenyCid;
        public string PhotoDenyCid
        {
            get { return _photoDenyCid; }
            set { _photoDenyCid = value; }
        }

        private string _photoAllowGid;
        public string PhotoAllowGid
        {
            get { return _photoAllowGid; }
            set { _photoAllowGid = value; }
        }

        private string _photoDenyGid;
        public string PhotoDenyGid
        {
            get { return _photoDenyGid; }
            set { _photoDenyGid = value; }
        }

        private List<string> _photoLink;
        public List<string> PhotoLink
        {
            get { return _photoLink; }
            set { _photoLink = value; }
        }

        private string _photoData;
        public string PhotoData
        {
            get { return _photoData; }
            set { _photoData = value; }
        }


        // other derived properties from API return


        // TODO: created as Date, edited as Date, Height as int, width as int, profile as int?

        #region ClassConstructors

        public FriendicaPhoto()
        {
            PhotoId = "";
            PhotoCreated = "";
            PhotoEdited = "";
            PhotoTitle = "";
            PhotoDesc = "";
            PhotoAlbum = "";
            PhotoFilename = "";
            PhotoType = "";
            PhotoHeight = "";
            PhotoWidth = "";
            PhotoProfile = "";
            PhotoAllowCid = "";
            PhotoDenyCid = "";
            PhotoAllowGid = "";
            PhotoDenyGid = "";
            PhotoLink = new List<string>();
            PhotoData = "";
        }


        public FriendicaPhoto(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            PhotoId = (string)CheckAttribute(jsonObject, photoIdKey, AttributeTypes.String);
            PhotoCreated = (string)CheckAttribute(jsonObject, photoCreatedKey, AttributeTypes.String);
            PhotoEdited = (string)CheckAttribute(jsonObject, photoEditedKey, AttributeTypes.String);
            PhotoTitle = (string)CheckAttribute(jsonObject, photoTitleKey, AttributeTypes.String);
            PhotoDesc = (string)CheckAttribute(jsonObject, photoDescKey, AttributeTypes.String);
            PhotoAlbum = (string)CheckAttribute(jsonObject, photoAlbumKey, AttributeTypes.String);
            PhotoFilename = (string)CheckAttribute(jsonObject, photoFilenameKey, AttributeTypes.String);
            PhotoType = (string)CheckAttribute(jsonObject, photoTypeKey, AttributeTypes.String);
            PhotoHeight = (string)CheckAttribute(jsonObject, photoHeightKey, AttributeTypes.String);
            PhotoWidth = (string)CheckAttribute(jsonObject, photoWidthKey, AttributeTypes.String);
            PhotoProfile = (string)CheckAttribute(jsonObject, photoProfileKey, AttributeTypes.String);
            PhotoAllowCid = (string)CheckAttribute(jsonObject, photoAllowCidKey, AttributeTypes.String);
            PhotoDenyCid = (string)CheckAttribute(jsonObject, photoDenyCidKey, AttributeTypes.String);
            PhotoAllowGid = (string)CheckAttribute(jsonObject, photoAllowGidKey, AttributeTypes.String);
            PhotoDenyGid = (string)CheckAttribute(jsonObject, photoDenyCidKey, AttributeTypes.String);
            PhotoLink = (List<string>)CheckAttribute(jsonObject, photoLinkKey, AttributeTypes.StringArray);
            PhotoData = (string)CheckAttribute(jsonObject, photoDataKey, AttributeTypes.String);
        }


        public FriendicaPhoto(JsonObject jsonObject) : this()
        {
            PhotoId = (string)CheckAttribute(jsonObject, photoIdKey, AttributeTypes.String);
            PhotoCreated = (string)CheckAttribute(jsonObject, photoCreatedKey, AttributeTypes.String);
            PhotoEdited = (string)CheckAttribute(jsonObject, photoEditedKey, AttributeTypes.String);
            PhotoTitle = (string)CheckAttribute(jsonObject, photoTitleKey, AttributeTypes.String);
            PhotoDesc = (string)CheckAttribute(jsonObject, photoDescKey, AttributeTypes.String);
            PhotoAlbum = (string)CheckAttribute(jsonObject, photoAlbumKey, AttributeTypes.String);
            PhotoFilename = (string)CheckAttribute(jsonObject, photoFilenameKey, AttributeTypes.String);
            PhotoType = (string)CheckAttribute(jsonObject, photoTypeKey, AttributeTypes.String);
            PhotoHeight = (string)CheckAttribute(jsonObject, photoHeightKey, AttributeTypes.String);
            PhotoWidth = (string)CheckAttribute(jsonObject, photoWidthKey, AttributeTypes.String);
            PhotoProfile = (string)CheckAttribute(jsonObject, photoProfileKey, AttributeTypes.String);
            PhotoAllowCid = (string)CheckAttribute(jsonObject, photoAllowCidKey, AttributeTypes.String);
            PhotoDenyCid = (string)CheckAttribute(jsonObject, photoDenyCidKey, AttributeTypes.String);
            PhotoAllowGid = (string)CheckAttribute(jsonObject, photoAllowGidKey, AttributeTypes.String);
            PhotoDenyGid = (string)CheckAttribute(jsonObject, photoDenyCidKey, AttributeTypes.String);
            PhotoLink = (List<string>)CheckAttribute(jsonObject, photoLinkKey, AttributeTypes.StringArray);
            PhotoData = (string)CheckAttribute(jsonObject, photoDataKey, AttributeTypes.String);
        }

        #endregion

    }
}