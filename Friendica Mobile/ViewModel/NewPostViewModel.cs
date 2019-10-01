using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ExifLib;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using Friendica_Mobile.Strings;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Friendica_Mobile.ViewModel
{
    public class NewPostViewModel : BaseViewModel
    {
        private const int ImageWarnLimit = 2097512; // is 2mb

        // model holding all data for the new post and the methods to send the post to the server
        private NewPostModel _newPost;
        public NewPostModel NewPost
        {
            get { return _newPost; }
            set { SetProperty(ref _newPost, value); }
        }

        // indicator that user is retweeting a post, showing a short explaining text
        private bool _isRetweeting;
        public bool IsRetweeting
        {
            get { return _isRetweeting; }
            set { SetProperty(ref _isRetweeting, value); }
        }

        // post which user wants to retweet
        public FriendicaPost RetweetPost { get; set; }
        
        // enables sending GPS coordinates with the post 
        private bool _sendCoordinates;
        public bool SendCoordinates
        {
            get { return _sendCoordinates; }
            set
            {
                SetProperty(ref _sendCoordinates, value);
                CheckCoordinatesImageVisible();
            }
        }

        // enables to send the GPS coordinates from the image
        private bool _sendCoordinatesImage;
        public bool SendCoordinatesImage
        {
            get { return _sendCoordinatesImage; }
            set { SetProperty(ref _sendCoordinatesImage, value); }
        }

        // triggers to show the SendCoordinatesImage switch
        private bool _sendCoordinatesImageVisible;
        public bool SendCoordinatesImageVisible
        {
            get { return _sendCoordinatesImageVisible; }
            set { SetProperty(ref _sendCoordinatesImageVisible, value); }
        }

        // triggers visibility of selected image or hint text instead
        private bool _imageLoaded;
        public bool ImageLoaded
        {
            get { return _imageLoaded; }
            set { SetProperty(ref _imageLoaded, value); }
        }

        // contains image as ImageSource for displaying it
        private ImageSource _newPostImage;
        public ImageSource NewPostImage
        {
            get { return _newPostImage; }
            set { SetProperty(ref _newPostImage, value);
                ImageLoaded = (value != null); }
        }

        // trigger to show an exclamation mark with the hint that picture is bigger than 2mb
        private bool _imageSizeIsBig;
        public bool ImageSizeIsBig
        {
            get { return _imageSizeIsBig; }
            set { SetProperty(ref _imageSizeIsBig, value); }
        }

        // contains latitude of image if available
        public double NewPostImageLatitude { get; set; }
        public double NewPostImageLongitude { get; set; }

        // triggers to show the ACL settings section
        private bool _showACLSettings;
        public bool ShowACLSettings
        {
            get { return _showACLSettings; }
            set { SetProperty(ref _showACLSettings, value); }
        }

        // stores index of selected item in radio button (public = 0, private = 1, no selection = -1)
        private int _postACLSetting;
        public int PostACLSetting
        {
            get { return _postACLSetting; }
            set
            {
                SetProperty(ref _postACLSetting, value);
                SaveSelectedToModel();
                OnPropertyChanged("IsPrivateACLVisible");
            }
        }

        // triggers visibility of details for private post settings
        public bool IsPrivateACLVisible
        {
            get { return (PostACLSetting == 1); }
        }

        // triggers visibility of listviews (false if loading has failed)
        private bool _isListViewsVisible;
        public bool IsListViewsVisible
        {
            get { return _isListViewsVisible; }
            set { SetProperty(ref _isListViewsVisible, value); }
        }

        // triggers visibility of a button to perform a reload of the contacts
        private bool _isButtonReloadVisible;
        public bool IsButtonReloadVisible
        {
            get { return _isButtonReloadVisible; }
            set { SetProperty(ref _isButtonReloadVisible, value); }
        }

        private ObservableCollection<SelectableData<FriendicaGroup>> _groups;
        public ObservableCollection<SelectableData<FriendicaGroup>> Groups
        {
            get { return _groups; }
            set { SetProperty(ref _groups, value); }
        }

        private ObservableCollection<SelectableData<FriendicaUser>> _friends;
        public ObservableCollection<SelectableData<FriendicaUser>> Friends
        {
            get { return _friends; }
            set { SetProperty(ref _friends, value); }
        }

        List<int> SelectedContacts { get; set; }
        List<int> SelectedGroups { get; set; }

        // trigger to show a hint if user hasn't selected any contact or group in private mode
        private bool _noContactsGroupsSelected;
        public bool NoContactsGroupsSelected
        {
            get { return _noContactsGroupsSelected; }
            set { SetProperty(ref _noContactsGroupsSelected, value); }
        }

        // user can set that current ACL setting should become new standard for future posts
        private bool _saveACLStandard;
        public bool SaveACLStandard
        {
            get { return _saveACLStandard; }
            set { SetProperty(ref _saveACLStandard, value); }
        }

        // trigger to show the current thread as a history reference
        private bool _showCurrentThread;
        public bool ShowCurrentThread
        {
            get { return _showCurrentThread; }
            set { SetProperty(ref _showCurrentThread, value); }
        }

        // container for current thread to be displayed as a history reference
        private ObservableCollection<FriendicaThread> _currentThread;
        public ObservableCollection<FriendicaThread> CurrentThread
        {
            get { return _currentThread; }
            set { SetProperty(ref _currentThread, value); }
        }


        #region Commands
        private ICommand _loadImageCommand;
        public ICommand LoadImageCommand => _loadImageCommand ?? (_loadImageCommand = new Command(LoadImage));
        private async void LoadImage()
        {
            // we need a different handling on macOS because Xamarin.Plugin.Media is not yet supporting macOS
            if (Device.RuntimePlatform == Device.macOS)
            {
                // ask user to select an image from filesystem
                var image = await DependencyService.Get<IImagePicker>().PickImage();

                // cancel if no image has been selected
                if (image == null)
                    return;

                try
                {
                    // display selected picture below editor box
                    NewPostImage = ImageSource.FromStream(() =>
                    {
                        // extract photo properties to get GPS coordinates if available
                        var stream = image.GetStream();
                        var picInfo = ExifReader.ReadJpeg(stream);
                        NewPostImageLatitude = ConvertGps(picInfo.GpsLatitude);
                        NewPostImageLongitude = ConvertGps(picInfo.GpsLongitude);
                        CheckCoordinatesImageVisible();

                        // prepare data of picture for sending to server
                        stream.Seek(0, SeekOrigin.Begin);
                        NewPost.Media = image.DataArray;
                        ImageSizeIsBig = (NewPost.Media.Length > ImageWarnLimit); 
                        return stream;
                    });
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.buttonLoadImage_Content,
                                            String.Format(AppResources.MessageDialogGeneralErrorMessage, ex.Message, ex.StackTrace),
                                            AppResources.buttonOK);
                }
                return;
            }
            else
            {
                // initialize Xamarin.Plugin.Media (supported on Android, iOS, UWP) and check if we are allowed to pick a photo from the gallery
                await CrossMedia.Current.Initialize();
                var allowed = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);

                if (!CrossMedia.Current.IsPickPhotoSupported || allowed == PermissionStatus.Denied)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.buttonLoadImage_Content,
                                            AppResources.MessageDialogPhotoGalleryForbidden, 
                                            AppResources.buttonOK);
                    return;
                }

                // now ask user to pick one photo (currently we don't allow multiple selections as API is not capable of more than one image and it's a matter of HTTP/POST size in php.ini)
                // large image = 75% of original, slightly compressed by 92% to shrink size a little bit
                MediaFile file = null;
                try
                {
                    file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    { PhotoSize = PhotoSize.Large, CompressionQuality = 92 });
                }
                catch (MediaPermissionException ex)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.buttonLoadImage_Content,
                                            AppResources.MessageDialogPhotoGalleryForbidden,
                                            AppResources.buttonOK);
                    return;
                }

                // cancel silently if no photo has been selected
                if (file == null)
                    return;

                // display selected picture below editor box
                NewPostImage = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    return stream;
                });

                // extract photo properties to get GPS coordinates if available
                using (var streamExif = file.GetStream())
                {
                    var picInfo = ExifReader.ReadJpeg(streamExif);
                    NewPostImageLatitude = ConvertGps(picInfo.GpsLatitude);
                    NewPostImageLongitude = ConvertGps(picInfo.GpsLongitude);
                    CheckCoordinatesImageVisible();
                }

                // prepare data of picture for sending to server
                using (var memoryStream = new MemoryStream())
                {
                    file.GetStream().CopyTo(memoryStream);
                    NewPost.Media = memoryStream.ToArray();
                    ImageSizeIsBig = (NewPost.Media.Length > ImageWarnLimit); 
                }
                return;
            }
        }


        private ICommand _deleteImageCommand;
        public ICommand DeleteImageCommand => _deleteImageCommand ?? (_deleteImageCommand = new Command(DeleteImage));
        private void DeleteImage()
        {
            // reset all image information
            NewPostImage = null;
            NewPostImageLatitude = 0.0;
            NewPostImageLongitude = 0.0;
            NewPost.Media = null;
            NewPost.Latitude = 0.0;
            NewPost.Longitude = 0.0;
            CheckCoordinatesImageVisible();
        }


        private ICommand _showHintOnImageCommand;
        public ICommand ShowHintOnImageCommand => _showHintOnImageCommand ?? (_showHintOnImageCommand = new Command(ShowHintOnImage));
        private void ShowHintOnImage()
        {
            // display a dialog if user clicks on the warning sign on images if size exceeding the limit
            var imageSize = Math.Round((double)NewPost.Media.Length / 1048576, 1).ToString() + " MB";
            var limitSize = Math.Round((double)ImageWarnLimit / 1048576, 1).ToString() + " MB";
            Application.Current.MainPage.DisplayAlert(AppResources.pageTitleNewPost_Text,
                                string.Format(AppResources.MessageDialogNewPostImageExceedWarnLimit, imageSize, limitSize),
                                AppResources.buttonOK);
        }


        private ICommand _reloadContactsCommand;
        public ICommand ReloadContactsCommand => _reloadContactsCommand ?? (_reloadContactsCommand = new Command(ReloadContacts));
        async void ReloadContacts()
        {
            await ReloadContactData();
        }
#endregion


        public NewPostViewModel()
        {
            Title = AppResources.pageTitleNewPost_Text;
            NewPost = new NewPostModel();
            CurrentThread = new ObservableCollection<FriendicaThread>();

            // init HttpFriendicaContacts
            if (App.Contacts == null)
                App.Contacts = new HttpFriendicaContacts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            if (Settings.IsFriendicaLoginDefined())
                ReloadContactData();

            // retrieve standard setting of user
            SendCoordinates = Settings.SendCoordinatesAllowed;
            // retrieve standard setting for ACL
            PostACLSetting = ConvertDefaultACLSetting();
        }


        /// <summary>
        /// Clean up the html text extracted by the JS function and set the HtmlStatus in the new post
        /// </summary>
        /// <param name="html"></param>
        public void SetHtmlStatus(string html)
        {
            // remove empty lines at the end of the post
            while (html.EndsWith("\\n", StringComparison.InvariantCulture))
            {
                html = html.Substring(0, html.Length - 2);
            }
            while (html.EndsWith("<div><br></div>", StringComparison.InvariantCulture))
                html = html.Substring(0, html.Length - 15);

            // shorten empty lines to separate paragraphs
            html = html.Replace("<div><br></div>", "<br>");
            NewPost.HtmlStatus = html;
        }


        /// <summary>
        /// send the new post to server
        /// </summary>
        /// <returns></returns>
        public async Task SendNewPostAsync()
        {
            // do not continue with sending if we are in demo mode
            if (!Settings.IsFriendicaLoginDefined())
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleNewPost_Text,
                            AppResources.messageDialogNewPostNoSettings,
                            AppResources.buttonOK);
                ActivityIndicatorVisible = false;
                return;
            }

            // cancel operation if there is no text or image in the new post
            if (string.IsNullOrEmpty(NewPost.HtmlStatus) && NewPost.Media == null)
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleNewPost_Text,
                    AppResources.MessageDialogNewPostNoContentSendCancelled,
                            AppResources.buttonOK);
                ActivityIndicatorVisible = false;
                return;
            }

            // show user that we are now transferring the new post to server
            ActivityIndicatorText = AppResources.textblockIndicatorSendingNewPost_Text;
            ActivityIndicatorVisible = true;

            // set coordinates to be sent with the posting
            await SetCoordinatesAsync();

            // let's perform a check on the settings, warn user if a public post could be sent unintentionally
            if (PostACLSetting != 0 && (string.IsNullOrEmpty(NewPost.ContactAllow) && string.IsNullOrEmpty(NewPost.GroupAllow)))
            {
                // don't warn if we are commenting an existing post, as the comment always takes the permissions from the original posts
                if (NewPost.InReplyToStatusId == 0)
                {
                    var answer = await Application.Current.MainPage.DisplayAlert(AppResources.buttonNewPostSend_Content,
                                AppResources.MessageDialogNewPostPublicPostOk,
                                AppResources.buttonYes, AppResources.buttonNo);
                    if (!answer)
                    {
                        ActivityIndicatorVisible = false;
                        return;
                    }
                }
            }

            // save current ACL settings to AppSettings if user wishes this
            if (SaveACLStandard)
            {
                switch (PostACLSetting)
                {
                    case 0:
                        Settings.ACLPublicPost = true;
                        Settings.ACLPrivatePost = false;
                        break;
                    case 1:
                        Settings.ACLPublicPost = false;
                        Settings.ACLPrivatePost = true;
                        SaveSelectedToModel(true);
                        break;
                    default:
                        Settings.ACLPublicPost = false;
                        Settings.ACLPrivatePost = false;
                        break;
                }
            }

            // add retweeted content if needed
            if (IsRetweeting && RetweetPost != null)
            {
                var retweetContent = RetweetPost.CreateRetweetContent();
                NewPost.HtmlStatus += retweetContent;
            }

            // send to server through admin class (to send more than one post in parallel if it takes longer)
            if (App.NewPosts == null)
                App.NewPosts = new NewPostsAdmin();
            App.NewPosts.NewPosts.Add(NewPost);

            // move back to previous page
            ActivityIndicatorVisible = false;
            NavigateBack();
        }


        private double ConvertGps(double[] exifGps)
        {
            // convert array of double values for coordinates (degree, minutes, second) into one single double value
            return exifGps[0] + exifGps[1] / 60 + exifGps[2] / 3600;
        }


        private void CheckCoordinatesImageVisible()
        {
            // display the slider for sending the image coordinates
            if (SendCoordinates)
            {
                var hasCoordinates = (!(NewPostImageLatitude == 0.0 && NewPostImageLongitude == 0.0));
                SendCoordinatesImageVisible = hasCoordinates;
                SendCoordinatesImage = hasCoordinates;
            }
            else
                SendCoordinatesImageVisible = false;
        }


        private async Task SetCoordinatesAsync()
        {
            // get location coordinates (latitude, longitude) to be sent along with the new post
            if (SendCoordinates)
            {
                if (SendCoordinatesImage && !(NewPostImageLatitude == 0.0 && NewPostImageLongitude == 0.0))
                {
                    // we have image coordinates and are allowed to send them
                    NewPost.Latitude = NewPostImageLatitude;
                    NewPost.Longitude = NewPostImageLongitude;
                }
                else
                {
                    // we don't have image coordinates or are not allowed to use them
                    // get coordinates from device and send this info (not available on macOS)
                    try
                    {
                        if (Device.RuntimePlatform != Device.macOS)
                        {
                            var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                            var location = await Geolocation.GetLocationAsync(request);

                            if (location != null)
                            {
                                NewPost.Latitude = location.Latitude;
                                NewPost.Longitude = location.Longitude;
                            }
                        }
                        else
                        {
                            NewPost.Latitude = 0.0;
                            NewPost.Longitude = 0.0;
                        }
                    }
                    catch (PermissionException)
                    {
                        await Application.Current.MainPage.DisplayAlert(AppResources.appBarNetworkAdd_Label,
                                            AppResources.MessageDialogLocationServicesNotAllowed,
                                            AppResources.buttonOK);
                    }
                }
            }
            else
            {
                // not allowed to send any coordinates
                SendCoordinatesImageVisible = false;
                NewPost.Latitude = 0.0;
                NewPost.Longitude = 0.0;
            }
        }


        private int ConvertDefaultACLSetting()
        {
            // convert stored setting (bool for private or public) to int index for radio button
            var publicPost = Settings.ACLPublicPost;
            var privatePost = Settings.ACLPrivatePost;

            if (publicPost && !privatePost)
                return 0;
            if (!publicPost && privatePost)
                return 1;
            else
                return -1;
        }


        private async Task ReloadContactData()
        {
            // loads the contact data, if not yet loaded
            if (App.Contacts == null)
                return;

            if (App.Contacts.Friends == null && App.Contacts.Groups == null)
            {
                ActivityIndicatorText = AppResources.TextSettingsLoadingContacts;
                ActivityIndicatorVisible = true;
                var result = await App.Contacts.GetStatusesFriends();
                var resultGroup = await App.Contacts.GetFriendicaGroupShow();
                ActivityIndicatorVisible = false;

                // now work with the results
                if (result == HttpFriendicaContacts.GetFriendicaFriendsResults.OK &&
                   resultGroup == HttpFriendicaContacts.GetFriendicaFriendsResults.OK)
                {
                    IsButtonReloadVisible = false;
                    // display an info text if there were no contacts returned by server
                    if (App.Contacts.Friends == null && App.Contacts.Groups == null)
                    {
                        IsListViewsVisible = false;
                        LabelServerActivityFailed = AppResources.textblockNoDataAvailableFriends_Text;
                        return;
                    }
                    IsListViewsVisible = true;
                    CreateListViews();
                }
                else
                {
                    // display error messages
                    ErrorMessages(result);
                    ErrorMessages(resultGroup);
                }
            }
            else
            {
                IsListViewsVisible = true;
                CreateListViews();
            }
        }


        private void ErrorMessages(HttpFriendicaContacts.GetFriendicaFriendsResults result)
        {
            // handle different results from loading the contacts
            switch (result)
            {
                case HttpFriendicaContacts.GetFriendicaFriendsResults.NotAnswered:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = true;
                    LabelServerActivityFailed = AppResources.messageDialogHttpStatusNone;
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.NotImplemented:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = false;
                    // show a hint if server version is too old to retrieve group data
                    LabelServerActivityFailed = AppResources.textblockNoGroupsAvailable_Text;
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.SerializationError:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = true;
                    // show error with requested URL and the returned JSON upon serialization error
                    LabelServerActivityFailed = String.Format(AppResources.TextSerializationError,
                                                        App.Contacts._requestedUrl, App.Contacts.ReturnString);
                    break;
                case HttpFriendicaContacts.GetFriendicaFriendsResults.UnexpectedError:
                    IsListViewsVisible = false;
                    IsButtonReloadVisible = true;
                    LabelServerActivityFailed = String.Format(AppResources.TestConnectionResultUnexpectedError,
                                                              App.Contacts.StatusCode, App.Contacts.ErrorMessage, App.Contacts.ErrorHResult);
                    break;
            }
        }


        private void CreateListViews()
        {
            // enhance contacts with SelectableData to be used in Listviews and test against stored selections
            SelectedContacts = GetSelectedContactsFromSettings();
            SelectedGroups = GetSelectedGroupsFromSettings();

            if (App.Contacts.Groups != null)
            {
                Groups = new ObservableCollection<SelectableData<FriendicaGroup>>();
                foreach (var group in App.Contacts.Groups)
                {
                    var selectedData = new SelectableData<FriendicaGroup>(group)
                    {
                        IsSelected = (SelectedGroups.Contains(group.GroupGid))
                    };
                    selectedData.PropertyChanged += (sender, e) => { SaveSelectedToModel(); };
                    Groups.Add(selectedData);
                }
            }

            if (App.Contacts.Friends != null)
            {
                Friends = new ObservableCollection<SelectableData<FriendicaUser>>();
                foreach (var friend in App.Contacts.Friends)
                {
                    var selectedData = new SelectableData<FriendicaUser>(friend)
                    {
                        IsSelected = (SelectedContacts.Contains(friend.UserCid))
                    };
                    selectedData.PropertyChanged += (sender, e) => { SaveSelectedToModel(); };
                    Friends.Add(selectedData);
                }
            }
            SaveSelectedToModel();
        }


        void SaveSelectedToModel(bool saveSettings = false)
        {
            // create temporary list with selected contacts, if saveSetting = true we will also save this permanently to the settings
            if (Friends == null || Groups == null)
                return;

            if (PostACLSetting == 1)
            {
                var selectedContacts = Friends.Where(f => f.IsSelected).OrderBy(f => f.Data.UserCid);
                var selectedIds = "";
                foreach (var friend in selectedContacts)
                {
                    selectedIds += "<" + friend.Data.UserCid + ">";
                }
                if (saveSettings)
                    Settings.ACLPrivateSelectedContacts = selectedIds;
                else
                    NewPost.ContactAllow = selectedIds;

                var selectedGroups = Groups.Where(g => g.IsSelected).OrderBy(g => g.Data.GroupGid);
                selectedIds = "";
                foreach (var group in selectedGroups)
                {
                    selectedIds += "<" + group.Data.GroupGid + ">";
                }
                if (saveSettings)
                    Settings.ACLPrivateSelectedGroups = selectedIds;
                else
                    NewPost.GroupAllow = selectedIds;

                NoContactsGroupsSelected = (string.IsNullOrEmpty(NewPost.ContactAllow) && string.IsNullOrEmpty(NewPost.GroupAllow));
            }
            else
            {
                NewPost.ContactAllow = "";
                NewPost.GroupAllow = "";
            }
        }


        private List<int> GetSelectedContactsFromSettings()
        {
            var list = new List<int>();
            var cids = Regex.Split(Settings.ACLPrivateSelectedContacts, @"<");
            foreach (var cid in cids)
            {
                if (cid != "")
                {
                    var cidClean = cid.Replace("<", "");
                    cidClean = cid.Replace(">", "");
                    list.Add(Convert.ToInt32(cidClean));
                }
            }
            return list;
        }


        private List<int> GetSelectedGroupsFromSettings()
        {
            var list = new List<int>();
            var gids = Regex.Split(Settings.ACLPrivateSelectedGroups, @"<");
            foreach (var gid in gids)
            {
                if (gid != "")
                {
                    var gidClean = gid.Replace("<", "");
                    gidClean = gid.Replace(">", "");
                    list.Add(Convert.ToInt32(gidClean));
                }
            }
            return list;
        }
    }
}
