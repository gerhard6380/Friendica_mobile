namespace Friendica_Mobile.PCL.Models
{
    public sealed class FriendicaMessageNew
    {
        // used to post a new private message to another friend - currently not possible to post private images

        private enum AttributeTypes { String, Number, Boolean, FriendicaPost, FriendicaUser, FriendicaGeo };

        private const string newMessageUserIdKey = "user_id";
        private const string newMessageTextKey = "text";
        private const string newMessageReplyToKey = "replyto";
        private const string newMessageTitleKey = "title";

        private string _newMessageUserUrl;
        private string _newMessageText;
        private string _newMessageReplyTo;
        private string _newMessageTitle;
        //private byte[] _newMessageMedia;

        public FriendicaMessageNew()
        {
            NewMessageUserUrl = "";
            NewMessageText = "";
            NewMessageReplyTo = "";
            NewMessageTitle = "";
            //NewMessageMedia = null;
        }


        public string NewMessageUserUrl
        {
            get { return _newMessageUserUrl; }
            set { _newMessageUserUrl = value; }
        }

        public string NewMessageText
        {
            get { return _newMessageText; }
            set { _newMessageText = value; }
        }

        public string NewMessageReplyTo
        {
            get { return _newMessageReplyTo; }
            set { _newMessageReplyTo = value; }
        }

        public string NewMessageTitle
        {
            get { return _newMessageTitle; }
            set { _newMessageTitle = value; }
        }

        //public byte[] NewMessageMedia
        //{
        //    get { return _newMessageMedia; }
        //    set { _newMessageMedia = value; }
        //}

    }
}
