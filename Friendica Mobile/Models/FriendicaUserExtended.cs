using Friendica_Mobile.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Friendica_Mobile.Models
{
    public class FriendicaUserExtended : BindableClass
    {
        private AppSettings _appSettings = new AppSettings();
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public event EventHandler ButtonShowProfileClicked;

        private FriendicaUser _user;
        public FriendicaUser User
        {
            get { return _user; }
            set { _user = value;
                IsAuthenticatedUser = SetIsAuthenticatedUser(); }
        }

        private string _characterGroup;
        public string CharacterGroup
        {
            get { return _characterGroup; }
            set { _characterGroup = value; }
        }

        private ContactTypes _contactType;
        public ContactTypes ContactType
        {
            get { return _contactType; }
            set { _contactType = value; }
        }

        // used to set an indicator if the user in User is the same as in appsettings
        // can happen on likes/dislikes as a user could like his own post, too
        private bool _isAutheticatedUser;
        public bool IsAuthenticatedUser
        {
            get { return _isAutheticatedUser; }
            set { _isAutheticatedUser = value; }
        }


        // fire event for button clicked to show profile of the author
        Mvvm.Command<FriendicaUserExtended> _showProfileCommand;
        public Mvvm.Command<FriendicaUserExtended> ShowProfileCommand { get { return _showProfileCommand ?? (_showProfileCommand = new Mvvm.Command<FriendicaUserExtended>(ExecuteShowProfile)); } }
        private void ExecuteShowProfile(FriendicaUserExtended user)
        {
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
        }


        // button clicked to show profile of the liking/disliking user
        Mvvm.Command<FriendicaUserExtended> _showProfileLikeCommand;
        public Mvvm.Command<FriendicaUserExtended> ShowProfileLikeCommand { get { return _showProfileLikeCommand ?? (_showProfileLikeCommand = new Mvvm.Command<FriendicaUserExtended>(ExecuteShowProfileLike)); } }
        private async void ExecuteShowProfileLike(FriendicaUserExtended user)
        {
            // implement A3_Browser
            var mvvmBrowser = new BrowserViewmodel();
            mvvmBrowser.PageTitle = user.User.UserName;
            mvvmBrowser.IsVisibleHeader = true;

            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://" ||
                App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                // when no settings we have only sample contacts which have no real profile page
                string errorMsg;
                errorMsg = loader.GetString("messageDialogBrowserNoProfilePage");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);

                // we are in sample data test modus - no profile of the testusers to display, show support page
                mvvmBrowser.Url = "http://mozartweg.dyndns.org/friendica/profile/friendicamobile";
                mvvmBrowser.Uri = new Uri(mvvmBrowser.Url);
            }
            else
            {
                // build link to the profile of the author incl. zrl link to the own profile
                string baseUrl = "{0}?zrl={1}&timestamp={2}";
                var userProfile = user.User.UserUrl;

                string profile = App.Settings.FriendicaServer + "/profile/" + App.Settings.FriendicaUsername;

                var url = String.Format(baseUrl, userProfile, Uri.EscapeDataString(profile), Uri.EscapeDataString(DateTime.Now.ToString()));
                mvvmBrowser.Url = userProfile;
                mvvmBrowser.Uri = new Uri(url);
            }

            var frame = App.GetFrameForNavigation();
            frame.Navigate(typeof(Views.A3_Browser), mvvmBrowser);
        }


        public FriendicaUserExtended()
        {
        }

        // manual filling of class instance with sample data for contacts page
        public FriendicaUserExtended(double sampleCid, string sampleName, string sampleUrl, string sampleScreenName, string sampleImageUrl, ContactTypes type)
        {
            User = new FriendicaUser();
            User.UserCid = sampleCid;
            User.UserName = sampleName;
            User.UserUrl = sampleUrl;
            User.UserScreenName = sampleScreenName;
            User.UserProfileImageUrl = sampleImageUrl;
            CharacterGroup = User.UserName.Substring(0, 1).ToUpper();
            ContactType = type;
        }

        public FriendicaUserExtended(string jsonString) : this()
        {
            User = new FriendicaUser(jsonString);
            CharacterGroup = User.UserName.Substring(0, 1).ToUpper();
            if (User.UserFollowing)
                ContactType = ContactTypes.Friends;
            else
                ContactType = ContactTypes.Forums;
        }

        public FriendicaUserExtended(JsonObject jsonObject) : this()
        {
            User = new FriendicaUser(jsonObject);
            if (User.UserName != "")
                CharacterGroup = User.UserName.Substring(0, 1).ToUpper();

            if (User.UserFollowing)
                ContactType = ContactTypes.Friends;
            else
                ContactType = ContactTypes.Forums;
        }

        public FriendicaUserExtended(FriendicaUser user)
        {
            User = user;
            if (User.UserName != "")
                CharacterGroup = User.UserName.Substring(0, 1).ToUpper();

            if (User.UserFollowing)
                ContactType = ContactTypes.Friends;
            else
                ContactType = ContactTypes.Forums;
        }

        public JsonObject ToJsonObject()
        {
            return User.ToJsonObject();
        }

        private bool SetIsAuthenticatedUser()
        {
            // no authenticated user if there is no server set so far (sample mode)
            if (_appSettings.FriendicaServer == null || _appSettings.FriendicaServer == "" 
                    || _appSettings.FriendicaUsername == null || _appSettings.FriendicaUsername == "")
                return false;

            // check url as well as there could be users with identic screen names on different servers
            if (_appSettings.FriendicaUsername.ToLower() == User.UserScreenName.ToLower() &&
                User.UserUrl.Contains(_appSettings.FriendicaServer))
                return true;
            else
                return false;
        }

    }
}