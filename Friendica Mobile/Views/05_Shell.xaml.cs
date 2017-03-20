using Friendica_Mobile.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Friendica_Mobile.Views
{
    public class NotificationSymbol : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        
        public NotificationSymbol()
        {
            GetActivatedString();
        }

        private void GetActivatedString()
        {
            if (NotificationActivated)
                NotificationActivatedString = loader.GetString("menuflyoutNotificationOff");
            else
                NotificationActivatedString = loader.GetString("menuflyoutNotificationOn");
        }

        public bool NotificationActivated
        {
            get { return App.Settings.NotificationActivated; }
        }

        private string _notificationActivatedString;
        public string NotificationActivatedString
        {
            get { return _notificationActivatedString; }
            set { _notificationActivatedString = value;
                OnPropertyChanged("NotificationActivatedString"); }
        }

        public void Changed()
        {
            OnPropertyChanged("NotificationActivated");
            GetActivatedString();
        }

        // toggle flight mode (deactivate background checks for notifications on new postings)
        Mvvm.Command _flightModeCommand;
        public Mvvm.Command FlightModeCommand { get { return _flightModeCommand ?? (_flightModeCommand = new Mvvm.Command(ExecuteFlightMode)); } }
        private void ExecuteFlightMode()
        {
            if (App.Settings.NotificationActivated)
            {
                App.Settings.NotificationActivated = false;
            }
            else
            {
                App.Settings.NotificationActivated = true;
            }
        }

    }

    public sealed partial class Shell : Page
    {
        private Frame contentFrame;
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        NotificationSymbol symbol = new NotificationSymbol();
        bool isCtrlKeyPressed = false;
        bool isAltKeyPressed = false;
        // react on back button of phones - fire event in App.xaml.cs
        public event EventHandler BackToConversationsRequested;
        public event EventHandler BackToAlbumsRequested;


        public Shell(Frame frame)
        {
            App.Settings.PropertyChanged += Settings_PropertyChanged;

            this.contentFrame = frame;
            this.InitializeComponent();
            this.ShellSplitView.Content = frame;
            var update = new Action(() =>
            {
                // update radiobuttons after frame navigates
                var type = frame.CurrentSourcePageType;
                foreach (var radioButton in AllRadioButtons(this))
                {
                    var target = radioButton.CommandParameter as NavType;
                    if (target == null)
                        continue;
                    radioButton.IsChecked = target.Type.Equals(type);
                }
                this.ShellSplitView.IsPaneOpen = false;
                this.BackCommand.RaiseCanExecuteChanged();
            });
            frame.Navigated += (s, e) => update();
            this.Loaded += (s, e) => update();
            this.DataContext = this;

            // setzt Context auf App.Settings für die Gesamtseite, und auf Navigationsleiste für das Grid
            ShellSplitView.DataContext = App.Settings;
            advDesktop.DataContext = App.Settings;
            advMobile.DataContext = App.Settings;
            textblockAdvertising.DataContext = App.Settings;
            borderAdvMobile.DataContext = App.Settings;
            counterUnseenHome.DataContext = App.TileCounter;
            counterUnseenNetwork.DataContext = App.TileCounter;
            counterUnseenMessages.DataContext = App.TileCounter;
            iconNotificationActivated.DataContext = symbol;
            menuflyoutNotification.DataContext = symbol;

            masterGrid.DataContext = this;

            // set datacontext to enable change of visibility on subpages where navigation is useless
            ScrollViewerRadioButtons.DataContext = App.Settings;
            stackpanelOtherOptions.DataContext = App.Settings;
            radioOthers.DataContext = this;
            RadioButtonContainer.DataContext = this;


            this.SizeChanged += Shell_SizeChanged;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }

        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NotificationActivated")
            {
                symbol.Changed();
            }
            if (e.PropertyName == "HideNavigationElements")
            {
                AdaptNavigationBar();
            }
        }

        private async void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            var navigationAllowed = await TestNavigationState();

            // if we are in Messages and have navigated into a conversation we don't want to go back
            // in that case fire event in order to navigate back to conversation overview
            if (App.MessagesNavigatedIntoConversation)
            {
                if (navigationAllowed)
                    BackToConversationsRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            // if we are in Photos and have navigated into a photoalbum we don't want to go back
            // in that case fire event in order to navigate back to conversation overview
            if (App.PhotosNavigatedIntoAlbum)
            {
                if (navigationAllowed)
                    BackToAlbumsRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (navigationAllowed)
            {
                if (this.contentFrame == null)

                {
                    return;
                }

                if (this.contentFrame.CanGoBack)
                {
                    this.contentFrame.GoBack();
                    e.Handled = true;
                }
                else  // if no back page, close the app on the phone devices
                {
                    var currentView = App.GetNameOfCurrentView();
                    // if we are in A0_NewPost and have no CanGoBack, App has started from Toast Notification into A0_NewPost
                    // in this case we do not want to set the badge count to zero
                    if (currentView != "A0_NewPost")
                    {
                        await App.Badge.ClearBadgeNumber();
                    }
                    Application.Current.Exit();
                }
            }
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdaptNavigationBar();
            AdaptShellSize();
        }

        private void AdaptShellSize()
        {
            // vergrößert Splitview bis zur Shellgröße minus Advertising rechts wenn wir im LandscapeDesktop Modus sind
            var actualwidth = this.ActualWidth;
            if (actualwidth > 810)
                App.Settings.ShellWidth = actualwidth - 312;
            else
                App.Settings.ShellWidth = actualwidth;

            var actualheight = this.ActualHeight;
            App.Settings.ShellHeight = actualheight;
        }

        private void AdaptNavigationBar()
        {
            // blendet Pfeile in Navigationsleiste ein oder aus, wenn Bedarf besteht
            var actualheight = RadioButtonContainer.ActualHeight;

            var desiredsize = RadioButtonContainer.DesiredSize;
            if (actualheight <= desiredsize.Height && !App.Settings.HideNavigationElements)
            {
                radioUpButton.Visibility = Visibility.Visible;
                radioDownButton.Visibility = Visibility.Visible;
            }
            else
            {
                radioUpButton.Visibility = Visibility.Collapsed;
                radioDownButton.Visibility = Visibility.Collapsed;
            }
        }


        // back
        Mvvm.Command _backCommand;
        public Mvvm.Command BackCommand { get { return _backCommand ?? (_backCommand = new Mvvm.Command(ExecuteBack, CanBack)); } }
        private bool CanBack()
        {
            return true;
            //return this.contentFrame.CanGoBack;
        }
        private async void ExecuteBack()
        {
            var navigationAllowed = await TestNavigationState();

            // if we are in Messages and have navigated into a conversation we don't want to go back
            // in that case fire event in order to navigate back to conversation overview
            if (App.MessagesNavigatedIntoConversation)
            {
                if (navigationAllowed)
                    BackToConversationsRequested?.Invoke(this, EventArgs.Empty);
                return;
            }

            // if we are in Photos and have navigated into a photoalbum we don't want to go back
            // in that case fire event in order to navigate back to conversation overview
            if (App.PhotosNavigatedIntoAlbum)
            {
                if (navigationAllowed)
                    BackToAlbumsRequested?.Invoke(this, EventArgs.Empty);
            }

            if (this.contentFrame.CanGoBack)
            {
                if (navigationAllowed)
                {
                    App.NavStatus = NavigationStatus.OK;
                    if (this.contentFrame.CanGoBack)
                        this.contentFrame.GoBack();
                }
            }
        }

        // menu
        Mvvm.Command _menuCommand;
        public Mvvm.Command MenuCommand { get { return _menuCommand ?? (_menuCommand = new Mvvm.Command(ExecuteMenu)); } }
        private void ExecuteMenu()
        {
            this.ShellSplitView.IsPaneOpen = !this.ShellSplitView.IsPaneOpen;
        }

        // nav
        Mvvm.Command<NavType> _navCommand;

        public Mvvm.Command<NavType> NavCommand { get { return _navCommand ?? (_navCommand = new Mvvm.Command<NavType>(ExecuteNav)); } }
        private async void ExecuteNav(NavType navType)
        {
            var navigationAllowed = await TestNavigationState();
            if (navigationAllowed)
            {
                App.NavStatus = NavigationStatus.OK;
                var type = navType.Type;
                this.contentFrame.Navigate(navType.Type);
                // when we nav home, clear history
                if (App.Settings.StartPage == "Home")
                {
                    if (type.Equals(typeof(Views.Home)))
                    {
                        ClearBackStack();
                    }
                }
                else if (App.Settings.StartPage == "Network")
                {
                    if (type.Equals(typeof(Views.Network)))
                    {
                        ClearBackStack();
                    }
                }
            }
        }


        // exit application
        Mvvm.Command _exitCommand;
        public Mvvm.Command ExitCommand { get { return _exitCommand ?? (_exitCommand = new Mvvm.Command(ExecuteExit)); } }
        private async void ExecuteExit()
        {
            var navigationAllowed = await TestNavigationState();

            if (navigationAllowed)
            {
                // ask user if exiting is ok
                var dialog = new MessageDialog(loader.GetString("messageDialogExitApplication"));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                dialog.DefaultCommandIndex = 1;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsync();
                if ((int)result.Id == 1)
                    navigationAllowed = false;
                else
                {
                    navigationAllowed = true;
                }

                if (navigationAllowed)
                {
                    if (this.contentFrame == null)
                    {
                        return;
                    }

                    var currentView = App.GetNameOfCurrentView();
                    // if we are in A0_NewPost and have no CanGoBack, App has started from Toast Notification into A0_NewPost
                    // in this case we do not want to set the badge count to zero
                    if (!(currentView == "A0_NewPost" && !contentFrame.CanGoBack))
                    {
                        await App.Badge.ClearBadgeNumber();
                    }
                    Application.Current.Exit();
                }
            }
        }


        private void ClearBackStack()
        {
            this.contentFrame.BackStack.Clear();
            if (this.BackCommand != null)
            {
                this.BackCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task<bool> TestNavigationState()
        {
            switch (App.NavStatus)
            {
                // Kennzeichen auf OK, wird dürfen uns frei bewegen
                case NavigationStatus.OK:
                    {
                        return true;
                    }
                // Kennzeichen gesetzt, dass Settings ohne Test geändert wurden, Frage Benutzer ob ohne Speichern fortfahren.
                case NavigationStatus.SettingsChanged:
                    {
                        // "Einstellungen wurden ohne erfolgreichen Test verändert. Wollen Sie trotzdem die Seite verlassen?"
                        var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                        dialog.DefaultCommandIndex = 1;
                        dialog.CancelCommandIndex = 1;
                        var result = await dialog.ShowAsync();
                        if ((int)result.Id == 1)
                            return false;
                        else
                        {
                            return true;
                        }
                    }
                case NavigationStatus.GroupChanged:
                    {
                        // "Die Änderungen an der Gruppe wurden noch nicht gespeichert. Sind Sie sicher, dass Sie die Seite verlassen wollen?"
                        var dialog = new MessageDialog(loader.GetString("messageDialogGroupChangeNotSaved"));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                        dialog.DefaultCommandIndex = 1;
                        dialog.CancelCommandIndex = 1;
                        var result = await dialog.ShowAsync();
                        if ((int)result.Id == 1)
                            return false;
                        else
                        {
                            return true;
                        }
                    }
                // indicator set, that user has changed something for new posts, but not yet posted
                case NavigationStatus.NewPostChanged:
                    {
                        // "Der neue Beitrag wurde noch nicht gesendet. Sind Sie sicher, dass Sie die Seite verlassen wollen?"
                        var dialog = new MessageDialog(loader.GetString("messageDialogNewPostNotSent"));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                        dialog.DefaultCommandIndex = 1;
                        dialog.CancelCommandIndex = 1;
                        var result = await dialog.ShowAsync();
                        if ((int)result.Id == 1)
                            return false;
                        else
                        {
                            return true;
                        }
                    }
                // indicator set, that user has changed something for new posts, but not yet posted
                case NavigationStatus.NewMessageChanged:
                    {
                        // "Die neue Nachricht wurde noch nicht gesendet. Sind Sie sicher, dass Sie die Seite verlassen wollen?"
                        var dialog = new MessageDialog(loader.GetString("messageDialogMessagesDiscardNewConversation"));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                        dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                        dialog.DefaultCommandIndex = 1;
                        dialog.CancelCommandIndex = 1;
                        var result = await dialog.ShowAsync();
                        if ((int)result.Id == 1)
                            return false;
                        else
                        {
                            App.NavStatus = NavigationStatus.OK;
                            return true;
                        }
                    }
                // indicator set, that app is currently deleting the conversation from server
                case NavigationStatus.ConversationDeleting:
                    {
                        // "App is currently deleting a conversation from the server, please wait until completion"
                        string errorMsg = loader.GetString("messageDialogMessagesDeletingInProgress");
                        var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                        await dialog.ShowDialog(0, 0);
                        return false;
                    }
                // Fallback-Option für alle anderen Fälle
                default:
                    {
                        return true;
                    }
            }

        }

        // utility
        public List<RadioButton> AllRadioButtons(DependencyObject parent)
        {
            var list = new List<RadioButton>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is RadioButton)
                {
                    list.Add(child as RadioButton);
                    continue;
                }
                list.AddRange(AllRadioButtons(child));
            }
            return list;
        }

        // prevent check
        private void DontCheck(object s, RoutedEventArgs e)
        {
            // don't let the radiobutton check
            (s as RadioButton).IsChecked = false;
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewerRadioButtons.ChangeView(0, ScrollViewerRadioButtons.VerticalOffset - 48, 1);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewerRadioButtons.ChangeView(0, ScrollViewerRadioButtons.VerticalOffset + 48, 1);
        }

        private void RadioOthersButton_Click(object sender, RoutedEventArgs e)
        {
            // fire menuflyout if user taps/clicks on text of the radioOthers
            menuFlyoutOthers.ShowAt(radioOthers);
        }

        private void Page_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control) isCtrlKeyPressed = false;
            if (e.Key == Windows.System.VirtualKey.Menu) isAltKeyPressed = false;
        }

        private void Page_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Control) isCtrlKeyPressed = true;
            else if (isCtrlKeyPressed)
            {
                switch (e.Key)
                {
                    case Windows.System.VirtualKey.V:
                        var view = App.GetNameOfCurrentView();
                        if (view == "Home" || view == "Network")
                            App.PasteClipboardContent(null);
                        break;
                }
            }

            // listen for ALT+Left for backwards navigation (like in Internet browsers)
            if (e.Key == Windows.System.VirtualKey.Menu) isAltKeyPressed = true;
            else if (isAltKeyPressed)
            {
                switch(e.Key)
                {
                    case Windows.System.VirtualKey.Left:
                        ExecuteBack();
                        break;
                }
            }
        }
    }

    public class NavType
    {
        public Type Type { get; set; }
        public string Parameter { get; set; }
    }
}
