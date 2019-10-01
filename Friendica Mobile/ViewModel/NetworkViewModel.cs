﻿using Friendica_Mobile.Strings;
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
    public class NetworkViewModel : BaseViewModel
    {
        // connects to instance in App where all data for Home, Network and Newsfeed is stored
        public PostsModel PostsModel
        {
            get { return App.Posts; }
        }

        // disables button if currently a loading process (initial load or refresh itself) is active
        private bool _refreshNetworkEnabled;
        public bool RefreshNetworkEnabled
        {
            get { return _refreshNetworkEnabled; }
            set { SetProperty(ref _refreshNetworkEnabled, value); }
        }


#region Commands
        private ICommand _refreshNetworkCommand;
        public ICommand RefreshNetworkCommand => _refreshNetworkCommand ?? (_refreshNetworkCommand = new Command(RefreshNetwork));
        private async void RefreshNetwork()
        {
            // TODO: refresh will also trigger refresh for newsfeed, should also refresh home, and later messages, pictures?
            ActivityIndicatorText = AppResources.textblockIndicatorRefreshing_Text;
            ActivityIndicatorVisible = true;
            RefreshNetworkEnabled = false;

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
                // remove new item indicators for network items
                PostsModel.ExecuteSetAllSeen(PostsModel.ThreadType.Network);
                // perform on refreshing
                await App.Posts.LoadNewPostsAsync();
            }

            RefreshNetworkEnabled = true;
            ActivityIndicatorVisible = false;
        }

        private ICommand _addNewPostCommand;
        public ICommand AddNewPostCommand => _addNewPostCommand ?? (_addNewPostCommand = new Command(AddNewPost));
        private async void AddNewPost()
        {
            // TODO: don't forget to show an activity indicator after returning from newPost during sending to server
            // TODO: textblockIndicatorSendingNewPost_Text
            NavigateTo(new Views.NewPost());
        }

        private ICommand _navigateSettingsCommand;
        public ICommand NavigateSettingsCommand => _navigateSettingsCommand ?? (_navigateSettingsCommand = new Command(NavigateSettings));
        private void NavigateSettings()
        {
            NavigateTo(new Views.Settings());
        }

#endregion

        public NetworkViewModel()

        {
            Title = AppResources.pageTitleNetwork_Text;

            // init PostsModel
            if (App.Posts == null)
                App.Posts = new PostsModel();
                
            StartLoad();
        }

        private async void StartLoad()
        {
            // start loading data if not yet available
            ActivityIndicatorText = AppResources.textblockNetworkIsLoadingInitial_Text;
            ActivityIndicatorVisible = true;
            RefreshNetworkEnabled = false;
            await App.Posts.LoadNewPostsAsync();
            RefreshNetworkEnabled = true;
            ActivityIndicatorVisible = false;
        }

        public async Task LoadNextAsync()
        {
            if (PostsModel.IsLoadingNext)
                return;

            await App.Posts.LoadNextNetworkAsync();
        }
    }
}
