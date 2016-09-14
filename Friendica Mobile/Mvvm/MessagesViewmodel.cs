using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.Web.Http;


namespace Friendica_Mobile.Mvvm
{
    public class MessagesViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum MessagesViewStates { Fullmode, OnlyConversations, OnlyMessages };

        public Thickness BottomAppBarMargin { get { return App.Settings.BottomAppBarMargin; } }
        public double ListViewWidth { get { return App.Settings.ShellWidth; } }

        // status of current view
        private MessagesViewStates _messagesView;
        public MessagesViewStates MessagesView
        {
            get { return _messagesView; }
            set { _messagesView = value;
                if (value == MessagesViewStates.Fullmode)
                    App.MessagesNavigatedIntoConversation = false;
                OnPropertyChanged("MessagesView"); }
        }

        // contains the returned messages from server as a list (to have original returns from server stored)
        private List<FriendicaMessage> _retrievedMessages;
        public List<FriendicaMessage> RetrievedMessages
        {
            get { return _retrievedMessages; }
            set { _retrievedMessages = value; }
        }

        // contains the conversations from the server
        private ObservableCollection<FriendicaConversation> _conversations;
        public ObservableCollection<FriendicaConversation> Conversations
        {
            get { return _conversations; }
            set { _conversations = value;
                Conversations.CollectionChanged += Conversations_CollectionChanged;
            }
        }

        // following used to save conversations on navigating from and restoring on navigating to messages.xaml
        private List<FriendicaConversation> _saveConversations;


        private void Conversations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Conversations");
        }


        // contains the selected Conversation
        private FriendicaConversation _selectedConversation;
        public FriendicaConversation SelectedConversation
        {
            get { return _selectedConversation; }
            set { _selectedConversation = value;
                if (value == null)
                    IsSelectedConversation = false;
                else
                    IsSelectedConversation = true;
                OnPropertyChanged("SelectedConversation"); }
        }

        // selected conversation for updating on server
        private List<FriendicaConversation> _conversationsForUpdating = new List<FriendicaConversation>();
        private FriendicaConversation _conversationUpdating;

        // indicator if user has selected something
        private bool _isSelectedConversation;
        public bool IsSelectedConversation
        {
            get { return _isSelectedConversation; }
            set { _isSelectedConversation = value;
                OnPropertyChanged("IsSelectedConversation"); }
        }

        // indicator if no conversations are available
        private bool _noMessagesAvailable;
        public bool NoMessagesAvailable
        {
            get { return _noMessagesAvailable; }
            set { _noMessagesAvailable = value;
                LoadOlderMessagesCommand.RaiseCanExecuteChanged();
                EnableSearchCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("NoMessagesAvailable"); }
        }

        // indicator if searchmode is enabled (then display input box and search results instead of conversation list)
        private bool _isSearchModeEnabled;
        public bool IsSearchModeEnabled
        {
            get { return _isSearchModeEnabled; }
            set { _isSearchModeEnabled = value;
                if (!value)
                {
                    SearchConversations = new ObservableCollection<FriendicaConversation>();
                    SearchResults = new ObservableCollection<FriendicaMessage>();
                    SelectedConversation = null;
                }
                OnPropertyChanged("IsSearchModeEnabled");
            }
        }


        // search string
        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set { _searchString = value;
                SearchCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SearchString"); }
        }

        // contains the search results from the server
        private ObservableCollection<FriendicaMessage> _searchResults;
        public ObservableCollection<FriendicaMessage> SearchResults
        {
            get { return _searchResults; }
            set { _searchResults = value;
                SearchResults.CollectionChanged += SearchResults_CollectionChanged;
                OnPropertyChanged("SearchResults"); }
        }

        private void SearchResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (FriendicaMessage message in e.NewItems)
                SearchResults.Single(m => m.MessageId == message.MessageId).ConvertHtmlToParagraph();
        }

        // contains the conversations from the server
        private ObservableCollection<FriendicaConversation> _searchConversations;
        public ObservableCollection<FriendicaConversation> SearchConversations
        {
            get { return _searchConversations; }
            set
            {
                _searchConversations = value;
                OnPropertyChanged("SearchConversations");
            }
        }

        // indicator if no search results are returned
        private bool _noSearchResults;
        public bool NoSearchResults
        {
            get { return _noSearchResults; }
            set { _noSearchResults = value;
                OnPropertyChanged("NoSearchResults"); }
        }

        // indicator if no settings available (then display sample conversations)
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set { _noSettings = value;
                OnPropertyChanged("NoSettings"); }
        }

        // indicator if server is not supporting messages API for App
        private bool _noServerSupport;
        public bool NoServerSupport
        {
            get { return _noServerSupport; }
            set { _noServerSupport = value;
                AddNewConversationCommand.RaiseCanExecuteChanged();
                EnableSearchCommand.RaiseCanExecuteChanged();
                LoadOlderMessagesCommand.RaiseCanExecuteChanged();
                RefreshConversationsCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("NoServerSupport"); }
        }

        // indicator on top for loading initial conversations
        private bool _isLoadingMessages;
        public bool IsLoadingMessages
        {
            get { return _isLoadingMessages; }
            set { _isLoadingMessages = value;
                AddNewConversationCommand.RaiseCanExecuteChanged();
                RefreshConversationsCommand.RaiseCanExecuteChanged();
                EnableSearchCommand.RaiseCanExecuteChanged();
                LoadOlderMessagesCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingMessages"); }
        }

        // indicator on top for loading older conversations
        private bool _isLoadingOlderMessages;
        public bool IsLoadingOlderMessages
        {
            get { return _isLoadingOlderMessages; }
            set { _isLoadingOlderMessages = value;
                AddNewConversationCommand.RaiseCanExecuteChanged();
                RefreshConversationsCommand.RaiseCanExecuteChanged();
                EnableSearchCommand.RaiseCanExecuteChanged();
                LoadOlderMessagesCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingOlderMessages"); }
        }

        // indicator that all messages have been loaded from server
        private bool _allMessagesLoaded;
        public bool AllMessagesLoaded
        {
            get { return _allMessagesLoaded; }
            set { _allMessagesLoaded = value;
                LoadOlderMessagesCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("AllMessagesLoaded"); }
        }

        // set Property if refresh is currently in progress (showing progress bar and preventing from clicking Refresh again)
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                RefreshConversationsCommand.RaiseCanExecuteChanged();
                EnableSearchCommand.RaiseCanExecuteChanged();
                AddNewConversationCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsRefreshing");
            }
        }

        // indicator showing that app is querying server for the searchstring
        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set { _isSearching = value;
                OnPropertyChanged("IsSearching"); }
        }

        // indicator for showing the editor for new messages
        private bool _isEditorEnabled;
        public bool IsEditorEnabled
        {
            get { return _isEditorEnabled; }
            set { _isEditorEnabled = value;
                OnPropertyChanged("IsEditorEnabled"); }
        }

        // indicator for the editor mode
        private bool _isEditorInFullscreenMode;
        public bool IsEditorInFullscreenMode
        {
            get { return _isEditorInFullscreenMode; }
            set
            {
                _isEditorInFullscreenMode = value;
                OnPropertyChanged("IsEditorInFullscreenMode");
            }
        }

        // indicator for starting a new conversation (for add button)
        private bool _isStartingNewConversation;
        public bool IsStartingNewConversation
        {
            get { return _isStartingNewConversation; }
            set { _isStartingNewConversation = value;
                AddNewConversationCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsStartingNewConversation"); }
        }

        // list of possible contacts for selecting one
        public ObservableCollection<FriendicaUserExtended> Contacts
        {
            get { return App.ContactsFriends; }
        }

        // save selected contact
        private FriendicaUserExtended _selectedContact;
        public FriendicaUserExtended SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                SetNavigationStatus();
                OnPropertyChanged("SelectedContact");
            }
        }

        // string with title of new conversation
        private string _newMessageTitle;
        public string NewMessageTitle
        {
            get { return _newMessageTitle; }
            set { _newMessageTitle = value;
                SetNavigationStatus();
                OnPropertyChanged("NewMessageTitle"); }
        }

        // string with message content
        private string _newMessageContent;
        public string NewMessageContent
        {
            get { return _newMessageContent; }
            set { _newMessageContent = value;
                SetNavigationStatus();  }
        }

        // indicator showing that app is sending a new message to server
        private bool _isSendingNewMessage;
        public bool IsSendingNewMessage
        {
            get { return _isSendingNewMessage; }
            set { _isSendingNewMessage = value;
                RefreshConversationsCommand.RaiseCanExecuteChanged();
                AddNewConversationCommand.RaiseCanExecuteChanged();
                EnableSearchCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsSendingNewMessage"); }
        }

        // indicator that user came from clicking on a toast notification
        private bool _isNavigationFromToast;
        public bool IsNavigationFromToast
        {
            get { return _isNavigationFromToast; }
            set { _isNavigationFromToast = value; }
        }

        // saving conversation parenturi from toast notification
        private string _toastConversationUri;
        public string ToastConversationUri
        {
            get { return _toastConversationUri; }
            set { _toastConversationUri = value; }
        }

        // event handlers for 
        //      starting a new conversation
        public event EventHandler ButtonAddConversationClicked;
        //      enabling search mode
        public event EventHandler ButtonEnableSearchClicked;
        //      updating messages of a conversation (only used in this class)
        private event EventHandler _updateConversationsEvent;
        //      fired when a new conversation has been started to update the listview selection
        public event EventHandler NewMessageAdded;
        //      fired when we want to clear out the richeditboxes
        public event EventHandler ClearOutRicheditboxesRequested;
        //      fired when user clicks the enable editor button to scroll down to bottom
        public event EventHandler GoToBottomListViewRequested;

        // refresh button
        Mvvm.Command _refreshConversationsCommand;
        public Mvvm.Command RefreshConversationsCommand { get { return _refreshConversationsCommand ?? (_refreshConversationsCommand = new Mvvm.Command(ExecuteRefresh, CanRefresh)); } }
        private bool CanRefresh()
        {
            if (IsLoadingMessages || IsLoadingOlderMessages || IsRefreshing || IsSendingNewMessage || IsSearching || NoServerSupport)
                return false;
            else
                return true;
        }

        private async void ExecuteRefresh()
        {
            IsRefreshing = true;

            // if we are displaying sample data we cannot do a correct refresh, so we need to wait some seconds before removing "loading" flag
            if (NoSettings)
            {
                await Task.Delay(3000);
                IsRefreshing = false;
            }
            else
            {
                LoadMessagesNew();
            }
        }


        // add conversation button
        Mvvm.Command _addNewConversationCommand;
        public Mvvm.Command AddNewConversationCommand { get { return _addNewConversationCommand ?? (_addNewConversationCommand = new Mvvm.Command(ExecuteAddNewConversation, CanAddNewConversation)); } }
        private bool CanAddNewConversation()
        {
            if (IsLoadingMessages || IsLoadingOlderMessages || IsRefreshing || IsSearching || IsSendingNewMessage || IsStartingNewConversation || NoServerSupport)
                return false;
            else if (App.IsLoadedContacts)
                return true;
            else
                return false;
        }

        private void ExecuteAddNewConversation()
        {
            ButtonAddConversationClicked?.Invoke(this, EventArgs.Empty);
        }


        // search button - enabling search mode
        Mvvm.Command _enableSearchCommand;
        public Mvvm.Command EnableSearchCommand { get { return _enableSearchCommand ?? (_enableSearchCommand = new Mvvm.Command(ExecuteEnableSearch, CanEnableSearch)); } }
        private bool CanEnableSearch()
        {
            if (IsLoadingMessages || IsLoadingOlderMessages || IsRefreshing || IsSendingNewMessage || NoServerSupport || NoMessagesAvailable)
                return false;
            else
                return true;
        }

        private void ExecuteEnableSearch()
        {
            ButtonEnableSearchClicked?.Invoke(this, EventArgs.Empty);
        }

        // send search string button
        Mvvm.Command _searchCommand;
        public Mvvm.Command SearchCommand { get { return _searchCommand ?? (_searchCommand = new Mvvm.Command(ExecuteSearch, CanSearch)); } }
        private bool CanSearch()
        {
            if (SearchString != null && SearchString != "" && IsSearching == false)
                return true;
            else
                return false;
        }

        private void ExecuteSearch()
        {
            // if we are displaying sample data we cannot do a correct search on server, so we need to wait some seconds before removing "loading" flag
            if (NoSettings)
            {
                IsSearching = true;
                // perform a search within the loaded sample data set
                var results = new ObservableCollection<FriendicaMessage>();

                foreach (var conv in Conversations)
                {
                    var found = conv.Messages.Where(m => m.MessageText.ToLower().Contains(SearchString.ToLower()));
                    foreach (var message in found)
                    {
                        message.ConvertHtmlToParagraph();
                        results.Add(message);
                    }
                }

                if (results.Count != 0)
                {
                    NoSearchResults = false;
                    SearchResults = results;
                }
                else
                    NoSearchResults = true;
                IsSearching = false;
            }
            else
            {
                SearchMessages();
            }
        }

        // button for loading older messages
        Mvvm.Command _loadOlderMessagesCommand;
        public Mvvm.Command LoadOlderMessagesCommand { get { return _loadOlderMessagesCommand ?? (_loadOlderMessagesCommand = new Mvvm.Command(ExecuteLoadOlderMessages, CanLoadOlderMessages)); } }
        private bool CanLoadOlderMessages()
        {
            if (IsLoadingMessages || IsLoadingOlderMessages || AllMessagesLoaded || NoServerSupport || NoMessagesAvailable)
                return false;
            else
                return true;
        }

        private void ExecuteLoadOlderMessages()
        {
            LoadMessagesNext();
        }


        // toggle button - enabling editor
        Mvvm.Command _enableEditorCommand;
        public Mvvm.Command EnableEditorCommand { get { return _enableEditorCommand ?? (_enableEditorCommand = new Mvvm.Command(ExecuteEnableEditor, CanEnableEditor)); } }
        private bool CanEnableEditor()
        {
            if (IsSendingNewMessage)
                return false;
            else
                return true;
        }

        private void ExecuteEnableEditor()
        {
            GoToBottomListViewRequested?.Invoke(this, EventArgs.Empty);
            //IsEditorEnabled = !IsEditorEnabled;
        }


        // send message command
        Mvvm.Command _sendMessageCommand;
        public Mvvm.Command SendMessageCommand { get { return _sendMessageCommand ?? (_sendMessageCommand = new Mvvm.Command(ExecuteSendMessage, CanSendMessage)); } }
        private bool CanSendMessage()
        {
            if (NewMessageContent != null && NewMessageContent != "\r\r")
            {
                if (IsStartingNewConversation && SelectedContact == null)
                    return false;
                else
                {
                    App.NavStatus = NavigationStatus.NewMessageChanged;
                    return true;
                }
            }
            else
                return false;
        }

        private async void ExecuteSendMessage()
        {
            // if we are displaying sample data we cannot send a message to the server
            if (NoSettings)
            {
                // message to user: "no sending possible in test mode"
                string errorMsg;
                errorMsg = loader.GetString("messageDialogMessagesNewNoSettings");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
            else
            {
                var message = CreateMessage();
                ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);

                IsSendingNewMessage = true;
                // sending new message to server
                var postMessage = new PostFriendicaMessage();
                postMessage.FriendicaMessageSent += PostMessage_FriendicaMessageSent;
                postMessage.PostFriendicaMessageNew(message);
            }
        }

        private async void PostMessage_FriendicaMessageSent(object sender, EventArgs e)
        {
            var postMessage = sender as PostFriendicaMessage;
            if (!postMessage.IsSuccessStateCode)
            {
                IsSendingNewMessage = false;
                // message to user: "there was an error in sending the message"
                string errorMsg;
                errorMsg = String.Format(loader.GetString("messageDialogMessagesNewErrorSending"), postMessage.ErrorMessage);
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
            else
            {
                IsSendingNewMessage = false;
                IsEditorEnabled = false;
                App.NavStatus = NavigationStatus.OK;
                ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);

                if (postMessage.NewMessage.NewMessageReplyTo != "")
                {
                    // we were answering an existing message - reload conversation messages
                    SelectedConversation.NewMessageAdded += Conv_NewMessageAdded;
                    SelectedConversation.ReloadConversation();
                }
                else
                {
                    // we were creating a new conversation - load new conversations and set selection
                    LoadMessagesNew();
                    IsStartingNewConversation = false;
                }
            }
        }


        public MessagesViewmodel()
        {
            Conversations = new ObservableCollection<FriendicaConversation>();
            _saveConversations = new List<FriendicaConversation>();
            SearchConversations = new ObservableCollection<FriendicaConversation>();
            RetrievedMessages = new List<FriendicaMessage>();

            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;
            SetMessagesView();
            
            // check if there is a setting for the server, otherwise we will use sample data for the user
            CheckServerSettings();

            IsSendingNewMessage = App.IsSendingNewMessage;
            App.ContactsLoaded += App_ContactsLoaded;
            App.SendingNewMessageChanged += App_SendingNewMessageChanged;
            _updateConversationsEvent += MessagesViewmodel__updateConversationsEvent;
        }


        private void CheckServerSettings()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                NoSettings = true;
                PrepareSampleData();
                SetNewMessagesCounter();
            }
            else
                NoSettings = false;
        }

        private void PrepareSampleData()
        {
            // load sample data from separate class and save it in Observable Collection
            var sampleData = new FriendicaConversationSamples();
            Conversations = sampleData.ConversationSamples;
            foreach (var conv in sampleData.ConversationSamples)
                _saveConversations.Add(conv);
        }

        private void SetNavigationStatus()
        {
            if (NewMessageTitle != null || (NewMessageContent != null && NewMessageContent != "\r\r") || SelectedContact != null)
            {
                App.NavStatus = NavigationStatus.NewMessageChanged;
            }
            else
            {
                if (App.NavStatus == NavigationStatus.NewMessageChanged)
                    App.NavStatus = NavigationStatus.OK;
            }
            SendMessageCommand.RaiseCanExecuteChanged();
        }

        private FriendicaMessageNew CreateMessage()
        {
            var newMessage = new FriendicaMessageNew();
            if (SelectedConversation != null)
            {
                FriendicaUser user = null;
                if (SelectedConversation.NewestMessage.MessageSenderScreenName.ToLower() == App.Settings.FriendicaUsername.ToLower())
                    user = SelectedConversation.NewestMessage.MessageRecipient;
                else if (SelectedConversation.NewestMessage.MessageRecipientScreenName.ToLower() == App.Settings.FriendicaUsername.ToLower())
                    user = SelectedConversation.NewestMessage.MessageSender;
                newMessage.NewMessageUserUrl = user.UserUrl;

                newMessage.NewMessageReplyTo = SelectedConversation.NewestMessage.MessageId;
                newMessage.NewMessageTitle = SelectedConversation.Title;
            }
            else
            {
                newMessage.NewMessageUserUrl = SelectedContact.User.UserUrl;
                newMessage.NewMessageTitle = NewMessageTitle;
            }
            newMessage.NewMessageText = NewMessageContent;
            return newMessage;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth")
            {
                OnPropertyChanged("ListViewWidth");
                SetMessagesView();
            }
            if (e.PropertyName == "OrientationDevice")
                OnPropertyChanged("ListViewWidth");
        }

        private void SetMessagesView()
        {
            if (App.Settings.ShellWidth < 812)
            {
                if (SelectedConversation != null)
                {
                    App.MessagesNavigatedIntoConversation = true;
                    MessagesView = MessagesViewStates.OnlyMessages;
                }
                else
                    MessagesView = MessagesViewStates.OnlyConversations;
            }
            else
                MessagesView = MessagesViewStates.Fullmode;
        }

        private void App_ContactsLoaded(object sender, EventArgs e)
        {
            OnPropertyChanged("Contacts");
            AddNewConversationCommand.RaiseCanExecuteChanged();
        }


        private void App_SendingNewMessageChanged(object sender, EventArgs e)
        {
            IsSendingNewMessage = App.IsSendingNewPost;
            RefreshConversationsCommand.RaiseCanExecuteChanged();
            ExecuteRefresh();
        }


        public void SaveConversations()
        {
            _saveConversations = new List<FriendicaConversation>();
            foreach (var conv in Conversations)
                _saveConversations.Add(conv);
            Conversations = new ObservableCollection<FriendicaConversation>();
        }


        public void RestoreConversations()
        {
            Conversations = new ObservableCollection<FriendicaConversation>();
            foreach (var conv in _saveConversations)
                Conversations.Add(conv);
        }


        public void LoadMessagesInitial()
        {
            if (!NoSettings)
            {
                // load data from server
                IsLoadingMessages = true;
                var getMessagesInitial = new GetFriendicaMessages();
                getMessagesInitial.FriendicaMessagesLoaded += GetMessagesInitial_FriendicaMessagesLoaded;
                getMessagesInitial.LoadMessagesInitial(20);
            }
        }

        public void LoadMessagesInitialAfterTesting()
        {
            if (!NoSettings)
            {
                IsLoadingMessages = true;
                var getMessagesTestServer = new GetFriendicaMessages();
                getMessagesTestServer.FriendicaMessagesLoaded += GetMessagesTestServer_FriendicaMessagesLoaded;
                getMessagesTestServer.SetSeenMessage("0");
            }
        }

        private void GetMessagesTestServer_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesInitial = sender as GetFriendicaMessages;

            if (getMessagesInitial.StatusCode == HttpStatusCode.Ok && !getMessagesInitial.IsErrorOccurred)
            {
                LoadMessagesInitial();
            }
            else
            {
                IsLoadingMessages = false;

                if (getMessagesInitial.StatusCode == HttpStatusCode.NotImplemented || getMessagesInitial.StatusCode == HttpStatusCode.NotFound)
                {
                    // message to user: "server is not supporting private messages for the app - please update server to Friendica 3.5"
                    NoServerSupport = true;
                    //string errorMsg;
                    //errorMsg = String.Format(loader.GetString("messageDialogMessagesNotImplemented"));
                    //var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    //await dialog.ShowDialog(0, 0);
                }
            }
        }

        public async void UpdateStatusOnServer()
        {
            // save currently SelectedConversation for into queue of conversations for updating the messages
            if (!_conversationsForUpdating.Contains(SelectedConversation))
                _conversationsForUpdating.Add(SelectedConversation);

            // give user time to see the new messages
            await Task.Delay(3000);

            // fire event which is working on the queue of conversations
            if (_updateConversationsEvent != null)
                _updateConversationsEvent.Invoke(this, EventArgs.Empty);
        }


        private void MessagesViewmodel__updateConversationsEvent(object sender, EventArgs e)
        {
            // load next conversation for updating
            var conv = _conversationsForUpdating.FirstOrDefault();
            if (conv != null)
            {
                // select the corresponding conversation in Conversations list for setting indicator
                _conversationUpdating = Conversations.Single(c => c.ConversationUri == conv.ConversationUri);
                _conversationUpdating.IsUpdatingServerStatus = true;

                // go through all new message for updating them on server
                var MessagesUpdate = _conversationUpdating.MessagesForUpdate();

                if (MessagesUpdate.Count == 0)
                    _conversationUpdating.IsUpdatingServerStatus = false;

                foreach (var message in MessagesUpdate)
                {
                    if (App.Settings.FriendicaServer != null)
                    {

                        var getMessages = new GetFriendicaMessages();
                        getMessages.RequestFinished += GetMessages_RequestFinished;
                        getMessages.SetSeenMessage(message.MessageId);

                    }
                    else
                    {
                        message.MessageSeen = "1";
                    }
                }

                if (App.Settings.FriendicaServer == null)
                {
                    _conversationUpdating.IsUpdatingServerStatus = false;
                    _conversationUpdating.CounterMessagesUnseen = _conversationUpdating.Messages.Count(m => m.MessageSeen == "0");
                    SetNewMessagesCounter();
                }

                // we can now remove the conversation as we have started all updating processes
                _conversationsForUpdating.Remove(conv);
            }
        }


        private void GetMessages_RequestFinished(object sender, EventArgs e)
        {
            var getMessages = sender as GetFriendicaMessages;
            if (!getMessages.IsErrorOccurred)
            {
                foreach (var conv in Conversations)
                {
                    try
                    {
                        var message = conv.Messages.Single(m => m.MessageId == getMessages.MessageId);
                        message.MessageSeen = "1";
                        conv.CounterMessagesUnseen = conv.Messages.Count(m => m.MessageSeen == "0");
                    }
                    catch { }
                }

            }
            CheckMessagesUpdating();
        }


        private async void GetMessagesInitial_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesInitial = sender as GetFriendicaMessages;

            if (getMessagesInitial.StatusCode == HttpStatusCode.Ok && !getMessagesInitial.IsErrorOccurred)
            {
                // save loaded messages into variable
                RetrievedMessages = getMessagesInitial.MessagesReturned;

                var convs = getMessagesInitial.RetrieveConversations();
                foreach (var conv in convs)
                {
                    conv.IsLoading = true;
                    conv.ConversationDeleted += Conv_ConversationDeleted;
                    conv.NewMessageAdded += Conv_NewMessageAdded;
                    var getMessagesConv = new GetFriendicaMessages();
                    getMessagesConv.FriendicaMessagesLoaded += GetMessagesConv_FriendicaMessagesLoaded;
                    getMessagesConv.LoadConversation(conv.ConversationUri);
                    Conversations.Add(conv);

                    if (App.GetNameOfCurrentView() != "25_Messages")
                        _saveConversations.Add(conv);
                }
            }
            else
            {
                IsLoadingMessages = false;

                if (getMessagesInitial.StatusCode == HttpStatusCode.NotImplemented || getMessagesInitial.StatusCode == HttpStatusCode.NotFound)
                {
                    // message to user: "server is not supporting private messages for the app - please update server to Friendica 3.5"
                    NoServerSupport = true;
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogMessagesNotImplemented"));
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                }

                if (getMessagesInitial.ErrorMessageFriendica == GetFriendicaMessages.MessageErrors.NoMailsAvailable)
                {
                    NoMessagesAvailable = true;
                }
            }
        }


        private void SetNewMessagesCounter()
        {
            var unseenMessages = Conversations.Sum(c => c.CounterMessagesUnseen);
            App.TileCounter.CounterUnseenMessages = unseenMessages;

            // set last notified message to loaded max id to avoid user getting notification on read messages
            Double maxMessageId;
            if (RetrievedMessages.Count > 0)
                maxMessageId = Convert.ToDouble(RetrievedMessages.Max(m => m.MessageIdInt));
            else
                maxMessageId = 0;
            if (maxMessageId > App.Settings.LastNotifiedMessage)
                App.Settings.LastNotifiedMessage = maxMessageId;
        }

        private void Conv_NewMessageAdded(object sender, EventArgs e)
        {
            SetNewMessagesCounter();
            IsSendingNewMessage = false;
        }

        private void Conv_ConversationDeleted(object sender, EventArgs e)
        {
            var conv = sender as FriendicaConversation;
            if (SelectedConversation != null && conv.ConversationUri == SelectedConversation.ConversationUri)
                SelectedConversation = null;
            // remove the messages from the retrievedMessages otherwise we have a problem with loading new messages
            RetrievedMessages.RemoveAll(m => m.MessageParentUri == conv.ConversationUri);
            Conversations.Remove(conv);
            SetNewMessagesCounter();
        }

        private void GetMessagesConv_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesConv = sender as GetFriendicaMessages;
            if (!getMessagesConv.IsErrorOccurred)
            {
                var conv = Conversations.Single(c => c.ConversationUri == getMessagesConv.ConversationUri);
                if (conv.Messages == null)
                    conv.Messages = new ObservableCollection<FriendicaMessage>();

                if (getMessagesConv.MessagesReturned == null)
                    return;

                var messages = getMessagesConv.MessagesReturned.OrderBy(m => m.MessageCreatedAtDateTime);
                foreach (var message in messages)
                {
                    conv.Messages.Add(message);
                }
                conv.IsLoaded = true;
                conv.IsLoading = false;
            }
            CheckConversationsLoading();
        }


        private void CheckConversationsLoading()
        {
            bool isStillLoading = false;
            foreach (var conv in Conversations)
                isStillLoading |= conv.IsLoading;

            if (IsLoadingMessages)
                IsLoadingMessages = isStillLoading;
            else if (IsRefreshing)
                IsRefreshing = isStillLoading;
            else if (IsLoadingOlderMessages)
            {
                IsLoadingOlderMessages = isStillLoading;
                LoadOlderMessagesCommand.RaiseCanExecuteChanged();
            }

            SetNewMessagesCounter();

            if (!isStillLoading && IsNavigationFromToast)
            {
                SelectedConversation = Conversations.SingleOrDefault(c => c.ConversationUri == ToastConversationUri);
            }
        }

        private void CheckMessagesUpdating()
        {
            // retrieve correct conversation
            var conv = Conversations.Single(c => c.ConversationUri == _conversationUpdating.ConversationUri);
            if (conv.IsUpdatingServerStatus)
            {
                // only false if all messages are seen
                bool isStillUpdating = false;
                foreach (var message in _conversationUpdating.Messages)
                    isStillUpdating |= (message.MessageSeen == "0" ? true : false);
                conv.IsUpdatingServerStatus = isStillUpdating;

                if (!isStillUpdating)
                {
                    // all messages have been seen - we can change the indicator in the conversation
                    conv.HasNewMessages = false;
                    _conversationUpdating = null;

                    // fire event if still open conversations for updating available
                    if (_conversationsForUpdating.Count != 0)
                    {
                        if (_updateConversationsEvent != null)
                            _updateConversationsEvent.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            SetNewMessagesCounter();
        }

        public void LoadMessagesNext()
        {
            if (!IsLoadingOlderMessages)
            {
                var getMessagesNext = new GetFriendicaMessages();
                getMessagesNext.FriendicaMessagesLoaded += GetMessagesNext_FriendicaMessagesLoaded;

                // reduce minimum ID by 1 to avoid retrieving the oldest post again
                //var oldestId = RetrievedMessages.Min(m => m.MessageIdInt) - 1;
                int oldestId;
                if (Conversations.Count == 0)
                    oldestId = 0;
                else
                    oldestId = Conversations.SelectMany(m => m.Messages).Min(m => m.MessageIdInt) - 1;
                // oldestId may not be negative or zero, otherwise API returns the newest posts again
                if (oldestId > 0)
                {
                    IsLoadingOlderMessages = true;
                    LoadOlderMessagesCommand.RaiseCanExecuteChanged();
                    getMessagesNext.LoadMessagesNext(oldestId, 20);
                }
                else if (oldestId == 0)
                    AllMessagesLoaded = true;
            }
        }


        private async void GetMessagesNext_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesNext = sender as GetFriendicaMessages;

            if (getMessagesNext.StatusCode == HttpStatusCode.Ok && !getMessagesNext.IsErrorOccurred)
            {
                var convs = getMessagesNext.RetrieveConversations();
                foreach (var conv in convs)
                {
                    conv.IsLoading = true;
                    conv.ConversationDeleted += Conv_ConversationDeleted;
                    var getMessagesConv = new GetFriendicaMessages();
                    getMessagesConv.FriendicaMessagesLoaded += GetMessagesConv_FriendicaMessagesLoaded;
                    getMessagesConv.LoadConversation(conv.ConversationUri);
                    ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);
                    Conversations.Add(conv);
                }
            }
            else
            {
                if (getMessagesNext.ErrorMessageFriendica != GetFriendicaMessages.MessageErrors.NoMailsAvailable)
                {
                    // message to user: "there was an error on loading the private messages"
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogMessagesErrorLoading"), getMessagesNext.ErrorMessage);
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadMessagesNext();
                }
                else
                    AllMessagesLoaded = true;

                IsLoadingOlderMessages = false;
            }

        }


        public void LoadMessagesNew()
        {
            var getMessagesNew = new GetFriendicaMessages();
            getMessagesNew.FriendicaMessagesLoaded += GetMessagesNew_FriendicaMessagesLoaded;

            double newestId = 0;
            if (RetrievedMessages.Count != 0)
                newestId = Conversations.SelectMany(m => m.Messages).Max(m => m.MessageIdInt);
            IsRefreshing = true;
            getMessagesNew.LoadMessagesNew(newestId, 20);
        }


        private async void GetMessagesNew_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesNew = sender as GetFriendicaMessages;

            if (getMessagesNew.StatusCode == HttpStatusCode.Ok && !getMessagesNew.IsErrorOccurred)
            {
                // save loaded messages into variable
                foreach (var message in getMessagesNew.MessagesReturned)
                    RetrievedMessages.Insert(0, message);

                // set indicator back if we had no conversation in the list before
                if (NoMessagesAvailable)
                    NoMessagesAvailable = false;

                var convs = getMessagesNew.RetrieveConversations();
                foreach (var conv in convs)
                {
                    if (Conversations.Where(c => c.ConversationUri == conv.ConversationUri).Any())
                    {
                        var conversation = Conversations.Single(c => c.ConversationUri == conv.ConversationUri);
                        conversation.IsLoading = true;
                        conversation.NewMessageAdded += Conv_NewMessageAdded;
                        conversation.ReloadConversation();
                        if (MessagesView != MessagesViewStates.OnlyConversations)
                            SelectedConversation = conversation;
                    }
                    else
                    {
                        conv.IsLoading = true;
                        conv.ConversationDeleted += Conv_ConversationDeleted;
                        var getMessagesConv = new GetFriendicaMessages();
                        getMessagesConv.FriendicaMessagesLoaded += GetMessagesConv_FriendicaMessagesLoaded;
                        getMessagesConv.LoadConversation(conv.ConversationUri);
                        Conversations.Insert(0, conv);
                        if (MessagesView != MessagesViewStates.OnlyConversations)
                            SelectedConversation = conv;
                    }
                    ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);
                    if (MessagesView == MessagesViewStates.OnlyMessages)
                    {
                        App.MessagesNavigatedIntoConversation = true;
                    }
                    if (NewMessageAdded != null)
                        NewMessageAdded.Invoke(this, EventArgs.Empty);
                }
                IsRefreshing = false;
            }
            else
            {
                if (getMessagesNew.ErrorMessageFriendica != GetFriendicaMessages.MessageErrors.NoMailsAvailable)
                {
                    // message to user: "there was an error on loading the private messages"
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogMessagesErrorLoading"), getMessagesNew.ErrorMessage);
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadMessagesNew();
                }
                IsRefreshing = false;
            }

        }


        public void SearchMessages()
        {
            var searchMessages = new GetFriendicaMessages();
            searchMessages.FriendicaMessagesLoaded += SearchMessages_FriendicaMessagesLoaded;

            IsSearching = true;
            SearchResults = new ObservableCollection<FriendicaMessage>();
            SearchConversations = new ObservableCollection<FriendicaConversation>();
            searchMessages.SearchMessage(SearchString.ToLower());
        }

        private async void SearchMessages_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var searchMessages = sender as GetFriendicaMessages;

            if (searchMessages.StatusCode == HttpStatusCode.Ok && !searchMessages.IsErrorOccurred)
            {
                if (searchMessages.NoSearchResultsReturned)
                    NoSearchResults = true;
                else
                {
                    NoSearchResults = false;
                    foreach (var message in searchMessages.SearchResults)
                        SearchResults.Add(message);

                    // retrieve conversations of search results
                    var convs = searchMessages.RetrieveSearchConversations();
                    foreach (var conv in convs)
                    {
                        conv.IsLoading = true;
                        conv.NewMessageAdded += Conv_NewMessageAdded;
                        var getSearchConv = new GetFriendicaMessages();
                        getSearchConv.FriendicaMessagesLoaded += GetSearchConv_FriendicaMessagesLoaded;
                        getSearchConv.LoadConversation(conv.ConversationUri);
                        SearchConversations.Add(conv);
                    }
                }
            }
            else
            {
                if (searchMessages.NoSearchResultsReturned)
                {
                    NoSearchResults = true;
                }
                else
                {
                    // message to user: "there was an error on loading the private messages"
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogMessagesErrorLoading"), searchMessages.ErrorMessage);
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadMessagesNew();
                }
            }
            IsSearching = false;

        }

        private void GetSearchConv_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getSearchConv = sender as GetFriendicaMessages;
            if (!getSearchConv.IsErrorOccurred)
            {
                var conv = SearchConversations.Single(c => c.ConversationUri == getSearchConv.ConversationUri);
                if (conv.Messages == null)
                    conv.Messages = new ObservableCollection<FriendicaMessage>();

                if (getSearchConv.MessagesReturned == null)
                    return;

                var messages = getSearchConv.MessagesReturned.OrderBy(m => m.MessageCreatedAtDateTime);
                foreach (var message in messages)
                {
                    conv.Messages.Add(message);
                }
                conv.IsLoaded = true;
                conv.IsLoading = false;
            }
        }
    }
}