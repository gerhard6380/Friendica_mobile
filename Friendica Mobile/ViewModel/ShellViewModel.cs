using System.Windows.Input;
using Friendica_Mobile.Styles;
using Xamarin.Forms;
using Friendica_Mobile.Views;

namespace Friendica_Mobile.ViewModel
{
    // manages commands to be executed from menu entries defined in Shell.xaml
    public class ShellViewModel : MasterDetailControlViewModel
    {
        // create commands for each button
        private ICommand _toHomeCommand;
        public ICommand ToHomeCommand
        {
            get { return _toHomeCommand ?? (_toHomeCommand = new Command(ToHome)); }
        }
        private void ToHome()
        {
            NavigateTo(new Views.Home());
        }

        private ICommand _toNetworkCommand;
        public ICommand ToNetworkCommand
        {
            get { return _toNetworkCommand ?? (_toNetworkCommand = new Command(ToNetwork)); }
        }
        private void ToNetwork()
        {
            NavigateTo(new Views.Network());
        }

        private ICommand _toNewsfeedCommand;
        public ICommand ToNewsfeedCommand
        {
            get { return _toNewsfeedCommand ?? (_toNewsfeedCommand = new Command(ToNewsfeed)); }
        }
        private void ToNewsfeed()
        {
            NavigateTo(new Views.Newsfeed());
        }

        private ICommand _toContactsCommand;
        public ICommand ToContactsCommand
        {
            get { return _toContactsCommand ?? (_toContactsCommand = new Command(ToContacts)); }
        }
        private void ToContacts()
        {
            NavigateTo(new Views.Contacts());
        }

        private ICommand _toMessagesCommand;
        public ICommand ToMessagesCommand
        {
            get { return _toMessagesCommand ?? (_toMessagesCommand = new Command(ToMessages)); }
        }
        private void ToMessages()
        {
            NavigateTo(new Views.Messages());
        }

        private ICommand _toProfilesCommand;
        public ICommand ToProfilesCommand
        {
            get { return _toProfilesCommand ?? (_toProfilesCommand = new Command(ToProfiles)); }
        }
        private void ToProfiles()
        {
            NavigateTo(new Views.Profiles());
        }

        private ICommand _toPhotosCommand;
        public ICommand ToPhotosCommand
        {
            get { return _toPhotosCommand ?? (_toPhotosCommand = new Command(ToPhotos)); }
        }
        private void ToPhotos()
        {
            NavigateTo(new Views.Photos());
        }

        private ICommand _toVideosCommand;
        public ICommand ToVideosCommand
        {
            get { return _toVideosCommand ?? (_toVideosCommand = new Command(ToVideos)); }
        }
        private void ToVideos()
        {
            NavigateTo(new Views.Videos());
        }

        private ICommand _toEventsCommand;
        public ICommand ToEventsCommand
        {
            get { return _toEventsCommand ?? (_toEventsCommand = new Command(ToEvents)); }
        }
        private void ToEvents()
        {
            NavigateTo(new Views.Events());
        }

        private ICommand _toPersonalNotesCommand;
        public ICommand ToPersonalNotesCommand
        {
            get { return _toPersonalNotesCommand ?? (_toPersonalNotesCommand = new Command(ToPersonalNotes)); }
        }
        private void ToPersonalNotes()
        {
            NavigateTo(new Views.PersonalNotes());
        }

        private ICommand _toSettingsCommand;
        public ICommand ToSettingsCommand
        {
            get { return _toSettingsCommand ?? (_toSettingsCommand = new Command(ToSettings)); }
        }
        private void ToSettings()
        {
            NavigateTo(new Views.Settings());
        }

        private ICommand _flightModeCommand;
        public ICommand FlightModeCommand
        {
            get { return _flightModeCommand ?? (_flightModeCommand = new Command(FlightMode)); }
        }
        private void FlightMode()
        {
            // TODO: enable/disable notifications - per plattform implementation needed
        }

        private ICommand _toHelpCommand;
        public ICommand ToHelpCommand
        {
            get { return _toHelpCommand ?? (_toHelpCommand = new Command(ToHelp)); }
        }
        private void ToHelp()
        {
            NavigateTo(new Views.Help());
        }

        private ICommand _toAboutCommand;
        public ICommand ToAboutCommand
        {
            get { return _toAboutCommand ?? (_toAboutCommand = new Command(ToAbout)); }
        }
        private void ToAbout()
        {
            NavigateTo(new Views.About());
        }


        // add a counter for each of the buttons where we want to display a counter of new elements
        private int _networkCounter;
        public int NetworkCounter
        {
            get { return _networkCounter; }
            set { SetProperty(ref _networkCounter, value); }
        }

        private int _newsfeedCounter;
        public int NewsfeedCounter
        {
            get { return _newsfeedCounter; }
            set { SetProperty(ref _newsfeedCounter, value); }
        }

    }
}