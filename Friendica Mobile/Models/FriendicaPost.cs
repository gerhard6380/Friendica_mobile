using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Input;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Strings;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Friendica_Mobile.HttpRequests.HttpFriendicaPosts;
using static Friendica_Mobile.Models.FriendicaActivities;

namespace Friendica_Mobile.Models
{
    public class FriendicaPost : BindableClass
    {
        public enum PostTypes { UserGenerated, Newsfeed }

        // property containing the original post returned from server
        private JsonFriendicaPost _post;
        public JsonFriendicaPost Post
        {
            get { return _post; }
            set { _post = value;
                if (Post.PostSource != null)
                {
                    if (Post.PostSource.ToLower().Contains("rss/atom"))
                        PostType = PostTypes.Newsfeed;
                    else
                        PostType = PostTypes.UserGenerated;
                }

                PostHtml = (string.IsNullOrEmpty(Post.PostRenderedHtml)) ? Post.PostStatusnetHtml : Post.PostRenderedHtml;

                if (Post.PostInReplyToStatusId > 0)
                    IsComment = true;
                else
                    IsComment = false;

                CreatedAtDateTime = DateTime.ParseExact(Post.PostCreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
                CreatedAtLocalized = CreatedAtDateTime.ToString("ddd") + " " + CreatedAtDateTime.ToString("d") + " " + CreatedAtDateTime.ToString("t");

                SetSenderNameConcat();

                // set flag for showing textblock with location and add globe icon to the location
                // TO DO: missing location in Posts
                PostHasLocation = !(string.IsNullOrEmpty(Post.PostLocation));
                if (PostHasLocation)
                    LocationWithIcon = Post.PostLocation + " " + WebUtility.HtmlDecode("&#x1F30D;");

                SetIsNewEntry();
                SetActivitiesParameters();
            }
        }

        // property containing the html code, if available we will use the rendered_html, else the statusnet_html
        // the statusnet_html has less information, e.g. RSS feeds don#t contain preview stuff in statusment_html but probably in renderend_html
        private string _postHtml;
        public string PostHtml
        {
            get { return _postHtml; }
            set { SetProperty(ref _postHtml, value); }
        }

        // property containing the converted retweeted status (extended by commands, events etc.)
        private FriendicaPost _retweetedStatus;
        public FriendicaPost RetweetedStatus
        {
            get { return _retweetedStatus; }
            set { _retweetedStatus = value;
                SetSenderNameConcat(); }
        }

        //// store activity on newsfeed items where user wants to like/dislike them
        //private FriendicaActivity _intendedActivity;
        //public FriendicaActivity IntendedActivity
        //{
        //    get { return _intendedActivity; }
        //    set { _intendedActivity = value; }
        //}

        // property containing the type of the post
        private PostTypes _postType;
        public PostTypes PostType
        {
            get { return _postType; }
            set { _postType = value; }
        }

        // property for indicating that post is a new one
        private bool _isNewEntry;
        public bool IsNewEntry
        {
            get { return _isNewEntry; }
            set { SetProperty(ref _isNewEntry, value);
            }
        }

        // property for indicating that post is a comment
        private bool _isComment;
        public bool IsComment
        {
            get { return _isComment; }
            set { _isComment = value; }
        }

        // property indicating that comment will be displayed or not (toggle button changed this state)
        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        // DateTime of created_at to sort comments
        private DateTime _createdAtDateTime;
        public DateTime CreatedAtDateTime
        {
            get { return _createdAtDateTime; }
            set { _createdAtDateTime = value; }
        }

        // localized version of the date
        private string _createdAtLocalized;
        public string CreatedAtLocalized
        {
            get { return _createdAtLocalized; }
            set { _createdAtLocalized = value; }
        }

        // concat names if user is retweeting a post
        private string _senderNameConcat;
        public string SenderNameConcat
        {
            get { return _senderNameConcat; }
            set { SetProperty(ref _senderNameConcat, value); }
        }

        // show button for map only if post has a location to show on map
        private bool _postHasLocation;
        public bool PostHasLocation
        {
            get { return _postHasLocation; }
            set { _postHasLocation = value; }
        }

        // add the world logo to the location string
        private string _locationWithIcon;
        public string LocationWithIcon
        {
            get { return _locationWithIcon; }
            set { _locationWithIcon = value; }
        }

        // indicator that server is not supporting likes at the moment (lists of users are null)
        private bool _activitiesNotSupported;
        public bool ActivitiesNotSupported
        {
            get { return _activitiesNotSupported; }
            set { _activitiesNotSupported = value; }
        }

        // indicator for showing like symbol in color or not
        private bool _isLikedByMe;
        public bool IsLikedByMe
        {
            get { return _isLikedByMe; }
            set
            {
                SetProperty(ref _isLikedByMe, value);
                ButtonLikeText = (value) ? AppResources.ButtonUnLikeText : AppResources.ButtonLikeText;
            }
        }

        // indicator for showing dislike symbol in color or not
        private bool _isDislikedByMe;
        public bool     IsDislikedByMe
        {
            get { return _isDislikedByMe; }
            set
            {
                SetProperty(ref _isDislikedByMe, value);
                ButtonDislikeText = (value) ? AppResources.ButtonUnDislikeText : AppResources.ButtonDislikeText;
            }
        }

        // string showing the count of likes
        private string _countLikes;
        public string CountLikes
        {
            get { return _countLikes; }
            set { SetProperty(ref _countLikes, value); }
        }

        // string showing the count of dislikes
        private string _countDislikes;
        public string CountDislikes
        {
            get { return _countDislikes; }
            set { SetProperty(ref _countDislikes, value); }
        }

        // indicator if likes are visible 
        private bool _isLikesVisible;
        public bool IsLikesVisible
        {
            get { return _isLikesVisible; }
            set { SetProperty(ref _isLikesVisible, value); }
        }

        // indicator if dislikes are visible 
        private bool _isDislikesVisible;
        public bool IsDislikesVisible
        {
            get { return _isDislikesVisible; }
            set { SetProperty(ref _isDislikesVisible, value); }
        }

        // string shown in button 
        private string _buttonLikeText = "blablabla";
        public string ButtonLikeText
        {
            get { return _buttonLikeText; }
            set { SetProperty(ref _buttonLikeText, value); }
        }

        // string shown in button 
        private string _buttonDislikeText;
        public string ButtonDislikeText
        {
            get { return _buttonDislikeText; }
            set { SetProperty(ref _buttonDislikeText, value); }
        }

        // List with Likes
        private List<FriendicaUser> _likesForDisplay;
        public List<FriendicaUser> LikesForDisplay
        {
            get { return _likesForDisplay; }
            set { _likesForDisplay = value; }
        }

        // List with Likes
        private List<FriendicaUser> _dislikesForDisplay;
        public List<FriendicaUser> DislikesForDisplay
        {
            get { return _dislikesForDisplay; }
            set { _dislikesForDisplay = value; }
        }

        // indicator for showing progress ring for like button
        private bool _isUpdatingLikes;
        public bool IsUpdatingLikes
        {
            get { return _isUpdatingLikes; }
            set { SetProperty(ref _isUpdatingLikes, value); }
        }

        // indicator for showing progress ring for dislike button
        private bool _isUpdatingDislikes;
        public bool IsUpdatingDislikes
        {
            get { return _isUpdatingDislikes; }
            set { SetProperty(ref _isUpdatingDislikes, value); }
        }


        // eventhandlers
        public event EventHandler ButtonShowProfileClicked;
        public event EventHandler ButtonRetweetClicked;
        public event EventHandler LikeNewsfeedClicked;
        public event EventHandler ButtonAddCommentClicked;


#region Commands
        private ICommand _loadMapCommand;
        public ICommand LoadMapCommand => _loadMapCommand ?? (_loadMapCommand = new Command(LoadMap));
        private async void LoadMap()
        {
            // load map with coordinates if available, otherwise try with location name
            if (Post.PostGeo != null && Post.PostGeo.FriendicaGeoCoordinates != null)
            {
                try
                {
                    var latitude = Post.PostGeo.FriendicaGeoCoordinates[0];
                    var longitude = Post.PostGeo.FriendicaGeoCoordinates[1];
                    await Map.OpenAsync(latitude, longitude);
                }
                catch { await Application.Current.MainPage.DisplayAlert("Friendica Mobile - Map", AppResources.MessageDialogPostsErrorOnLoadingMap, AppResources.buttonOK); }
            }
            else if (PostHasLocation)
                await Map.OpenAsync(new Placemark { Locality = Post.PostLocation });
        }


        private ICommand _showProfileCommand;
        public ICommand ShowProfileCommand => _showProfileCommand ?? (_showProfileCommand = new Command(ShowProfile));
        private void ShowProfile()
        {
            // open link to user profile incl. the users ZRL for displaying the connect button correctly
            Launchers.OpenUrlWithZrl(Post.PostUser.UserStatusnetProfileUrl, true);
        }


        private ICommand _addCommentCommand;
        public ICommand AddCommentCommand => _addCommentCommand ?? (_addCommentCommand = new Command(AddComment));
        private async void AddComment()
        {
            // invoke in FriendicaThread to add the thread to the NewPost.xaml navigation
            ButtonAddCommentClicked?.Invoke(this, EventArgs.Empty);
        }


        private ICommand _retweetCommand;
        public ICommand RetweetCommand => _retweetCommand ?? (_retweetCommand = new Command(Retweet));
        private async void Retweet()
        {
            // invoke in FriendicaThread to add the thread to the NewPost.xaml navigation
            ButtonRetweetClicked?.Invoke(this, EventArgs.Empty);
        }


        private ICommand _showLikesCommand;
        public ICommand ShowLikesCommand => _showLikesCommand ?? (_showLikesCommand = new Command(ShowLikes));
        private void ShowLikes()
        {
            if (IsDislikesVisible)
                IsDislikesVisible = false;
            IsLikesVisible = !IsLikesVisible;
        }


        private ICommand _showDislikesCommand;
        public ICommand ShowDislikesCommand => _showDislikesCommand ?? (_showDislikesCommand = new Command(ShowDislikes));
        private void ShowDislikes()
        {
            if (IsLikesVisible)
                IsLikesVisible = false;
            IsDislikesVisible = !IsDislikesVisible;
        }


        private ICommand _likeCommand;
        public ICommand LikeCommand => _likeCommand ?? (_likeCommand = new Command(Like));
        private async void Like()
        {
            // as the setting of the like can take some seconds we show a progress ring to the user
            IsUpdatingLikes = true;

            // if we are in sample mode, we cannot send data to a server
            var isTestMode = await CheckTestModeAsync(FriendicaActivity.like);
            if (isTestMode)
            {
                ChangeListAfterSuccessfulUpdate(FriendicaActivity.like);
            }
            else
            {
                // now we can set/unset the like, except for Newsfeed items where we need to retweet them before set/unset
                if (PostType == PostTypes.Newsfeed)
                    await RetweetItemAsync(FriendicaActivity.like);
                else
                    await UpdateActivityOnServerAsync(FriendicaActivity.like);
            }
            IsUpdatingLikes = false;
        }


        private ICommand _dislikeCommand;
        public ICommand DislikeCommand => _dislikeCommand ?? (_dislikeCommand = new Command(Dislike));
        private async void Dislike()
        {
            // as the setting of the like can take some seconds we show a progress ring to the user
            IsUpdatingDislikes = true;

            // if we are in sample mode, we cannot send data to a server
            var isTestMode = await CheckTestModeAsync(FriendicaActivity.dislike);
            if (isTestMode)
            {
                ChangeListAfterSuccessfulUpdate(FriendicaActivity.dislike);
            }
            else
            {
                // now we can set/unset the like, except for Newsfeed items where we need to retweet them before set/unset
                if (PostType == PostTypes.Newsfeed)
                    await RetweetItemAsync(FriendicaActivity.dislike);
                else
                    await UpdateActivityOnServerAsync(FriendicaActivity.dislike);
            }
            IsUpdatingDislikes = false;
        }
#endregion


        // constructor transferring the post returned from server into this extended class
        public FriendicaPost(JsonFriendicaPost post)
        {
            if (post != null)
            {
                Post = post;
                if (post.PostRetweetedStatus != null)
                    RetweetedStatus = new FriendicaPost(post.PostRetweetedStatus);
                IsVisible = true;
            }
        }


        public string CreateRetweetContent()
        {
            // prepare needed data 
            var username = Post.PostUser.UserName;
            var profile = Post.PostUser.UserUrl;
            var avatar = Post.PostUser.UserProfileImageUrl;
            var guid = "";
            var date = CreatedAtDateTime.ToString("yyyy-MM-dd hh:mm:ss");

            // create content - this part is in BBcode style, Friendica API will convert HTMl into BBcode, so this should be kept
            string content = "";
            content += "[share";
            if (!string.IsNullOrEmpty(username))
                content += " author='" + username + "'";
            if (!string.IsNullOrEmpty(profile))
                content += " profile='" + profile + "'";
            if (!string.IsNullOrEmpty(avatar))
                content += " avatar='" + avatar + "'";
            if (!string.IsNullOrEmpty(guid))
                content += " guid='" + guid + "'";
            if (!string.IsNullOrEmpty(date))
                content += " posted='" + date + "'";
            content += "]";

            // now add the html content of the post
            if (!string.IsNullOrEmpty(PostHtml))
                content += PostHtml;

            content += "[/share]";
            return content;
        }


        private void SetSenderNameConcat()
        {
            // combine usernames if we have a retweeted status
            if (RetweetedStatus != null && RetweetedStatus.Post.PostUser != null && RetweetedStatus.Post.PostUser.UserName != Post.PostUser.UserName)
                SenderNameConcat = RetweetedStatus.Post.PostUser.UserName + "  " + WebUtility.HtmlDecode("&#x27AF;") + "  " + Post.PostUser.UserName;
            else if (Post.PostUser != null)
                SenderNameConcat = Post.PostUser.UserName;
            else
                SenderNameConcat = "";
        }


        // get thread id of the post
        public int GetThreadId()
        {
            // retrieve PostInReplyToStatusIdStr as the PostInReplyToStatusId might link to different id's which are not in database
            // change 2017-MAR-16: PostInReplyToStatusIdStr is now always the same as ...id since Friendica 3.5.1
            // use statusnet_conversation_id instead
            int id = 0;
            if (Post.PostInReplyToStatusId == 0 || Post.PostInReplyToStatusId == null)
                id = Convert.ToInt32(Post.PostId);
            else if (Post.PostStatusnetConversationId != "0")
                id = Convert.ToInt32(Post.PostStatusnetConversationId);
            return id;
        }


        private async Task<bool> CheckTestModeAsync(FriendicaActivity friendicaActivity)
        {
            if (!Settings.IsFriendicaLoginDefined())
            {
                // show a hint to the user once per session if we are in demo mode
                if (!App.LikeDislikeInTestModeInfoAlreadyShown)
                {
                    var title = (friendicaActivity == FriendicaActivity.like) ? AppResources.ButtonLikeText : AppResources.ButtonDislikeText;
                    await Application.Current.MainPage.DisplayAlert(title,
                                                            AppResources.messageDialogLikesNotSentToServer,
                                                            AppResources.buttonOK);
                    App.LikeDislikeInTestModeInfoAlreadyShown = true;
                }
                return true;
            }
            return false;
        }


        private async Task UpdateActivityOnServerAsync(FriendicaActivity activity)
        {
            var title = (activity == FriendicaActivity.like) ? AppResources.ButtonLikeText : AppResources.ButtonDislikeText;
            var postActivity = new HttpFriendicaPosts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);
            PostFriendicaActivityResults result = PostFriendicaActivityResults.UnexpectedError;

            if (activity == FriendicaActivity.like)
            {
                if (IsLikedByMe)
                    result = await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.unlike);
                else
                    result = await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.like);
            }
            else if (activity == FriendicaActivity.dislike)
            {
                if (IsDislikedByMe)
                    result = await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.undislike);
                else
                    result = await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.dislike);
            }

            // react on server results
            switch (result)
            {
                case PostFriendicaActivityResults.SampleMode:
                case PostFriendicaActivityResults.NotAuthenticated:
                case PostFriendicaActivityResults.UnexpectedError:
                    // info to user with the error message
                    var errorMsg = string.Format(AppResources.messageDialogErrorOnActivitySettingBadRequest, postActivity.ReturnString);
                    await Application.Current.MainPage.DisplayAlert(title, errorMsg, AppResources.buttonOK);
                    break;
                case PostFriendicaActivityResults.NotAnswered:
                    // server has not answered, ask user to retry
                    var answer = await Application.Current.MainPage.DisplayAlert(title,
                                            AppResources.messageDialogErrorOnActivitySetting,
                                            AppResources.buttonYes, AppResources.buttonNo);
                    if (answer)
                        await UpdateActivityOnServerAsync(activity);
                    break;
                case PostFriendicaActivityResults.OK:
                    // let's update the lists in the app
                    var success = ChangeListAfterSuccessfulUpdate(activity);
                    if (!success)
                    {
                        // an error has occurred, info to user with error message
                        errorMsg = string.Format(AppResources.messageDialogErrorOnActivitySettingBadRequest, "error on updating lists in app");
                        await Application.Current.MainPage.DisplayAlert(title, errorMsg, AppResources.buttonOK);
                    }
                    break;
            }
        }


        private async Task RetweetItemAsync(FriendicaActivity activity)
        {
            // like/dislike on RSS feed item is not useful as other user cannot see the activity, old Windows version retweeted the post
            // with standard ACL setting (if available) and then set the like/dislike, however this is not useful, better to retweet by
            // opening NewPost.xaml for user to add a comment showing his/her mood on this post before sending the retweet to server
            var title = (activity == FriendicaActivity.like) ? AppResources.ButtonLikeText : AppResources.ButtonDislikeText;
            var answer = await Application.Current.MainPage.DisplayAlert(title,
                            AppResources.MessageDialogNewsfeedNotLikableRetweet,
                            AppResources.buttonYes, AppResources.buttonNo);
            if (answer)
            {
                // invoke in FriendicaThread to add the thread to the NewPost.xaml navigation
                ButtonRetweetClicked?.Invoke(this, EventArgs.Empty);
            }
        }


        private bool ChangeListAfterSuccessfulUpdate(FriendicaActivity friendicaActivity)
        {
            if (friendicaActivity == FriendicaActivity.like && IsLikedByMe)
                friendicaActivity = FriendicaActivity.unlike;
            else if (friendicaActivity == FriendicaActivity.dislike && IsDislikedByMe)
                friendicaActivity = FriendicaActivity.undislike;

            switch (friendicaActivity)
            {
                case FriendicaActivity.like:
                    // add authenticated user
                    var user = new FriendicaUser()
                    {
                        IsAuthenticatedUser = true
                    };
                    Post.PostFriendicaActivities.ActivitiesLike.Add(user);
                    break;
                case FriendicaActivity.dislike:
                    // add authenticated user
                    user = new FriendicaUser()
                    {
                        IsAuthenticatedUser = true
                    };
                    Post.PostFriendicaActivities.ActivitiesDislike.Add(user);
                    break;
                case FriendicaActivity.unlike:
                    // remove authenticated user
                    var post = Post.PostFriendicaActivities.ActivitiesLike.SingleOrDefault(u => u.IsAuthenticatedUser == true);
                    Post.PostFriendicaActivities.ActivitiesLike.Remove(post);
                    break;
                case FriendicaActivity.undislike:
                    // remove authenticated user
                    post = Post.PostFriendicaActivities.ActivitiesDislike.SingleOrDefault(u => u.IsAuthenticatedUser == true);
                    Post.PostFriendicaActivities.ActivitiesDislike.Remove(post);
                    break;
                default:
                    return false;
            }
            SetActivitiesParameters();
            return true;
        }


        public void SetActivitiesParameters()
        {
            ActivitiesNotSupported = SetActivitiesNotSupported();

            if (!ActivitiesNotSupported)
            {
                IsLikedByMe = ActivityContainsAuthenticatedUser(Post.PostFriendicaActivities.ActivitiesLike);
                IsDislikedByMe = ActivityContainsAuthenticatedUser(Post.PostFriendicaActivities.ActivitiesDislike);
                CountLikes = SetCountText(Post.PostFriendicaActivities.ActivitiesLike);
                CountDislikes = SetCountText(Post.PostFriendicaActivities.ActivitiesDislike);

                // list of all likes except the autheticated user for displaying in flyout
                LikesForDisplay = new List<FriendicaUser>();
                var likes = Post.PostFriendicaActivities.ActivitiesLike.Where(u => u.IsAuthenticatedUser == false);
                foreach (var like in likes)
                    LikesForDisplay.Add(like);

                // list of all dislikes except the authenticated user for displaying in flyout
                DislikesForDisplay = new List<FriendicaUser>();
                var dislikes = Post.PostFriendicaActivities.ActivitiesDislike.Where(u => u.IsAuthenticatedUser == false);
                foreach (var dislike in dislikes)
                    DislikesForDisplay.Add(dislike);
            }
        }


        // method to return if the server doesn't support activities (in that case it returns null for activities)
        private bool SetActivitiesNotSupported()
        {
            if (Post.PostFriendicaActivities == null)
                return true;

            if (Post.PostFriendicaActivities.ActivitiesLike == null &&
                Post.PostFriendicaActivities.ActivitiesDislike == null &&
                Post.PostFriendicaActivities.ActivitiesAttendYes == null &&
                Post.PostFriendicaActivities.ActivitiesAttendNo == null &&
                Post.PostFriendicaActivities.ActivitiesAttendMaybe == null)
                return true;
            else
                return false;
        }


        private bool ActivityContainsAuthenticatedUser(List<FriendicaUser> list)
        {
            if (list == null)
                return false;

            var count = list.Count(u => u.IsAuthenticatedUser == true);
            if (count == 0)
                return false;
            else
                return true;
        }


        private string SetCountText(List<FriendicaUser> list)
        {
            if (list == null)
                return null;
            var count = list.Count(u => u.IsAuthenticatedUser == false);
            if (ActivityContainsAuthenticatedUser(list))
            {
                if (count == 0)
                    return AppResources.textblockPostsLikedByMe;
                else
                    return count + " " + AppResources.textblockPostsLikedByMeAndOthers;
            }
            else
            {
                if (count == 0)
                    return "---";
                else
                    return count.ToString();
            }
        }


        // method to detect if we have a new post here
        private void SetIsNewEntry()
        {
            if (PostType == PostTypes.UserGenerated)
            {
                IsNewEntry = (Post.PostId > Settings.LastReadNetworkPost) ? true : false;
            }
            else if (PostType == PostTypes.Newsfeed)
            {
                // TO DO: Achtung: auf Windows möchten wir hier nicht auf die LocalSettings sondern auf die RemoteSettings zurückgreifen,
                // TO DO: falls User das nicht deaktiviert hat, bei Android kann man das auch irgendwie synchronisieren (siehe Wear und Phone)
                // TO DO: Achtung: Liste der Id's alleine genügt nicht, es könnte auf einem anderen Gerät ein anderes Friendica-Konto eingerichtet sein
                IsNewEntry = (Post.PostId > Settings.LastReadNetworkPost) ? true : false;
                if (Settings.UnseenNewsfeedItems.Contains((int)Post.PostId))
                    IsNewEntry = true;
            }
        }
    }
}
