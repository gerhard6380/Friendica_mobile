using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Styles;
using Friendica_Mobile.Triggers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Messages : Page
    {
        // helper element for distinguishing different navigation states
        enum MessagesNavStatus { None, SelectFirst };

        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        // indicator to avoid looping to ask user for discarding changes
        bool IsListViewConversationSelectionSetBack = false;

        public Messages()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // react on back button pressed on phones to navigate back to onlyconversations
            App.BackToConversationsRequested += App_BackToConversationsRequested;

            // the bottomappbar might cover important things on Phone devices if keyboard is shown - collapse the appbar thanl
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;

            var mvvm = this.DataContext as MessagesViewmodel;
            mvvm.ButtonAddConversationClicked += Mvvm_ButtonAddConversationClicked;
            mvvm.ButtonEnableSearchClicked += Mvvm_ButtonEnableSearchClicked;
            mvvm.NewMessageAdded += Mvvm_NewMessageAdded;
            mvvm.ClearOutRicheditboxesRequested += Mvvm_ClearOutRicheditboxesRequested;
            mvvm.GoToBottomListViewRequested += Mvvm_GoToBottomListViewRequested;
            //mvvm.LoadMessagesInitial();
            mvvm.LoadMessagesInitialAfterTesting();
        }

        private void Mvvm_NewMessageAdded(object sender, EventArgs e)
        {
            var mvvm = sender as MessagesViewmodel;
            if (mvvm.MessagesView == MessagesViewmodel.MessagesViewStates.Fullmode)
            {
                // mark new conversation in Conversations listview as highlighted one
                listviewConversations.SelectedItem = mvvm.SelectedConversation;
                // set new messages to read after new conversation has been selected and loaded
                mvvm.UpdateStatusOnServer();
            }
            else if (mvvm.MessagesView == MessagesViewmodel.MessagesViewStates.OnlyMessages)
                listviewConversations.SelectedItem = mvvm.SelectedConversation;
        }

        private void Mvvm_GoToBottomListViewRequested(object sender, EventArgs e)
        {
            viewerMessages.UpdateLayout();
            viewerMessages.ChangeView(0, viewerMessages.ScrollableHeight, 1);
        }

        private async void App_BackToConversationsRequested(object sender, EventArgs e)
        {
            var answer = await AskUserForConfirmingDiscard(listviewConversations);

            if (answer)
            {
                var mvvm = this.DataContext as MessagesViewmodel;
                if (mvvm.MessagesView == MessagesViewmodel.MessagesViewStates.OnlyMessages)
                {
                    // deactivate sending mode
                    if (mvvm.IsStartingNewConversation)
                        mvvm.IsStartingNewConversation = false;
                    ClearOutRicheditboxes();

                    mvvm.MessagesView = MessagesViewmodel.MessagesViewStates.OnlyConversations;
                    mvvm.SelectedConversation = null;
                    listviewSearchResults.SelectedIndex = -1;
                    listviewConversations.SelectedIndex = -1;
                    App.MessagesNavigatedIntoConversation = false;
                    App.NavStatus = NavigationStatus.OK;
                }
            }
        }
        

        private void Mvvm_ClearOutRicheditboxesRequested(object sender, EventArgs e)
        {
            ClearOutRicheditboxes();
        }

        private void ClearOutRicheditboxes()
        {
            // clear out the RichEditBoxes
            UserControlMessagesEditor editor = editorNewConversation;
            var foundRichEditBox = editor.FindName("rebNewMessageText") as RichEditBox;
            foundRichEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
            ITextRange tr = foundRichEditBox.Document.Selection;
            tr.CharacterFormat.BackgroundColor = Colors.Transparent;
            tr.CharacterFormat.Name = "Segoe UI";
            tr.CharacterFormat.Italic = FormatEffect.Off;
            tr.CharacterFormat.Bold = FormatEffect.Off;
            tr.CharacterFormat.Underline = UnderlineType.None;
            tr.CharacterFormat.ForegroundColor = Colors.Black;

            UserControlMessagesEditor editorAnswer = editorNewAnswer;
            var foundRichEditBoxAnswer = editorAnswer.FindName("rebNewMessageText") as RichEditBox;
            foundRichEditBoxAnswer.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
            tr = foundRichEditBoxAnswer.Document.Selection;
            tr.CharacterFormat.BackgroundColor = Colors.Transparent;
            tr.CharacterFormat.Name = "Segoe UI";
            tr.CharacterFormat.Italic = FormatEffect.Off;
            tr.CharacterFormat.Bold = FormatEffect.Off;
            tr.CharacterFormat.Underline = UnderlineType.None;
            tr.CharacterFormat.ForegroundColor = Colors.Black;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Collapsed;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Visible;
        }


        private void viewerConversations_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var viewer = sender as ScrollViewer;

            if (viewer.ExtentHeight > viewer.ViewportHeight)
                buttonMessagesLoadOlderMessages.Visibility = Visibility.Collapsed;
            else
                buttonMessagesLoadOlderMessages.Visibility = Visibility.Visible;

            var mvvm = this.DataContext as MessagesViewmodel;
            if (mvvm.IsSearchModeEnabled)
                return;

            // check if we are reaching the last 10% of the total extent --> trigger to start loading new entries
            var atBottom = viewer.VerticalOffset > (viewer.ScrollableHeight - viewer.ActualHeight);

            // start loading the next tranch of entries
            if (atBottom)
            {
                var context = this.DataContext as MessagesViewmodel;
                if (context.RetrievedMessages != null && context.RetrievedMessages.Count > 0)
                    context.LoadMessagesNext();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var mvvm = this.DataContext as MessagesViewmodel;
            mvvm.SaveConversations();
            App.MessagesVm = mvvm;
            App.MessagesNavigatedIntoConversation = false;
            
            //var foundRtb = listviewConversations.FindName("rtbContent") as RichTextBlock;
            //foundRtb.Blocks.Clear();

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var mvvm = this.DataContext as MessagesViewmodel;
            if (e.Parameter != null)
            {
                if (e.Parameter.GetType() == typeof(string))
                {
                    mvvm.IsNavigationFromToast = true;
                    mvvm.ToastConversationUri = e.Parameter as string;
                }
            }
            else
            {
                // navigating to this page without parameter means real navigation or backnav
                if (App.MessagesVm != null)
                {
                    this.DataContext = App.MessagesVm;
                    mvvm = this.DataContext as MessagesViewmodel;
                    mvvm.ButtonAddConversationClicked += Mvvm_ButtonAddConversationClicked;
                    mvvm.RestoreConversations();

                    if (App.Settings.ShellWidth < 812)
                        mvvm.MessagesView = MessagesViewmodel.MessagesViewStates.OnlyConversations;
                    else
                        mvvm.MessagesView = MessagesViewmodel.MessagesViewStates.Fullmode;

                    foreach (var conv in mvvm.Conversations)
                    {
                        conv.GetNewestMessage();
                        conv.NewestMessage.ConvertHtmlToParagraph();
                    }

                    mvvm.SelectedConversation = null;
                    if (App.Settings.FriendicaUsername != null && !mvvm.NoServerSupport)
                        mvvm.LoadMessagesNew();
                }
            }

            base.OnNavigatedTo(e);
        }

        private void buttonGotoSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.Settings));
        }


        private void textboxSearchString_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var mvvm = this.DataContext as MessagesViewmodel;
            mvvm.SearchCommand.RaiseCanExecuteChanged();

            // switch to next textbox if user hits enter
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var inputPane = InputPane.GetForCurrentView();
                inputPane.TryHide();
                mvvm.SearchCommand.Execute(mvvm.SearchCommand);
            }
        }

        private async void listviewConversations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            var mvvm = this.DataContext as MessagesViewmodel;

            if (App.MessagesNavigatedIntoConversation)
                return;

            // ask user for confirmation if changes have not been sent
            bool answer;
            if (IsListViewConversationSelectionSetBack)
            {
                IsListViewConversationSelectionSetBack = false;
                return;
            }
            else
                answer = await AskUserForConfirmingDiscard(listview);

            if (answer)
            {
                App.NavStatus = NavigationStatus.OK;
                mvvm.NewMessageTitle = null;
                mvvm.NewMessageContent = null;
                mvvm.SelectedContact = null;
                listBoxSelectedContact.SelectedItem = null;

                // clear out the RichEditBoxes
                ClearOutRicheditboxes();

                IsListViewConversationSelectionSetBack = false;
            }
            else
            {
                IsListViewConversationSelectionSetBack = true;
                GoBackToLastSelection(e);
                return;
            }
            
            if (mvvm.IsStartingNewConversation)
                mvvm.IsStartingNewConversation = false;

            if (mvvm.IsEditorEnabled)
                mvvm.IsEditorEnabled = false;

            mvvm.SelectedConversation = listview.SelectedItem as FriendicaConversation;
            if (mvvm.SelectedConversation != null)
            {
                if (mvvm.SelectedConversation.IsLoading)
                    return;
                else
                {
                    // set new messages to read
                    if (mvvm.SelectedConversation.HasNewMessages)
                        mvvm.UpdateStatusOnServer();
                    foreach (var message in mvvm.SelectedConversation.Messages)
                        message.ConvertHtmlToParagraph();
                }
            }

            if (mvvm.MessagesView == MessagesViewmodel.MessagesViewStates.OnlyConversations && mvvm.SelectedConversation != null)
            {
                //mvvm.NoSettings = false;
                mvvm.MessagesView = MessagesViewmodel.MessagesViewStates.OnlyMessages;
                foreach (var message in mvvm.SelectedConversation.Messages)
                    message.ConvertHtmlToParagraph();
                App.MessagesNavigatedIntoConversation = true;
            }
        }

        private void GoBackToLastSelection(SelectionChangedEventArgs e)
        {
            IsListViewConversationSelectionSetBack = true;
            try
            {
                listviewConversations.SelectedItem = e.RemovedItems[0];
            }
            catch
            {
                listviewConversations.SelectedIndex = -1;
            }
        }

        private async void Mvvm_ButtonAddConversationClicked(object sender, EventArgs e)
        {
            var mvvm = sender as MessagesViewmodel;
            var answer = await AskUserForConfirmingDiscard(listviewConversations);

            if (!mvvm.IsStartingNewConversation)
            {
                if (answer)
                {
                    App.NavStatus = NavigationStatus.OK;
                    listviewConversations.SelectedIndex = -1;
                    mvvm.NewMessageTitle = null;
                    mvvm.NewMessageContent = null;
                    mvvm.SelectedContact = null;
                    listBoxSelectedContact.SelectedItem = null;

                    // clear out the RichEditBoxes
                    UserControlMessagesEditor editor = editorNewConversation;
                    RichEditBox foundRichEditBox = editor.FindName("rebNewMessageText") as RichEditBox;
                    foundRichEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
                    UserControlMessagesEditor editorAnswer = editorNewAnswer;
                    RichEditBox foundRichEditBoxAnswer = editorAnswer.FindName("rebNewMessageText") as RichEditBox;
                    foundRichEditBoxAnswer.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");

                    mvvm.IsStartingNewConversation = true;
                    mvvm.IsEditorEnabled = true;
                    mvvm.IsSelectedConversation = false;
                    mvvm.SelectedConversation = null;

                    // navigate again to Messages.xaml to show only messages 
                    if (mvvm.MessagesView == MessagesViewmodel.MessagesViewStates.OnlyConversations)
                    {
                        App.MessagesNavigatedIntoConversation = true;
                        mvvm.MessagesView = MessagesViewmodel.MessagesViewStates.OnlyMessages;
                    }
                }
            }
            else
            {
                if (answer)
                {
                    App.NavStatus = NavigationStatus.OK;
                    listviewConversations.SelectedIndex = -1;
                    mvvm.NewMessageTitle = null;
                    mvvm.NewMessageContent = null;
                    mvvm.SelectedContact = null;
                    listBoxSelectedContact.SelectedItem = null;

                    // clear out the RichEditBoxes
                    UserControlMessagesEditor editor = editorNewConversation;
                    RichEditBox foundRichEditBox = editor.FindName("rebNewMessageText") as RichEditBox;
                    foundRichEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
                    UserControlMessagesEditor editorAnswer = editorNewAnswer;
                    RichEditBox foundRichEditBoxAnswer = editorAnswer.FindName("rebNewMessageText") as RichEditBox;
                    foundRichEditBoxAnswer.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
                }
            }

        }

        private async Task<bool> AskUserForConfirmingDiscard(ListView listview)
        {
            UserControlMessagesEditor editor = editorNewConversation;
            RichEditBox foundRichEditBox = editor.FindName("rebNewMessageText") as RichEditBox;
            string rebText;
            foundRichEditBox.Document.GetText(TextGetOptions.None, out rebText);

            UserControlMessagesEditor editorAnswer = editorNewAnswer;
            RichEditBox foundRichEditBoxAnswer = editorAnswer.FindName("rebNewMessageText") as RichEditBox;
            string rebTextAnswer;
            foundRichEditBoxAnswer.Document.GetText(TextGetOptions.None, out rebTextAnswer);

            if (App.NavStatus == NavigationStatus.NewMessageChanged)
            {
                if (listview.SelectedIndex != -1)
                {
                    // message to user "started new conversation - delete?"
                    var dialog = new MessageDialog(loader.GetString("messageDialogMessagesDiscardNewConversation"));
                    dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                    dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                    dialog.DefaultCommandIndex = 1;
                    dialog.CancelCommandIndex = 1;
                    var result = await dialog.ShowAsync();
                    if ((int)result.Id == 1)
                    {
                        return false;
                    }
                    else
                    {
                        if (App.NavStatus == NavigationStatus.NewMessageChanged)
                            App.NavStatus = NavigationStatus.OK;
                        ClearOutRicheditboxes();
                        return true;
                    }
                }
                else
                    return true;
            }
            return true;
        }


        private async void Mvvm_ButtonEnableSearchClicked(object sender, EventArgs e)
        {
            var mvvm = this.DataContext as MessagesViewmodel;
            var answer = await AskUserForConfirmingDiscard(listviewConversations);

            if (answer)
            {
                App.NavStatus = NavigationStatus.OK;
                listviewConversations.SelectedIndex = -1;
                mvvm.NewMessageTitle = null;
                mvvm.NewMessageContent = null;
                mvvm.SelectedContact = null;
                listBoxSelectedContact.SelectedItem = null;

                // clear out the RichEditBoxes
                UserControlMessagesEditor editor = editorNewConversation;
                RichEditBox foundRichEditBox = editor.FindName("rebNewMessageText") as RichEditBox;
                foundRichEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
                UserControlMessagesEditor editorAnswer = editorNewAnswer;
                RichEditBox foundRichEditBoxAnswer = editorAnswer.FindName("rebNewMessageText") as RichEditBox;
                foundRichEditBoxAnswer.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");

                // set focus on searchbox when user clicked the search button in appbar
                textboxSearchString.Focus(FocusState.Keyboard);
            }
            else
            {
                // cancel entering search mode if user wants to continue editing new message
                mvvm.IsSearchModeEnabled = false;
                return;
            }

            // switch back to conversation mode if user disabled search mode while showing messages of a conversation
            if (mvvm.MessagesView == MessagesViewmodel.MessagesViewStates.OnlyMessages)
            {
                App.MessagesNavigatedIntoConversation = false;
                mvvm.MessagesView = MessagesViewmodel.MessagesViewStates.OnlyConversations;
            }

            // delete searchstring on disabling search mode
            if (mvvm.IsSearchModeEnabled)
            {
                mvvm.NoSearchResults = false;
                mvvm.SearchResults = new ObservableCollection<FriendicaMessage>();
                mvvm.SearchConversations = new ObservableCollection<FriendicaConversation>();
                mvvm.SearchString = "";
            }
            else
            {
                try { listviewConversations.Focus(FocusState.Programmatic); } catch { }
            }
        }


        private async void listviewSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // show conversation for the selected message on the right (full mode) 
            // or in a new page (navigate to messages.xaml with only messages)
            var mvvm = this.DataContext as MessagesViewmodel;
            var listview = sender as ListView;

            if (App.MessagesNavigatedIntoConversation)
                return;

            // ask user for confirmation if changes have not been sent
            bool answer;
            if (IsListViewConversationSelectionSetBack)
            {
                IsListViewConversationSelectionSetBack = false;
                return;
            }
            else
                answer = await AskUserForConfirmingDiscard(listview);

            if (answer)
            {
                App.NavStatus = NavigationStatus.OK;
                mvvm.IsStartingNewConversation = false;
                mvvm.NewMessageTitle = null;
                mvvm.NewMessageContent = null;
                mvvm.SelectedContact = null;
                listBoxSelectedContact.SelectedItem = null;

                // clear out the RichEditBoxes
                UserControlMessagesEditor editor = editorNewConversation;
                RichEditBox foundRichEditBox = editor.FindName("rebNewMessageText") as RichEditBox;
                foundRichEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");
                UserControlMessagesEditor editorAnswer = editorNewAnswer;
                RichEditBox foundRichEditBoxAnswer = editorAnswer.FindName("rebNewMessageText") as RichEditBox;
                foundRichEditBoxAnswer.Document.SetText(Windows.UI.Text.TextSetOptions.None, "");

                IsListViewConversationSelectionSetBack = false;
            }
            else
            {
                IsListViewConversationSelectionSetBack = true;
                listviewSearchResults.SelectedIndex = -1;
                return;
            }

            var selectedMessage = listview.SelectedItem as FriendicaMessage;
            if (selectedMessage != null)
            {
                var conversationUri = selectedMessage.MessageParentUri;

                // show conversation from searchConversation if not available in loaded conversations
                var convAvailable = mvvm.Conversations.Count(c => c.ConversationUri == conversationUri);
                if (convAvailable == 0)
                {
                    var conv = mvvm.SearchConversations.Single(c => c.ConversationUri == conversationUri);                    
                    mvvm.IsEditorEnabled = false;
                    foreach (var message in conv.Messages)
                        message.ConvertHtmlToParagraph();
                    mvvm.SelectedConversation = conv;
                }
                else
                {
                    var conv = mvvm.Conversations.Single(c => c.ConversationUri == conversationUri);
                    mvvm.IsEditorEnabled = false;
                    foreach (var message in conv.Messages)
                        message.ConvertHtmlToParagraph();
                    mvvm.SelectedConversation = conv;
                }
            }

            if (mvvm.MessagesView == MessagesViewmodel.MessagesViewStates.OnlyConversations)
            {
                //mvvm.NoSettings = false;
                mvvm.MessagesView = MessagesViewmodel.MessagesViewStates.OnlyMessages;
                App.MessagesNavigatedIntoConversation = true;
            }
        }

        private void listviewMessages_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            viewerMessages.ChangeView(0, viewerMessages.ScrollableHeight, 1);
        }
    }
}
