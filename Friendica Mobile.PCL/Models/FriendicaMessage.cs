using Friendica_Mobile.PCL.Viewmodels;
using System;

namespace Friendica_Mobile.PCL.Models
{
    public class FriendicaMessage : BindableClass
    {
        // property containing the original message returned from server
        private JsonFriendicaMessage _message;
        public JsonFriendicaMessage Message
        {
            get { return _message; }
            set { _message = value;
                // set int version of the id
                try { MessageIdInt = Convert.ToInt16(_message.MessageId); } 
                    catch (FormatException) { MessageIdInt = 0; }

                // set MessageParentUri
                MessageParentUri = _message.MessageParentUri;

                // set indicator if we have sent or received the message
                SetIsReceivedMessage(_message.MessageSenderScreenName, _message.MessageRecipientScreenName);
                
                // create a compact header text for the search mode
                HeaderForSearchMode = _message.MessageSenderScreenName + ": " + _message.MessageTitle;

                // create a DateTime version of the created-At-Date
                MessageCreatedAtDateTime = DateTime.ParseExact(_message.MessageCreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        // property containing the converted sender (extended by commands, events etc.)
        private FriendicaUser _sender;
        public FriendicaUser Sender
        {
            get { return _sender; }
            set { _sender = value; }
        }

        // property containing the converted recipient (extended by commands, events etc.)
        private FriendicaUser _recipient;
        public FriendicaUser Recipient
        {
            get { return _recipient; }
            set { _recipient = value; }
        }

        // property with the int type of the message id
        private int _messageIdInt;
        public int MessageIdInt
        {
            get { return _messageIdInt; }
            set { _messageIdInt = value; }
        }

        // property with the int type of the parent uri
        private string _messageParentUri;
        public string MessageParentUri
        {
            get { return _messageParentUri; }
            set { _messageParentUri = value; }
        }


        private DateTime _messageCreatedAtDateTime;
        public DateTime MessageCreatedAtDateTime
        {
            get { return _messageCreatedAtDateTime; }
            set
            {
                _messageCreatedAtDateTime = value;
                MessageCreatedAtLocalized = value.ToString("ddd") + " " + value.ToString("d") + " " + value.ToString("t");
            }
        }

        private string _messageCreatedAtLocalized;
        public string MessageCreatedAtLocalized
        {
            get { return _messageCreatedAtLocalized; }
            set { _messageCreatedAtLocalized = value; }
        }

        private bool _isReceivedMessage;
        public bool IsReceivedMessage
        {
            get { return _isReceivedMessage; }
            set { _isReceivedMessage = value; }
        }

        // url of the counterpart (sender or recipient other than authenticated user
        private string _counterpartUrl;
        public string CounterpartUrl
        {
            get { return _counterpartUrl; }
            set { _counterpartUrl = value; }
        }


        private string _headerForSearchMode;
        public string HeaderForSearchMode
        {
            get { return _headerForSearchMode; }
            set { _headerForSearchMode = value; }
        }


        // constructor transferring the message returned from server into this extended class
        public FriendicaMessage(JsonFriendicaMessage message)
        {
            if (message != null)
            {
                Message = message;
                if (message.MessageSender != null)
                    Sender = new FriendicaUser(message.MessageSender);
                if (message.MessageRecipient != null)
                    Recipient = new FriendicaUser(message.MessageRecipient);

                // set counterpart
                SetCounterpartUrl();
            }
        }

        private void SetCounterpartUrl()
        {
            if (Message.MessageSenderScreenName.ToLower() == Settings.FriendicaUsername.ToLower())
                CounterpartUrl = Recipient.User.UserUrl;
            else if (Message.MessageRecipientScreenName.ToLower() == Settings.FriendicaUsername.ToLower())
                CounterpartUrl = Sender.User.UserUrl;
            else
                CounterpartUrl = "";
        }

        private void SetIsReceivedMessage(string senderName, string recipientName)
        {
            if (Settings.FriendicaUsername != null)
            {
                if (senderName != null && senderName.ToLower() == Settings.FriendicaUsername.ToLower() || senderName == "Sample")
                    IsReceivedMessage = false;
                else if (recipientName != null && recipientName.ToLower() == Settings.FriendicaUsername.ToLower() || recipientName == "Sample")
                    IsReceivedMessage = true;
            }
            else
            {
                if (senderName != null && recipientName == "Sample")
                    IsReceivedMessage = true;
                else if (recipientName != null && senderName == "Sample")
                    IsReceivedMessage = false;
            }
        }
    }
}
