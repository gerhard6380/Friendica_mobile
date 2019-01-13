using Friendica_Mobile.PCL.Viewmodels;
using System;

namespace Friendica_Mobile.PCL.Models
{
    public class FriendicaUser : BindableClass
    {
        // property for holding the original return from server
        private JsonFriendicaUser _user;
        public JsonFriendicaUser User
        {
            get { return _user; }
            set { _user = value;
                if (User.UserName != null)
                    CharacterGroup = User.UserName.Substring(0, 1).ToUpper();
                if (User.UserFollowing)
                    ContactType = ContactTypes.Friends;
                else
                    ContactType = ContactTypes.Forums;
                IsAuthenticatedUser = SetIsAuthenticatedUser();
            }
        }

        // property to hold the first letter for the grouping of contacts
        private string _characterGroup;
        public string CharacterGroup
        {
            get { return _characterGroup; }
            set { _characterGroup = value; }
        }

        // property to define 
        private ContactTypes _contactType;
        public ContactTypes ContactType
        {
            get { return _contactType; }
            set { _contactType = value; }
        }

        // property to identify if a user dataset is referring to the user himself (in activities used)
        private bool _isAuthenticatedUser;
        public bool IsAuthenticatedUser
        {
            get { return _isAuthenticatedUser; }
            set { _isAuthenticatedUser = value; }
        }


        // event handler for click commands
        public event EventHandler ButtonShowProfileClicked;


        // constructor providing the original return from server
        public FriendicaUser(JsonFriendicaUser user)
        {
            if (user != null)
            {
                User = user;
            }
        }

        public FriendicaUser()
        {
        }

        // fire event for button clicked to show profile of the author
        Command<FriendicaUser> _showProfileCommand;
        public Command<FriendicaUser> ShowProfileCommand { get { return _showProfileCommand ?? (_showProfileCommand = new Command<FriendicaUser>(ExecuteShowProfile)); } }
        private void ExecuteShowProfile(FriendicaUser user)
        {
            ButtonShowProfileClicked?.Invoke(this, EventArgs.Empty);
        }


        // button clicked to show profile of the liking/disliking user - same event as above
        Command<FriendicaUser> _showProfileLikeCommand;
        public Command<FriendicaUser> ShowProfileLikeCommand { get { return _showProfileLikeCommand ?? (_showProfileLikeCommand = new Command<FriendicaUser>(ExecuteShowProfile)); } }


        // method to check if user is the authenticated user
        private bool SetIsAuthenticatedUser()
        {
            // no authenticated user if there is no server set so far (sample mode)
            if (CheckSettings.ServerSettingsAvailable())
            {
                // check url as well as there could be users with identic screen names on different servers
                if (Settings.FriendicaUsername.ToLower() == User.UserScreenName.ToLower() &&
                    User.UserUrl.Contains(Settings.FriendicaServer))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }


    }
}
