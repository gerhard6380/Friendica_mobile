using Friendica_Mobile.Strings;
using Friendica_Mobile.HttpRequests;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Friendica_Mobile.Models;
using MvvmHelpers;
using System.Linq;
using System.Threading.Tasks;

namespace Friendica_Mobile.ViewModel
{
    public class NewsfeedViewModel : BaseViewModel
    {
        // connects to instance in App where all data for Home, Network and Newsfeed is stored
        public PostsModel PostsModel
        {
            get { return App.Posts; }
        }

        // disables button if currently a loading process (initial load or refresh itself) is active
        private bool _refreshNewsfeedEnabled;
        public bool RefreshNewsfeedEnabled
        {
            get { return _refreshNewsfeedEnabled; }
            set { SetProperty(ref _refreshNewsfeedEnabled, value); }
        }


#region Commands
        private ICommand _refreshNewsfeedCommand;
        public ICommand RefreshNewsfeedCommand => _refreshNewsfeedCommand ?? (_refreshNewsfeedCommand = new Command(RefreshNewsfeed));
        private async void RefreshNewsfeed()
        {
            // TODO: refresh will also trigger refresh for newsfeed, should also refresh home, and later messages, pictures?
            ActivityIndicatorText = AppResources.textblockIndicatorRefreshing_Text;
            ActivityIndicatorVisible = true;
            RefreshNewsfeedEnabled = false;

            if (!Settings.IsFriendicaLoginDefined())
            {
                // show hint on NoSettings to user once and then abort
                if (!App.NetworkNoSettingsAlreadyShownRefresh)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.appBarNetworkRefresh_Label, AppResources.messageDialogNetworkNoSettings, "OK");
                    App.NetworkNoSettingsAlreadyShownRefresh = true;
                }
                else
                    await Task.Delay(3000);
            }
            else
            {
                // not setting seen because user must do this manually
                // perform on refreshing
                await App.Posts.LoadNewPostsAsync();
            }

            RefreshNewsfeedEnabled = true;
            ActivityIndicatorVisible = false;
        }

        private ICommand _newsfeedSetAllSeenCommand;
        public ICommand NewsfeedSetAllSeenCommand => _newsfeedSetAllSeenCommand ?? (_newsfeedSetAllSeenCommand = new Command(NewsfeedSetAllSeen));
        private void NewsfeedSetAllSeen()
        {
            // disable the button during activity
            RefreshNewsfeedEnabled = false;
            PostsModel.ExecuteSetAllSeen(PostsModel.ThreadType.Newsfeed);
            RefreshNewsfeedEnabled = true;
        }

        private ICommand _navigateSettingsCommand;
        public ICommand NavigateSettingsCommand => _navigateSettingsCommand ?? (_navigateSettingsCommand = new Command(NavigateSettings));
        private void NavigateSettings()
        {
            NavigateTo(new Views.Settings());
        }

#endregion

        public NewsfeedViewModel()

        {
            Title = AppResources.pageTitleNewsfeed_Text;

            // init PostsModel
            if (App.Posts == null)
                App.Posts = new PostsModel();

            StartLoad();
        }

        public async void StartLoad()
        {
            // start loading data if not yet available
            ActivityIndicatorText = AppResources.textblockNewsfeedIsLoadingInitial_Text;
            ActivityIndicatorVisible = true;
            RefreshNewsfeedEnabled = false;
            await App.Posts.LoadNewPostsAsync();
            RefreshNewsfeedEnabled = true;
            ActivityIndicatorVisible = false;
        }

    }
}
