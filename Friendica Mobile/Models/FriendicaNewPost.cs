using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage.Streams;

namespace Friendica_Mobile.Models
{

    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaNewPost
    {
        public enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser, FriendicaGeo };

        private const string newPostTitleKey = "title";
        private const string newPostStatusKey = "status";
        private const string newPostHtmlStatusKey = "htmlstatus";
        private const string newPostInReplyToStatusIdKey = "in_reply_to_status_id";
        private const string newPostLatitudeKey = "lat";
        private const string newPostLongitudeKey = "long";
        private const string newPostMediaKey = "media";
        private const string newPostSourceKey = "source";
        private const string newPostGroupAllowKey = "group_allow";
        private const string newPostContactAllowKey = "contact_allow";
        private const string newPostGroupDenyKey = "group_deny";
        private const string newPostContactDenyKey = "contact_deny";
        private const string newPostNetworkKey = "network";

        private string _newPostTitle;
        private string _newPostStatus;
        private string _newPostHtmlStatus;
        private double _newPostInReplyToStatusId;
        private double _newPostLatitude;
        private double _newPostLongitude;
        private byte[] _newPostMedia;
        private string _newPostSource;
        private string _newPostGroupAllow;
        private string _newPostContactAllow;
        private string _newPostGroupDeny;
        private string _newPostContactDeny;
        private string _newPostNetwork;

        public FriendicaNewPost()
        {
            NewPostTitle = "";
            NewPostStatus = "";
            NewPostHtmlStatus = "";
            NewPostInReplyToStatusId = 0;
            NewPostLatitude = 0;
            NewPostLongitude = 0;
            NewPostMedia = null;
            NewPostSource = "";
            NewPostGroupAllow = "";
            NewPostContactAllow = "";
            NewPostGroupDeny = "";
            NewPostContactDeny = "";
            NewPostNetwork = "";
        }

        private object CheckAttribute(JsonObject jsonObject, string key, AttributeTypes type)
        {
            IJsonValue value;
            jsonObject.TryGetValue(key, out value);
            switch (type)
            {
                case AttributeTypes.String:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedString(key, "");
                case AttributeTypes.Number:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return 0.0;
                    else
                        return jsonObject.GetNamedNumber(key, 0);
                case AttributeTypes.Boolean:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return jsonObject.GetNamedBoolean(key, false);
                case AttributeTypes.FriendicaPost:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaPost(jsonObject.GetNamedObject(key));
                case AttributeTypes.FriendicaGeo:
                    if (value == null || value.ValueType == JsonValueType.Null)
                        return null;
                    else
                        return new FriendicaGeo(jsonObject.GetNamedObject(key));
                default:
                    return null;
            } 
        }

        public string NewPostTitle
        {
            get { return _newPostTitle; }
            set { _newPostTitle = value; }
        }

        public string NewPostStatus
        {
            get { return _newPostStatus; }
            set { _newPostStatus = value; }
        }

        public string NewPostHtmlStatus
        {
            get { return _newPostHtmlStatus; }
            set { _newPostHtmlStatus = value; }
        }

        public double NewPostInReplyToStatusId
        {
            get { return _newPostInReplyToStatusId; }
            set { _newPostInReplyToStatusId = value; }
        }

        public double NewPostLatitude
        {
            get { return _newPostLatitude; }
            set { _newPostLatitude = value; }
        }

        public double NewPostLongitude
        {
            get { return _newPostLongitude; }
            set { _newPostLongitude = value; }
        }

        public byte[] NewPostMedia
        {
            get { return _newPostMedia; }
            set { _newPostMedia = value; }
        }

        public string NewPostSource
        {
            get { return _newPostSource; }
            set { _newPostSource = value; }
        }

        public string NewPostGroupAllow
        {
            get { return _newPostGroupAllow; }
            set { _newPostGroupAllow = value; }
        }

        public string NewPostContactAllow
        {
            get { return _newPostContactAllow; }
            set { _newPostContactAllow = value; }
        }

        public string NewPostGroupDeny
        {
            get { return _newPostGroupDeny; }
            set { _newPostGroupDeny = value; }
        }

        public string NewPostContactDeny
        {
            get { return _newPostContactDeny; }
            set { _newPostContactDeny = value; }
        }

        public string NewPostNetwork
        {
            get { return _newPostNetwork; }
            set { _newPostNetwork = value; }
        }

        private IBuffer _newPostMediaBuffer;
        public IBuffer NewPostMediaBuffer
        {
            get { return _newPostMediaBuffer; }
            set { _newPostMediaBuffer = value; }
        }
    }
}
