using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using Friendica_Mobile.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace Friendica_Mobile.Mvvm
{
    class ContactsViewmodel : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public event EventHandler ButtonShowProfileClicked;

        public Thickness BottomAppBarMargin { get { return App.Settings.BottomAppBarMargin; } }
        public double ListViewWidth { get { return App.Settings.ShellWidth; } }
        public double ListViewHeight {  get { return App.Settings.ShellHeight; } }

        // data for GridView listing friends incl. Eventhandler triggering preparation of group header overview
        public event EventHandler SourceFriendsLoaded;
        private IOrderedEnumerable<IGrouping<string, FriendicaUserExtended>> _sourceFriends;
        public IOrderedEnumerable<IGrouping<string, FriendicaUserExtended>> SourceFriends
        {
            get { return _sourceFriends; }
            set { _sourceFriends = value;
                OnPropertyChanged("SourceFriends");
                if (SourceFriendsLoaded != null)
                    SourceFriendsLoaded(this, EventArgs.Empty);
            }
        }

        // data for Grid View listing forums incl. Eventhandler triggering preparation of group header overview
        public event EventHandler SourceForumsLoaded;
        private IOrderedEnumerable<IGrouping<string, FriendicaUserExtended>> _sourceForums;
        public IOrderedEnumerable<IGrouping<string, FriendicaUserExtended>> SourceForums
        {
            get { return _sourceForums; }
            set
            {
                _sourceForums = value;
                OnPropertyChanged("SourceForums");
                if (SourceForumsLoaded != null)
                    SourceForumsLoaded(this, EventArgs.Empty);
            }
        }

        // containing data which will be shown in listview friends
        private ObservableCollection<FriendicaUserExtended> _friends;
        public ObservableCollection<FriendicaUserExtended> Friends
        {
            get { return _friends; }
            set { _friends = value;
                OnPropertyChanged("Friends"); }
        }

        // containing data which will be shown in listview forums
        private ObservableCollection<FriendicaUserExtended> _forums;
        public ObservableCollection<FriendicaUserExtended> Forums
        {
            get { return _forums; }
            set { _forums = value;
                OnPropertyChanged("Forums"); }
        }

        // containing data which will be shown in listview groups 
        private ObservableCollection<FriendicaGroup> _groups;
        public ObservableCollection<FriendicaGroup> Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                OnPropertyChanged("Groups");
            }
        }

        // events for loaded data to be used in Settings to display new contact lists
        public event EventHandler ContactsLoaded;
        public event EventHandler GroupsLoaded;

        // SearchString on Friends page
        private string _searchFriends;
        public string SearchFriends
        {
            get { return _searchFriends; }
            set { _searchFriends = value;
                FilterFriendsBySearch() ;
            }
        }

        // SearchString on Forums page
        private string _searchForums;
        public string SearchForums
        {
            get { return _searchForums; }
            set { _searchForums = value;
                FilterForumsBySearch(); }
        }

        // Friendicagroup to be changed
        private FriendicaGroup _changingGroup;
        public FriendicaGroup ChangingGroup
        {
            get { return _changingGroup; }
            set { _changingGroup = value; }
        }

        // FriendicaUserExtended selected on action
        private FriendicaUserExtended _selectedUser;
        public FriendicaUserExtended SelectedUser
        {
            get { return _selectedUser; }
            set { _selectedUser = value; }
        }

        // show or hide loading indicator and change status of buttons
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value;
                RefreshContactsCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoading"); }
        }

        // show or hide loading indicator and change status of buttons
        private bool _isLoadingFriends;
        public bool IsLoadingFriends
        {
            get { return _isLoadingFriends; }
            set
            {
                _isLoadingFriends = value;
                SetIsLoading();
            }
        }

        // show or hide loading indicator and change status of buttons
        private bool _isLoadingGroups;
        public bool IsLoadingGroups
        {
            get { return _isLoadingGroups; }
            set
            {
                _isLoadingGroups = value;
                SetIsLoading();
            }
        }


        // Indicator for no Settings declaring that the data are samples
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set { _noSettings = value;
                OnPropertyChanged("NoSettings"); }
        }


        // show or hide error message on server problems
        private bool _noDataAvailableFriends;
        public bool NoDataAvailableFriends
        {
            get { return _noDataAvailableFriends; }
            set { _noDataAvailableFriends = value;
                OnPropertyChanged("NoDataAvailableFriends"); }
        }


        // show or hide error message on server problems
        private bool _noDataAvailableForums;
        public bool NoDataAvailableForums
        {
            get { return _noDataAvailableForums; }
            set
            {
                _noDataAvailableForums = value;
                OnPropertyChanged("NoDataAvailableForums");
            }
        }

        // show or hide error message on server problems
        private bool _noDataAvailableGroups;
        public bool NoDataAvailableGroups
        {
            get { return _noDataAvailableGroups; }
            set
            {
                _noDataAvailableGroups = value;
                OnPropertyChanged("NoDataAvailableGroups");
            }
        }

        // show or hide error message if Server version is too old to implement needed API calls
        private bool _noGroupsAvailable = false;
        public bool NoGroupsAvailable
        {
            get { return _noGroupsAvailable; }
            set
            {
                _noGroupsAvailable = value;
                OnPropertyChanged("NoGroupsAvailable");
            }
        }


        // indicating number of columns shown in gridview zoomout depending on the device type and orientation
        private int _maxColumns;
        public int MaxColumns
        {
            get { return _maxColumns; }
            set { _maxColumns = value;
                OnPropertyChanged("MaxColumns"); }
        }

        // refresh button
        Mvvm.Command _refreshContactsCommand;
        public Mvvm.Command RefreshContactsCommand { get { return _refreshContactsCommand ?? (_refreshContactsCommand = new Mvvm.Command(ExecuteRefresh, CanRefresh)); } }
        private bool CanRefresh()
        {
            if (_isLoading)
                return false;
            else
                return true;
        }

        private void ExecuteRefresh()
        {
            if (SearchFriends != "" && SearchFriends != null)
            {
                SearchFriends = "";
                OnPropertyChanged("SearchFriends");
            }
            if (SearchForums != "" && SearchForums != null)
            {
                SearchForums = "";
                OnPropertyChanged("SearchForums");
            }
            InitialLoad();
        }

        // find local button
        Mvvm.Command _findLocalCommand;
        public Mvvm.Command FindLocalCommand { get { return _findLocalCommand ?? (_findLocalCommand = new Mvvm.Command(ExecuteFindLocal, CanFindLocal)); } }
        private bool CanFindLocal()
        {
                return true;
        }

        private async void ExecuteFindLocal()
        {
            // service is not yet implemented - give hint to user
            string errorMsg;
            errorMsg = loader.GetString("messageDialogContactsFindLocalNotAvailable");
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }


        // find global button
        Mvvm.Command _findGlobalCommand;
        public Mvvm.Command FindGlobalCommand { get { return _findGlobalCommand ?? (_findGlobalCommand = new Mvvm.Command(ExecuteFindGlobal, CanFindGlobal)); } }
        private bool CanFindGlobal()
        {
            return true;
        }

        private async void ExecuteFindGlobal()
        {
            // service is not yet implemented - give hint to user
            string errorMsg;
            errorMsg = loader.GetString("messageDialogContactsFindGlobalNotAvailable");
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }


        public event EventHandler ChangeGroupFired;

        public ContactsViewmodel()
        {
            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;

            // initialize observable collection
            if (Friends == null)
                Friends = new ObservableCollection<FriendicaUserExtended>();
            if (Forums == null)
                Forums = new ObservableCollection<FriendicaUserExtended>();
            if (Groups == null)
                Groups = new ObservableCollection<FriendicaGroup>();
        }


        public void InitialLoad()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                NoSettings = true;
                
                var obscollFriends = new ObservableCollection<FriendicaUserExtended>();
                obscollFriends.Add(new FriendicaUserExtended(1, "Tyrion Lannister", "https://friendica.server.test/profile/tyrion", "tyrion", "https://upload.wikimedia.org/wikipedia/en/5/50/Tyrion_Lannister-Peter_Dinklage.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(2, "Cersei Lannister", "https://friendica.server.text/profile/cersei", "cersei", "https://upload.wikimedia.org/wikipedia/en/a/a7/Queencersei.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(3, "Daenerys Targaryen", "https://friendica.server.text/profile/daenerys", "daenerys", "https://upload.wikimedia.org/wikipedia/en/0/0d/Daenerys_Targaryen_with_Dragon-Emilia_Clarke.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(4, "Jon Snow", "https://friendica.server.text/profile/jon_snow", "jon_snow", "https://upload.wikimedia.org/wikipedia/en/f/f0/Jon_Snow-Kit_Harington.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(5, "Sansa Stark", "https://friendica.server.text/profile/sansa", "sansa", "https://upload.wikimedia.org/wikipedia/en/7/74/SophieTurnerasSansaStark.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(6, "Arya Stark", "https://friendica.server.text/profile/aryastark", "aryastark", "https://upload.wikimedia.org/wikipedia/en/3/39/Arya_Stark-Maisie_Williams.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(7, "Ned Stark", "https://friendica.server.text/profile/nedstark", "nedstark", "https://upload.wikimedia.org/wikipedia/en/4/44/Ned_Stark_as_Portrayed_by_Sean_Bean_in_the_television_series_2011.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(8, "Jaime Lannister", "https://friendica.server.text/profile/jaimeL", "jaimeL", "https://upload.wikimedia.org/wikipedia/en/b/b5/JaimeLannister.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(9, "Petyr Baelish", "https://friendica.server.text/profile/petyr", "petyr", "https://upload.wikimedia.org/wikipedia/en/5/5e/Aidan_Gillen_as_Petyr_Baelish.jpg", ContactTypes.Friends));
                obscollFriends.Add(new FriendicaUserExtended(10, "Catelyn Stark", "https://friendica.server.text/profile/catelyn", "catelyn", "https://upload.wikimedia.org/wikipedia/en/1/1b/Catelyn_Stark_S3.jpg", ContactTypes.Friends));

                var obscollForums = new ObservableCollection<FriendicaUserExtended>();
                obscollForums.Add(new FriendicaUserExtended(500, "Games of Thrones", "https://friendica.server.test/profile/thrones", "thrones", "https://upload.wikimedia.org/wikipedia/en/9/93/AGameOfThrones.jpg", ContactTypes.Forums));
                obscollForums.Add(new FriendicaUserExtended(501, "Clash of Kings", "https://friendica.server.test/profile/kings", "kings", "https://upload.wikimedia.org/wikipedia/en/3/39/AClashOfKings.jpg", ContactTypes.Forums));
                obscollForums.Add(new FriendicaUserExtended(502, "Storm of Swords", "https://friendica.server.test/profile/swords", "swords", "https://upload.wikimedia.org/wikipedia/en/2/24/AStormOfSwords.jpg", ContactTypes.Forums));
                obscollForums.Add(new FriendicaUserExtended(503, "Feast for Crowns", "https://friendica.server.test/profile/crowns", "crowns", "https://upload.wikimedia.org/wikipedia/en/a/a3/AFeastForCrows.jpg", ContactTypes.Forums));

                var obscollGroups = new ObservableCollection<FriendicaGroup>();
                var userStarks = new List<FriendicaUser>();
                var userLannisters = new List<FriendicaUser>();
                var userOthers = new List<FriendicaUser>();

                foreach (var friend in obscollFriends)
                {
                    friend.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                    if (friend.User.UserName.Contains("Stark"))
                        userStarks.Add(friend.User);
                    else if (friend.User.UserName.Contains("Lannister"))
                        userLannisters.Add(friend.User);
                    else
                        userOthers.Add(friend.User);
                }

                foreach (var forum in obscollForums)
                {
                    forum.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                }
            
                obscollGroups.Add(new FriendicaGroup(1, "Stark", userStarks));
                obscollGroups.Add(new FriendicaGroup(2, "Lannister", userLannisters));
                obscollGroups.Add(new FriendicaGroup(3, "Others", userOthers));

                // insert result into list of posts
                Friends = obscollFriends;
                Forums = obscollForums;
                Groups = obscollGroups;
                foreach (var group in Groups)
                {
                    group.GroupDeleted += Group_GroupDeleted;
                    group.GroupChange += Group_GroupChange;
                }

                // group results and save lists to App.xaml.cs for storing during app runtime (no reload if switching between pages)
                PrepareSources();
                SaveToApp();

                // hide red bar with progress ring for loading process
                IsLoadingFriends = false;
                IsLoadingGroups = false;
                NoDataAvailableFriends = false;
                NoDataAvailableForums = false;
            }
            else
            {
                IsLoadingFriends = true;
                IsLoadingGroups = true;
                NoSettings = false;
                // load data from server
                var getFriends = new GetFriendicaFriends();
                getFriends.FriendicaFriendsLoaded += GetFriends_FriendicaFriendsLoaded;
                getFriends.GetFriendicaFriendsList();
                var getGroups = new GetFriendicaGroups();
                getGroups.FriendicaGroupsLoaded += GetGroups_FriendicaGroupsLoaded;
                getGroups.GetFriendicaGroupsList();
            }
        }

        private void GetGroups_FriendicaGroupsLoaded(object sender, EventArgs e)
        {
            var getGroups = sender as GetFriendicaGroups;

            if (getGroups.StatusCode == HttpStatusCode.Ok)
            {
                // convert result into an observableCollection
                var obscollGroups = ConvertJsonToObsCollGroup(getGroups);

                // insert result into list of posts
                Groups = obscollGroups;
                foreach (var group in Groups)
                {
                    group.GroupDeleted += Group_GroupDeleted;
                    group.GroupChange += Group_GroupChange;
                }
                SaveToApp();

                // hide red bar with progress ring for loading process
                IsLoadingGroups = false;

                // Anzeige einer Info triggern, die dem User anzeigt, dass die Daten geladen wurden, aber nichts gekommen ist
                NoDataAvailableGroups = (Groups == null || Groups.Count == 0);

                // showing hint if result is not possible due to old server version
                if (getGroups.ReturnString.Contains("error") && getGroups.ReturnString.Contains("not implemented"))
                    NoGroupsAvailable = true;
                else
                    NoGroupsAvailable = false;

                // fire event if settings page is listening for new contacts
                if (GroupsLoaded != null)
                    GroupsLoaded(this, EventArgs.Empty);
            }
            else
            {
                IsLoadingGroups = false;
                if (getGroups.ReturnString == null)
                    NoGroupsAvailable = true;
                else if (getGroups.ReturnString.Contains("error") && getGroups.ReturnString.Contains("not implemented"))
                    NoGroupsAvailable = true;

                // es gab einen Fehler beim Abrufen der Daten
                // "Fehler beim Laden der Daten. (Fehlermeldung: xxx) \n\n Möchten Sie es erneut versuchen?"
                // don't show two error messages if server not reachable (one for Friends one for Groups)
                //string errorMsg;
                //if (getGroups.StatusCode == HttpStatusCode.None)
                //    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), loader.GetString("messageDialogHttpStatusNone"));
                //else
                //    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), getGroups.StatusCode.ToString());
                //var dialog = new MessageDialog(errorMsg);
                ////var dialog = new MessageDialog(loader.GetString("messageDialogSettingsNotSaved"));
                //dialog.Commands.Add(new UICommand(loader.GetString("buttonYes"), null, 0));
                //dialog.Commands.Add(new UICommand(loader.GetString("buttonNo"), null, 1));
                //dialog.DefaultCommandIndex = 0;
                //dialog.CancelCommandIndex = 1;
                //var result = await dialog.ShowAsync();
                //if ((int)result.Id == 0)
                //    InitialLoad();
                //else
                //{
                //    NoDataAvailableFriends = (Friends == null || Friends.Count == 0);
                //    NoDataAvailableForums = (Forums == null || Forums.Count == 0);
                //}
            }
        }

        private void Group_GroupChange(object sender, EventArgs e)
        {
            ChangingGroup = sender as FriendicaGroup;
            if (ChangeGroupFired != null)
                ChangeGroupFired(this, EventArgs.Empty);
        }

        private void Group_GroupDeleted(object sender, EventArgs e)
        {
            foreach (var group in Groups)
                group.GroupDeleted -= Group_GroupDeleted;
            IsLoadingGroups = true;
            NoSettings = false;
            // load data from server
            var getGroups = new GetFriendicaGroups();
            getGroups.FriendicaGroupsLoaded += GetGroups_FriendicaGroupsLoaded;
            getGroups.GetFriendicaGroupsList();
        }


        private async void GetFriends_FriendicaFriendsLoaded(object sender, EventArgs e)
        {
            var getFriends = sender as GetFriendicaFriends;

            if (getFriends.StatusCode == HttpStatusCode.Ok)
            {
                // convert result into an observableCollection
                var obscollFriends = ConvertJsonToObsColl(getFriends, ContactTypes.Friends);
                var obscollForums = ConvertJsonToObsColl(getFriends, ContactTypes.Forums);
                
                // insert result into list of posts
                Friends = obscollFriends;
                Forums = obscollForums;

                // group results and save lists to App.xaml.cs for storing during app runtime (no reload if switching between pages)
                PrepareSources();
                SaveToApp();

                // hide red bar with progress ring for loading process
                IsLoadingFriends = false;

                // Anzeige einer Info triggern, die dem User anzeigt, dass die Daten geladen wurden, aber nichts gekommen ist
                NoDataAvailableFriends = (Friends == null || Friends.Count == 0);
                NoDataAvailableForums = (Forums == null || Forums.Count == 0);

                // fire event if settings page is listening for new contacts
                if (ContactsLoaded != null)
                    ContactsLoaded(this, EventArgs.Empty);
            }
            else
            {
                // es gab einen Fehler beim Abrufen der Daten
                // "Fehler beim Laden der Daten. (Fehlermeldung: xxx) \n\n Möchten Sie es erneut versuchen?"
                string errorMsg;
                if (getFriends.StatusCode == HttpStatusCode.None)
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), loader.GetString("messageDialogHttpStatusNone"));
                else
                    errorMsg = String.Format(loader.GetString("messageDialogErrorOnLoadingData"), getFriends.StatusCode.ToString());
                var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                await dialog.ShowDialog(0, 1);
                if (dialog.Result == 0)
                    InitialLoad();
                else
                {
                    NoDataAvailableFriends = (Friends == null || Friends.Count == 0);
                    NoDataAvailableForums = (Forums == null || Forums.Count == 0);
                }
                IsLoadingFriends = false;
            }

        }

        private void FilterFriendsBySearch()
        {
            if (SearchFriends == "")
                LoadFromApp();
            else
            {
                var filteredFriends = from user in App.ContactsFriends
                          where user.User.UserName.ToLower().Contains(SearchFriends.ToLower()) 
                                || user.User.UserUrl.ToLower().Contains(SearchFriends.ToLower())
                          select user;
                if (Friends.Count == filteredFriends.Count())
                    return;

                Friends = new ObservableCollection<FriendicaUserExtended>(filteredFriends);
                PrepareSources();
            }
        }

        private void FilterForumsBySearch()
        {
            if (SearchForums == "")
                LoadFromApp();
            else
            {
                var filteredForums = from user in App.ContactsForums
                                      where user.User.UserName.ToLower().Contains(SearchForums.ToLower()) 
                                      || user.User.UserUrl.ToLower().Contains(SearchForums.ToLower())
                                      select user;
                if (Forums.Count == filteredForums.Count())
                    return;

                Forums = new ObservableCollection<FriendicaUserExtended>(filteredForums);
                PrepareSources();
            }

        }

        private void PrepareSources()
        {
            SourceFriends = from user in Friends group user by user.CharacterGroup into grp orderby grp.Key select grp;
            SourceForums = from user in Forums group user by user.CharacterGroup into grp orderby grp.Key select grp;
        }

        private void SaveToApp()
        {
            // sort users before we save them to App (so we get an ordered list for the NewPostViewmodel
            var friends = Friends.OrderBy(c => c.User.UserName);
            var obscollFriends = new ObservableCollection<FriendicaUserExtended>();
            foreach (var friend in friends)
                obscollFriends.Add(friend);
            App.ContactsFriends = obscollFriends;

            App.ContactsForums = Forums;
            App.ContactsGroups = Groups;
            App.IsLoadedContacts = true;
        }

        private void LoadFromApp()
        {
            Friends = App.ContactsFriends;
            Forums = App.ContactsForums;
            Groups = App.ContactsGroups;
            foreach (var user in Friends)
                user.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
            foreach (var user in Forums)
                user.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;

            foreach (var group in Groups)
            {
                group.GroupDeleted += Group_GroupDeleted;
                group.GroupChange += Group_GroupChange;
            }
            PrepareSources();
        }

        private void SetIsLoading()
        {
            IsLoading = IsLoadingFriends || IsLoadingGroups;
        }

        private ObservableCollection<FriendicaUserExtended> ConvertJsonToObsColl(GetFriendicaFriends httpResult, ContactTypes type)
        {
            var obscoll = new ObservableCollection<FriendicaUserExtended>();
            JsonArray resultArray = null;
            var testArray = JsonArray.TryParse(httpResult.ReturnString, out resultArray);
            if (testArray)
            {
                int arraySize = resultArray.Count;
                for (int i = 0; i < arraySize; i++)
                {
                    IJsonValue element = resultArray.GetArray()[i];
                    switch (element.ValueType)
                    {
                        case JsonValueType.Object:
                            var result = new FriendicaUserExtended(element.ToString());
                            result.ButtonShowProfileClicked += Result_ButtonShowProfileClicked;
                            if (result.ContactType == type)
                                obscoll.Add(result);
                            break;
                    }
                }
            }
            return obscoll;
        }

        private void Result_ButtonShowProfileClicked(object sender, EventArgs e)
        {
            SelectedUser = sender as FriendicaUserExtended;
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
        }

        private ObservableCollection<FriendicaGroup> ConvertJsonToObsCollGroup(GetFriendicaGroups httpResult)
        {
            var groups = new List<FriendicaGroup>();
            var obscoll = new ObservableCollection<FriendicaGroup>();

            JsonArray resultArray = null;
            var testArray = JsonArray.TryParse(httpResult.ReturnString, out resultArray);

            if (testArray)
            {
                int arraySize = resultArray.Count;
                for (int i = 0; i < arraySize; i++)
                {
                    IJsonValue element = resultArray.GetArray()[i];
                    switch (element.ValueType)
                    {
                        case JsonValueType.Object:
                            var result = new FriendicaGroup(element.ToString());
                            groups.Add(result);
                            break;
                    }
                }
                foreach (var group in groups.OrderBy(group => group.GroupName))
                    obscoll.Add(group);
            }
            return obscoll;
        }


        public void ReloadContacts()
        {
            IsLoadingFriends = true;
            LoadFromApp();
            IsLoadingFriends = false;
        }


        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth")
                OnPropertyChanged("ListViewWidth");
            if (e.PropertyName == "ShellHeight")
                OnPropertyChanged("ListViewHeight");
            // if MobilePortrait is activated, recheck the height and width of the gridview
            if (e.PropertyName == "OrientationDevice")
            {
                OnPropertyChanged("ListViewHeight");
                // set number of columns depending on mobile device, otherwise columns are vanishing
                switch (App.Settings.OrientationDevice)
                {
                    case OrientationDeviceFamily.MobilePortrait:
                        MaxColumns = 4;
                        break;
                    case OrientationDeviceFamily.MobileLandscape:
                        MaxColumns = 7;
                        break;
                    default:
                        MaxColumns = 10;
                        break;
                }
            }
        }
    }
}