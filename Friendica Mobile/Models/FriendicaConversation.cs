using Friendica_Mobile.HttpRequests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.Web.Http;

namespace Friendica_Mobile.Models
{
    // used for private message conversations (conversations by posts and comments are "threads")
    public class FriendicaConversation : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        // keeps all messages from this conversation
        private ObservableCollection<FriendicaMessage> _messages;
        public ObservableCollection<FriendicaMessage> Messages
        {
            get { return _messages; }
            set { _messages = value;
                Messages.CollectionChanged += Messages_CollectionChanged;
                //GetNewestMessage();
                //CounterMessages = CountMessages();
                //CounterMessagesUnseen = CountUnseenMessages();
            }
        }

        // newest message used for displaying it in 25_Messages.xaml
        private FriendicaMessage _newestMessage;
        public FriendicaMessage NewestMessage
        {
            get { return _newestMessage; }
            set { _newestMessage = value;
                GetConversationDetails();
                if (value != null)
                {
                    NewestMessage.MessageTextTransformed = null;
                    NewestMessage.ConvertHtmlToParagraph();
                }
                OnPropertyChanged("NewestMessage"); }
        }

        // selected messages used for actions
        private FriendicaMessage _selectedMessage;
        public FriendicaMessage SelectedMessage
        {
            get { return _selectedMessage; }
            set { _selectedMessage = value; }
        }

        // store conversation parent-uri used for requests to the server
        private string _conversationUri;
        public string ConversationUri
        {
            get { return _conversationUri; }
            set { _conversationUri = value; }
        }

        // store title of conversation
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value;
                OnPropertyChanged("Title"); }
        }

        // store data of conversation partner
        private string _partnerName;
        public string PartnerName
        {
            get { return _partnerName; }
            set { _partnerName = value;
                OnPropertyChanged("PartnerName"); }
        }

        private string _partnerImageUrl;
        public string PartnerImageUrl
        {
            get { return _partnerImageUrl; }
            set { _partnerImageUrl = value;
                OnPropertyChanged("PartnerImageUrl"); }
        }

        // counter for messages in conversation
        private int _counterMessages;
        public int CounterMessages
        {
            get { return _counterMessages; }
            set { _counterMessages = value;
                OnPropertyChanged("CounterMessages"); }
        }

        private int _counterMessagesUnseen;
        public int CounterMessagesUnseen
        {
            get { return _counterMessagesUnseen; }
            set
            {
                _counterMessagesUnseen = value;
                if (value == 0)
                    HasNewMessages = false;
                else
                    HasNewMessages = true;
                OnPropertyChanged("CounterMessagesUnseen");
            }
        }

        // indicator for showing that new messages are available
        private bool _hasNewMessages;
        public bool HasNewMessages
        {
            get { return _hasNewMessages; }
            set { _hasNewMessages = value;
                OnPropertyChanged("HasNewMessages"); }
        }

        // indicator for showing that conversation has been loaded (to differentiate between already loaded and newly inserted conversations)
        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; }
        }

        // indicator for showing that system is currently loading conversation
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value;
                SetIsShowingContent();
                DeleteConversationCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoading"); }
        }

        // indicator for showing that system is currently updating seen status on server
        private bool _isUpdatingServerStatus;
        public bool IsUpdatingServerStatus
        {
            get { return _isUpdatingServerStatus; }
            set { _isUpdatingServerStatus = value;
                SetIsShowingContent();
                DeleteConversationCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsUpdatingServerStatus"); }
        }

        // indicator for showing that system is currently deleting conversation on server
        private bool _isDeletingOnServer;
        public bool IsDeletingOnServer
        {
            get { return _isDeletingOnServer; }
            set { _isDeletingOnServer = value;
                SetIsShowingContent();
                DeleteConversationCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsDeletingOnServer"); }
        }

        // indicator for showing content of newest message in conversations list
        private bool _isShowingContent;
        public bool IsShowingContent
        {
            get { return _isShowingContent; }
            set { _isShowingContent = value;
                OnPropertyChanged("IsShowingContent"); }
        }


        // add button
        Mvvm.Command _addMessageCommand;
        public Mvvm.Command AddMessageCommand { get { return _addMessageCommand ?? (_addMessageCommand = new Mvvm.Command(ExecuteAddMessage, CanAddMessage)); } }
        private bool CanAddMessage()
        {
            if (IsLoading || IsUpdatingServerStatus || IsDeletingOnServer)
                return false;
            else
            {
                if (IsLoaded)
                    return true;
                else
                    return false;
            }
        }

        private void ExecuteAddMessage()
        {
            // TODO: if fullmode show conversation right and open editor
            // TODO: if other modes navigate to Messages.xaml with OnlyMessages
        }

        // delete button
        Mvvm.Command _deleteConversationCommand;
        public Mvvm.Command DeleteConversationCommand { get { return _deleteConversationCommand ?? (_deleteConversationCommand = new Mvvm.Command(ExecuteDeleteConversation, CanDeleteConversation)); } }
        private bool CanDeleteConversation()
        {
            if (IsLoading || IsUpdatingServerStatus || IsDeletingOnServer)
                return false;
            else
            {
                if (IsLoaded)
                    return true;
                else
                    return false;
            }
        }

        private async void ExecuteDeleteConversation()
        {
            // ask user for confirmation to delete the conversation
            string errorMsg = String.Format(loader.GetString("messageDialogMessagesDeleteConversation"), Title);
            var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
            await dialog.ShowDialog(0, 1);
            if (dialog.Result == 0)
            {
                // delete the conversation, firstly avoid that user can navigate away during deletion
                App.NavStatus = NavigationStatus.ConversationDeleting;
                IsDeletingOnServer = true;
                foreach (var message in Messages)
                {
                    var deleteMessage = new GetFriendicaMessages();
                    deleteMessage.RequestFinished += DeleteMessage_RequestFinished;
                    deleteMessage.DeleteMessage(message.MessageId, ConversationUri);
                }
            }
        }


        public void ReloadConversation()
        {
            IsLoading = true;
            var getConv = new GetFriendicaMessages();
            getConv.FriendicaMessagesLoaded += GetConv_FriendicaMessagesLoaded;
            getConv.LoadConversation(ConversationUri);
        }

        private void GetConv_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getConv = sender as GetFriendicaMessages;
            if (!getConv.IsErrorOccurred)
            {
                var messages = getConv.MessagesReturned.OrderBy(m => m.MessageCreatedAtDateTime);
                Messages = new ObservableCollection<FriendicaMessage>();
                foreach (var message in messages)
                {
                    message.ConvertHtmlToParagraph();
                    Messages.Add(message);
                }
            }
            IsLoading = false;
            NewMessageAdded?.Invoke(this, EventArgs.Empty);
        }

        private async void DeleteMessage_RequestFinished(object sender, EventArgs e)
        {
            var deleteMessage = sender as GetFriendicaMessages;
            if (deleteMessage.StatusCode != HttpStatusCode.Ok)
                deleteMessage.IsErrorOccurred = true;

            if (!deleteMessage.IsErrorOccurred)
            {
                var deletedMess = Messages.SingleOrDefault(m => m.MessageId == deleteMessage.MessageId);
                if (deletedMess != null)
                    Messages.Remove(deletedMess);
            }
            else
            {
                // error occurred during communication with server
                string errorMsg;
                errorMsg = String.Format(loader.GetString("messageDialogMessagesNotImplemented"));
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
            CheckMessagesDeleting();
        }

        private void CheckMessagesDeleting()
        {
            if (IsDeletingOnServer)
            {
                // only false if all messages are deleted
                if (Messages.Count == 0)
                {
                    IsDeletingOnServer = false;
                    App.NavStatus = NavigationStatus.OK;
                    ConversationDeleted?.Invoke(this, EventArgs.Empty);
                }
            }

        }


        // event handlers for 
        //      conversation deleted (fired to remove conversation in MessageViewmodel)
        public event EventHandler ConversationDeleted;
        //      new message added to a conversation
        public event EventHandler NewMessageAdded;

        public FriendicaConversation()
        {
            SetIsShowingContent();
        }

        private void SetIsShowingContent()
        {
            if (!IsLoading && !IsUpdatingServerStatus && !IsDeletingOnServer)
                IsShowingContent = true;
            else
                IsShowingContent = false;
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            GetNewestMessage();
            CounterMessages = CountMessages();
            CounterMessagesUnseen = CountUnseenMessages();
            if (e.NewItems != null)
            {
                foreach (FriendicaMessage message in e.NewItems)
                    Messages.Single(m => m.MessageId == message.MessageId).ConvertHtmlToParagraph();
            }
            OnPropertyChanged("Messages");
        }

        public void GetNewestMessage()
        {
            // retrieve newest message from the conversation
            if (Messages != null && Messages.Count != 0)
            {
                //if (NewestMessage != null)
                //    NewestMessage.MessageTextTransformed = new Paragraph();
                ////NewestMessage = new FriendicaMessage();
                ////NewestMessage.MessageText = "";
                ////NewestMessage.ConvertHtmlToParagraph();
                var orderedByDate = Messages.OrderByDescending(m => m.MessageCreatedAtDateTime);
                var blankNewMessage = orderedByDate.First();
                NewestMessage = new FriendicaMessage(blankNewMessage);
            }
        }

        private void GetConversationDetails()
        {
            // retrieve ConversationUri and Title
            if (NewestMessage != null)
            {
                ConversationUri = NewestMessage.MessageParentUri;
                Title = NewestMessage.MessageTitle;

                // retrieve PartnerName and PartnerImageUrl
                FriendicaUser user;
                if (App.Settings.FriendicaUsername == null || App.Settings.FriendicaUsername == "")
                {
                    // we are in samples mode
                    if (NewestMessage.MessageSenderScreenName == "Sample")
                        user = NewestMessage.MessageRecipient;
                    else if (NewestMessage.MessageRecipientScreenName == "Sample")
                        user = NewestMessage.MessageSender;
                    else
                        user = null;
                }
                else
                {
                    if (NewestMessage.MessageSenderScreenName.ToLower() == App.Settings.FriendicaUsername.ToLower())
                        user = NewestMessage.MessageRecipient;
                    else if (NewestMessage.MessageRecipientScreenName.ToLower() == App.Settings.FriendicaUsername.ToLower())
                        user = NewestMessage.MessageSender;
                    else
                        user = null;
                }

                if (user != null)
                {
                    PartnerName = user.UserName;
                    PartnerImageUrl = user.UserProfileImageUrl;
                }
            }
        }

        private int CountMessages()
        {
            return Messages.Count();
        }

        private int CountUnseenMessages()
        {
            return Messages.Count(m => m.MessageSeen == "0");
        }

        public List<FriendicaMessage> MessagesForUpdate()
        {
            var list = new List<FriendicaMessage>();
            foreach (var message in Messages)
                if (message.MessageSeen == "0")
                    list.Add(message);
            return list;
        }

        public List<FriendicaMessage> MessagesForDelete()
        {
            var list = new List<FriendicaMessage>();
            foreach (var message in Messages)
                list.Add(message);
            return list;
        }
        
    }
}
