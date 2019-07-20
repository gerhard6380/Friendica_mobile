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
    public class HomeViewModel : BaseViewModel
    {
        // connects to instance in App where all data for Home, Network and Newsfeed is stored
        public PostsModel PostsModel
        {
            get { return App.Posts; }
        }

        // disables button if currently a loading process (initial load or refresh itself) is active
        private bool _refreshHomeEnabled;
        public bool RefreshHomeEnabled
        {
            get { return _refreshHomeEnabled; }
            set { SetProperty(ref _refreshHomeEnabled, value); }
        }


#region Commands
        private ICommand _refreshHomeCommand;
        public ICommand RefreshHomeCommand => _refreshHomeCommand ?? (_refreshHomeCommand = new Command(RefreshHome));
        private async void RefreshHome()
        {
            // TODO: refresh will also trigger refresh for newsfeed, should also refresh home, and later messages, pictures?
            ActivityIndicatorText = AppResources.textblockIndicatorRefreshing_Text;
            ActivityIndicatorVisible = true;
            RefreshHomeEnabled = false;

            if (!Settings.IsFriendicaLoginDefined())
            {
                // show hint on NoSettings to user once and then abort
                if (!App.HomeNoSettingsAlreadyShownRefresh)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.appBarNetworkRefresh_Label, AppResources.messageDialogNetworkNoSettings, "OK");
                    App.HomeNoSettingsAlreadyShownRefresh = true;
                }
                else
                    await Task.Delay(3000);
            }
            else
            {
                // remove new item indicators for home items
                PostsModel.ExecuteSetAllSeen(PostsModel.ThreadType.Home);
                // perform on refreshing
                await App.Posts.LoadNewPostsAsync();
            }

            RefreshHomeEnabled = true;
            ActivityIndicatorVisible = false;
        }

        private ICommand _addNewPostCommand;
        public ICommand AddNewPostCommand => _addNewPostCommand ?? (_addNewPostCommand = new Command(AddNewPost));
        private async void AddNewPost()
        {
            // TODO: implement routine = navigation to A0_NewPost.xaml
            // TODO: don't forget to show an activity indicator after returning from newPost during sending to server
            // TODO: textblockIndicatorSendingNewPost_Text
            await Application.Current.MainPage.DisplayAlert("", "### new Post noch nicht implementiert ###", AppResources.buttonOK);
        }

        private ICommand _navigateSettingsCommand;
        public ICommand NavigateSettingsCommand => _navigateSettingsCommand ?? (_navigateSettingsCommand = new Command(NavigateSettings));
        private void NavigateSettings()
        {
            NavigateTo(new Views.Settings());
        }


#endregion

        public HomeViewModel()

        {
            Title = AppResources.pageTitleHome_Text;

            // init PostsModel
            if (App.Posts == null)
                App.Posts = new PostsModel();
                
            StartLoad();
        }

        private async void StartLoad()
        {
            // start loading data if not yet available
            ActivityIndicatorText = AppResources.textblockHomeIsLoadingInitial_Text;
            ActivityIndicatorVisible = true;
            RefreshHomeEnabled = false;
            await App.Posts.LoadNewPostsAsync();
            RefreshHomeEnabled = true;
            ActivityIndicatorVisible = false;
        }

        public async Task LoadNextAsync()
        {
            if (PostsModel.IsLoadingNext)
                return;

            await App.Posts.LoadNextHomeAsync();
        }
    }
}
