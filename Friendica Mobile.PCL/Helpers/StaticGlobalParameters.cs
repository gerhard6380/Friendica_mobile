using Friendica_Mobile.PCL.Viewmodels;
using System;

namespace Friendica_Mobile.PCL
{
    public static class StaticGlobalParameters
    {
        // event handlers
        public static event EventHandler CounterNetworkChanged;
        public static event EventHandler CounterNewsfeedChanged;

        // set true if we have informed the user already that he/she cannot update likes/dislikes in testmode
        public static bool FriendicaPostTestModeInfoAlreadyShown;
        // set true if we have informed the user already that he/she cannot load older posts or refresh in testmode
        public static bool NetworkNoSettingsAlreadyShownNext;
        public static bool NetworkNoSettingsAlreadyShownRefresh;
        // set true if we have informed the user already that he/she cannot select the retweet content symbol when composing a new post
        public static bool NewPostDontSelectRetweetContentAlreadyShown;

        // show counter home on navigation button
        private static bool _showCounterUnseenHome;
        public static bool ShowCounterUnseenHome
        {
            get { return _showCounterUnseenHome; }
            set { _showCounterUnseenHome = value; }
        }

        // number as counter home on navigation button
        private static int _counterUnseenHome;
        public static int CounterUnseenHome
        {
            get { return _counterUnseenHome; }
            set { _counterUnseenHome = value;
                _counterUnseenHome = (value < 100) ? value : 99;
                ShowCounterUnseenHome = (_counterUnseenHome == 0) ? false : true;
            }
        }

        // show counter network on navigation button
        private static bool _showCounterUnseenNetwork;
        public static bool ShowCounterUnseenNetwork
        {
            get { return _showCounterUnseenNetwork; }
            set { _showCounterUnseenNetwork = value; }
        }

        // number as counter network on navigation button
        private static int _counterUnseenNetwork;
        public static int CounterUnseenNetwork
        {
            get { return _counterUnseenNetwork; }
            set { _counterUnseenNetwork = value;
                _counterUnseenNetwork = (value < 100) ? value : 99;
                ShowCounterUnseenNetwork = (_counterUnseenNetwork == 0) ? false : true;
                if (CounterNetworkChanged != null)
                    CounterNetworkChanged.Invoke(value, EventArgs.Empty);
            }
        }

        // show counter newsfeed on navigation button
        private static bool _showCounterUnseenNewsfeed;
        public static bool ShowCounterUnseenNewsfeed
        {
            get { return _showCounterUnseenNewsfeed; }
            set { _showCounterUnseenNewsfeed = value; }
        }

        // number as counter newsfeed on navigation button
        private static int _counterUnseenNewsfeed;
        public static int CounterUnseenNewsfeed
        {
            get { return _counterUnseenNewsfeed; }
            set { _counterUnseenNewsfeed = value;
                _counterUnseenNewsfeed = (value < 100) ? value : 99;
                ShowCounterUnseenNewsfeed = (_counterUnseenNewsfeed == 0) ? false : true;
                if (CounterNewsfeedChanged != null)
                    CounterNewsfeedChanged.Invoke(value, EventArgs.Empty);
            }
        }

        // show counter messages on navigation button
        private static bool _showCounterUnseenMessages;
        public static bool ShowCounterUnseenMessages
        {
            get { return _showCounterUnseenMessages; }
            set { _showCounterUnseenMessages = value; }
        }

        // number as counter messages on navigation button
        private static int _counterUnseenMessages;
        public static int CounterUnseenMessages
        {
            get { return _counterUnseenMessages; }
            set { _counterUnseenMessages = value;
                _counterUnseenMessages = (value < 100) ? value : 99;
                ShowCounterUnseenMessages = (_counterUnseenMessages == 0) ? false : true;
            }
        }

    }
}
