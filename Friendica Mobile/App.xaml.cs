using Friendica_Mobile.Models;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.HttpRequests;
using BackgroundTasks;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=402347&clcid=0x409

namespace Friendica_Mobile
{
    //public enum OrientationDeviceFamily { MobileLandscape, MobilePortrait, DesktopLandscape, DesktopPortrait };
    // possible states for NavStatus, currently only used in 91_Settings.xaml
    public enum NavigationStatus {  OK, SettingsChanged, GroupChanged, NewPostChanged, NewPostExternal, NewMessageChanged, ConversationDeleting };
    public enum ContactTypes { Friends, Forums, Groups }
    public enum NewPostTrigger {  None, ToastNotification, SpeechCommand }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public EventHandler<IActivatedEventArgs> Activated;
        public static LaunchActivatedEventArgs LaunchedEventArgs;
        public static IActivatedEventArgs ActivatedEventArgs;

        // indicator that device has camera capabilities
        public static bool DeviceHasCamera = false;

        // NavStatus used for blocking from navigating away from a page with unsaved content
        public static NavigationStatus NavStatus = NavigationStatus.OK;

        public static AppSettings Settings = new AppSettings();
        public static clsDetectTrial detectTrial = new clsDetectTrial();
        public static clsTileCounter TileCounter = new clsTileCounter();
        public static FriendicaConfig friendicaConfig = new FriendicaConfig();
        public static clsSQLiteConnection sqliteConnection = new clsSQLiteConnection();
        public static clsManageBadgeStatus Badge = new clsManageBadgeStatus();
        public static clsManageBackgroundTasks BackgroundTasks = new clsManageBackgroundTasks();

        public static ObservableCollection<FriendicaPostExtended> HomePosts = new ObservableCollection<FriendicaPostExtended>();
        public static ObservableCollection<FriendicaThread> HomeThreads = new ObservableCollection<FriendicaThread>();
        public static ObservableCollection<FriendicaPostExtended> NetworkPosts = null;
        public static ObservableCollection<FriendicaThread> NetworkThreads = null;
        public static bool IsVisibleChronological;
        public static bool IsVisibleChronologicalHome;

        // indicator showing that user has already been informed about test mode on trying likes/dislikes
        public static bool TestModeInfoAlreadyShown = false;

        private static bool _isLoadedContacts = false;
        public static bool IsLoadedContacts
        {
            get { return _isLoadedContacts; }
            set { _isLoadedContacts = value;
                // fire event so NetworkViewmodel.cs can react and enable the button 
                if (ContactsLoaded != null)
                    ContactsLoaded(null, EventArgs.Empty); }
        }
        public static event EventHandler ContactsLoaded;
        public static ObservableCollection<FriendicaUserExtended> ContactsFriends = null;
        public static ObservableCollection<FriendicaUserExtended> ContactsForums = null;
        public static ObservableCollection<FriendicaGroup> ContactsGroups = null;
        public static int ContactsSelectedPivot = 0;

        // parameter for showing that app is currently sending a new post 
        public static event EventHandler SendingNewPostChanged;
        private static bool _isSendingNewPost = false;
        public static bool IsSendingNewPost
        {
            get { return _isSendingNewPost; }
            set { _isSendingNewPost = value;
                if (SendingNewPostChanged != null)
                    SendingNewPostChanged(null, EventArgs.Empty); }
        }

        // parameter for showing that app is currently sending a new message 
        public static event EventHandler SendingNewMessageChanged;
        private static bool _isSendingNewMessage = false;
        public static bool IsSendingNewMessage
        {
            get { return _isSendingNewMessage; }
            set
            {
                _isSendingNewMessage = value;
                if (SendingNewMessageChanged != null)
                    SendingNewMessageChanged(null, EventArgs.Empty);
            }
        }
        
        // indicator if app has been started by an external trigger (ToastNotification, Speech)
        public static NewPostTrigger NewPostTriggered = NewPostTrigger.None;

        // container for getting the data of a new post and to start the sending process (see NewPostViewmodel.cs)
        public static PostFriendicaNewPost PostFriendica;

        // container for the MessageViewmodel storing current data on navigating
        public static MessagesViewmodel MessagesVm;

        // container for the ProfilesViewmodel storing current data on navigating
        public static ProfilesViewmodel ProfilesVm;

        // container for the PhotosViewmodel storing current data on navigating
        public static PhotosViewmodel PhotosVm;
        // set to true if the CheckServerSupport has taken place, just to avoid unnecessary web calls on reloads
        public static bool HasServerSupportChecked = false;

        // indicator if we have navigated into a conversation of messages view
        public static bool MessagesNavigatedIntoConversation;

        // indicator if we have navigated into a photoalbum of photos view
        public static bool PhotosNavigatedIntoAlbum;

        // event used for triggering moving back from the conversation to the overview 
        public static event EventHandler BackToConversationsRequested;

        // event used for triggering moving back from a photo album to the overview
        public static event EventHandler BackToAlbumsRequested;

        // event used for triggering change for OrientationDeviceFamilyTrigger 
        public static event EventHandler NavigationCompleted;
        private static bool _isNavigating = false;
        public static bool IsNavigating
        {
            get { return _isNavigating; }
            set { _isNavigating = value;
                if (NavigationCompleted != null)
                    NavigationCompleted(null, EventArgs.Empty); }
        }




        /// <summary>
        /// Allows tracking page views, exceptions and other telemetry through the Microsoft Application Insights service.
        /// </summary>
        //public static Microsoft.ApplicationInsights.TelemetryClient TelemetryClient;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            //TelemetryClient = new Microsoft.ApplicationInsights.TelemetryClient();
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            // trying to get more information on the unknown crashes after store update
            //UnhandledException += App_UnhandledException;
        }

        //private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    var errorMsg = String.Format("Unhandled exception on starting Friendica Mobile\n\nMessage:\n{0}\n\nSource:\n{1}\n\nStack Trace:\n{2}", e.Message + "///" + e.Exception.Message, e.Exception.Source, e.Exception.StackTrace);
        //    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
        //    await dialog.ShowDialog(0, 0);
        //    Application.Current.Exit();
        //}

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            LaunchedEventArgs = e;
            Initialize(e);
        }

        private async void Initialize(IActivatedEventArgs e)
        { 

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            // check if local device has a camera
            // TODO: check if we are allowed to use the camera
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);
            DeviceHasCamera = (devices.Count < 1) ? false : true;

            // check if user allows sending toasts from this app
            if (App.Settings.NotificationActivated)
                BackgroundTasks.ActivateNotifications();
            else
                BackgroundTasks.DeactivateNotifications();

            // start loading contacts in background so we have the data already available for new entries
            var contacts = new ContactsViewmodel();
            contacts.InitialLoad();

            // start loading the non loaded part of the app (if user wants to start into network, we load home; and vice versa)
            if (Settings.StartPage == "Home")
            {
                var network = new NetworkViewmodel();
                network.LoadInitial();
            }
            else if (Settings.StartPage == "Network")
            {
                var home = new HomeViewmodel();
                home.LoadInitial();
            }

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            // Place the frame in the current Window
            Window.Current.Content = new Views.Shell(rootFrame);

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                //rootFrame.Navigate(typeof(Views.A0_NewPost));
                // e.Arguments;
                if (e.Kind == ActivationKind.ToastNotification)
                {
                    var toastArgs = e as ToastNotificationActivatedEventArgs;
                    string[] parameter = toastArgs.Argument.Split(new char[] { '|' });

                    // if argument = "general" goto network, 
                    if (parameter[0] == "general")
                    {
                        rootFrame.Navigate(typeof(Views.Network));
                        await Badge.ClearBadgeNumber();
                    }
                    
                    // if argument = "reply" load thread of the clicked post and launch A0_NewPost-View
                    else if (parameter[0] == "reply")
                    {
                        App.NewPostTriggered = NewPostTrigger.ToastNotification;

                        double threadId = 0;
                        if (parameter[2] != "0")
                            threadId = Convert.ToDouble(parameter[2]);
                        else
                            threadId = Convert.ToDouble(parameter[1]);
                        var getThread = new GetFriendicaThread();
                        Task<ObservableCollection<FriendicaThread>> getThreadAsync = getThread.GetFriendicaThreadByIdAsync(threadId, App.Settings.FriendicaUsername, App.Settings.FriendicaPassword);
                        var thread = await getThreadAsync;
                        
                        rootFrame.Navigate(typeof(Views.A0_NewPost), thread);
                    }

                    // if argument = "pm_message" goto Message, 
                    else if (parameter[0] == "pm_direct")
                    {
                        rootFrame.Navigate(typeof(Views.Messages), parameter[2]);
                        await Badge.ClearBadgeNumber();
                    }

                }
                else
                {
                    // TODO back to normal behaviour
                    rootFrame.Navigate(typeof(Views.Photos));
                    //if (Settings.StartPage == "Home")
                    //    rootFrame.Navigate(typeof(Views.Home));
                    //else if (Settings.StartPage == "Network")
                    //    rootFrame.Navigate(typeof(Views.Network));
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();

            var shell = Window.Current.Content as Views.Shell;
            shell.BackToConversationsRequested += Shell_BackToConversationsRequested;
            shell.BackToAlbumsRequested += Shell_BackToAlbumsRequested;

            // start loading messages in background
            MessagesVm = new MessagesViewmodel();
            MessagesVm.LoadMessagesInitialAfterTesting();

            // start loading profiles in background - deactivated because of errors (already child of another element)
            //ProfilesVm = new ProfilesViewmodel();
            //ProfilesVm.LoadProfiles();
        }

        private void Shell_BackToAlbumsRequested(object sender, EventArgs e)
        {
            BackToAlbumsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Shell_BackToConversationsRequested(object sender, EventArgs e)
        {
            BackToConversationsRequested?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            
            // set badge number to zero if suspending
            var currentView = App.GetNameOfCurrentView();
            // if we are in A0_NewPost and have no CanGoBack, App has started from Toast Notification into A0_NewPost
            // in this case we do not want to set the badge count to zero
            if (currentView != "A0_NewPost")
            {
                await App.Badge.ClearBadgeNumber();
            }

            deferral.Complete();
        }


        protected override void OnActivated(IActivatedEventArgs args)
        {
            // coming here if user taps on a notification or the button there, send args to the Initialize()
            ActivatedEventArgs = args;
            if (Activated != null)
                Activated(this, args);

            Initialize(args);
            //base.OnActivated(args);
        }

        public static string GetNameOfCurrentView()
        {
            // retrieve the current shown view from the app
            var content = Window.Current.Content as Views.Shell;
            if (content == null)
                return "";
            else
            {
                var relPanel = content.Content as RelativePanel;
                string parent = "";
                foreach (var item in relPanel.Children)
                {
                    if (item.GetType() == typeof(SplitView))
                    {
                        var splitview = item as SplitView;
                        var contentSplitView = splitview.Content as Frame;
                        parent = contentSplitView.CurrentSourcePageType.Name;
                    }
                }
                return parent;
            }
        }


        public static Frame GetFrameOfCurrentView()
        {
            // retrieve the current shown view from the app as a Frame for the OrientationDeviceFamilyTrigger
            var content = Window.Current.Content as Views.Shell;
            if (content == null)
                return null;
            else
            {
                var relPanel = content.Content as RelativePanel;
                Frame contentSplitView = null;
                foreach (var item in relPanel.Children)
                {
                    if (item.GetType() == typeof(SplitView))
                    {
                        var splitview = item as SplitView;
                        contentSplitView = splitview.Content as Frame;
                    }
                }
                return contentSplitView;
            }
        }

        public static Frame GetFrameForNavigation()
        {
            var content = Window.Current.Content as Views.Shell;
            var relPanel = content.Content as RelativePanel;
            Frame frame = null;
            foreach (var item in relPanel.Children)
            {
                if (item.GetType() == typeof(SplitView))
                {
                    var splitview = item as SplitView;
                    frame = splitview.Content as Frame;
                }
            }
            return frame;
        }

        // prepare dropped or pasted content (ctrl-v) for a new post
        public static async void PasteClipboardContent(DataPackageView dataPackageView)
        {
            // dropped content is in parameter, if parameter = null than take content from Clipboard (ctrl-v pressed)
            if (dataPackageView == null)
                dataPackageView = Clipboard.GetContent();

            var post = new Models.FriendicaNewPost();
            var convert = new clsConvertDroppedContent();
            convert.MoreThanOneImageDropped += Convert_MoreThanOneImageDropped;
            convert.ConversionErrorCreated += Convert_ConversionErrorCreated;
            post = await convert.ConvertDroppedContent(dataPackageView);

            // navigate only if no error has been occured (post not returned on error)
            if (post != null)
            {
                // navigate to NewEntry.xaml page and pass the data from the drop operation
                var frame = GetFrameForNavigation();
                frame.Navigate(typeof(Views.A0_NewPost), convert);
            }
        }

        private static async void Convert_ConversionErrorCreated(object sender, EventArgs e)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            var convert = sender as clsConvertDroppedContent;
            var errorMsg = loader.GetString("messageDialogAppContentConversionError");
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }

        private static async void Convert_MoreThanOneImageDropped(object sender, EventArgs e)
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            var convert = sender as clsConvertDroppedContent;
            var errorMsg = String.Format(loader.GetString("messageDialogAppContentMoreThanOneImage"), convert.ImageFilename);
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }


    }
}
