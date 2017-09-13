using Friendica_Mobile.HttpRequests;
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
using static Friendica_Mobile.HttpRequests.PostFriendicaActivities;

namespace Friendica_Mobile.Models
{
    // Ausgabe von server/api/statusnet/config (ohne credentials) ergibt ein FriendicaConfig-Object, welches seinerseits
    // aus FriendicaSite und FriendicaServer besteht. 
    public class FriendicaPostExtended : BindableClass
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        private FriendicaPost _post;
        public FriendicaPost Post
        {
            get { return _post; }
            set { _post = value;
                SetActivitiesParameters();
            }
        }

        private Paragraph _contentTransformed;
        public Paragraph ContentTransformed
        {
            get { return _contentTransformed; }
            set { _contentTransformed = value; }
        }

        private Thickness _newEntryIndicatorBorder;
        public Thickness NewEntryIndicatorBorder
        {
            get { return _newEntryIndicatorBorder; }
            set
            {
                _newEntryIndicatorBorder = value;
                OnPropertyChanged("NewEntryIndicatorBorder");
            }
        }

        private bool _isComment;
        public bool IsComment
        {
            get { return _isComment; }
            set { _isComment = value; }
        }

        // we need to implement this bool in post for showing the button
        private bool _isPostWithComments = false;
        public bool IsPostWithComments
        {
            get { return _isPostWithComments; }
            set { _isPostWithComments = value; }
        }
        public event EventHandler ButtonShowMorePostsClicked;

        private bool _toggleShowCommentsState;
        public bool ToggleShowCommentsState
        {
            get { return _toggleShowCommentsState; }
            set
            {
                _toggleShowCommentsState = value;
                if (ButtonShowMorePostsClicked != null)
                    ButtonShowMorePostsClicked(this, EventArgs.Empty);
                OnPropertyChanged("ToggleShowCommentsState");
            }
        }

        public event EventHandler ButtonShowThreadClicked;
        public event EventHandler ButtonAddCommentClicked;
        public event EventHandler ButtonShowProfileClicked;

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        private string _createdAtLocalized;
        public string CreatedAtLocalized
        {
            get { return _createdAtLocalized; }
            set { _createdAtLocalized = value; }
        }

        private string _senderNameConcat;
        public string SenderNameConcat
        {
            get { return _senderNameConcat; }
            set { _senderNameConcat = value; }
        }

        private bool _entryHasLocation;
        public bool EntryHasLocation
        {
            get { return _entryHasLocation; }
            set { _entryHasLocation = value; }
        }

        private string _locationWithIcon;
        public string LocationWithIcon
        {
            get { return _locationWithIcon; }
            set { _locationWithIcon = value; }
        }

        public FlowDirection FlowButton
        {
            get
            {
                if (App.Settings.OrientationDevice == OrientationDeviceFamily.MobilePortrait)
                {
                    if (App.Settings.NavigationSide == "RightToLeft")
                        return FlowDirection.RightToLeft;
                    else
                        return FlowDirection.LeftToRight;
                }
                else
                    return FlowDirection.LeftToRight;
            }
        }

        // indicator that server is not supporting likes at the moment (lists of users are null)
        private bool _activitiesNotSupported;
        public bool ActivitiesNotSupported
        {
            get { return _activitiesNotSupported; }
            set { _activitiesNotSupported = value;
                OnPropertyChanged("ActivitiesNotSupported"); }
        }

        // indicator for showing like symbol in color or not
        private bool _isLikedByMe;
        public bool IsLikedByMe
        {
            get { return _isLikedByMe; }
            set { _isLikedByMe = value;
                OnPropertyChanged("IsLikedByMe");}
        }

        // indicator for showing dislike symbol in color or not
        private bool _isDislikedByMe;
        public bool IsDislikedByMe
        {
            get { return _isDislikedByMe; }
            set { _isDislikedByMe = value;
                OnPropertyChanged("IsDislikedByMe"); }
        }

        // string showing the count of likes
        private string _countLikes;
        public string CountLikes
        {
            get { return _countLikes; }
            set { _countLikes = value;
                OnPropertyChanged("CountLikes"); }
        }

        // string showing the count of dislikes
        private string _countDislikes;
        public string CountDislikes
        {
            get { return _countDislikes; }
            set { _countDislikes = value;
                OnPropertyChanged("CountDislikes"); }
        }

        // List with Likes
        private List<FriendicaUserExtended> _likesForDisplay;
        public List<FriendicaUserExtended> LikesForDisplay
        {
            get { return _likesForDisplay; }
            set { _likesForDisplay = value; }
        }

        // List with Likes
        private List<FriendicaUserExtended> _dislikesForDisplay;
        public List<FriendicaUserExtended> DislikesForDisplay
        {
            get { return _dislikesForDisplay; }
            set { _dislikesForDisplay = value; }
        }

        // indicator for showing progress ring for like button
        private bool _isUpdatingLikes;
        public bool IsUpdatingLikes
        {
            get { return _isUpdatingLikes; }
            set { _isUpdatingLikes = value;
                OnPropertyChanged("IsUpdatingLikes"); }
        }

        // indicator for showing progress ring for dislike button
        private bool _isUpdatingDislikes;
        public bool IsUpdatingDislikes
        {
            get { return _isUpdatingDislikes; }
            set { _isUpdatingDislikes = value;
                OnPropertyChanged("IsUpdatingDislikes"); }
        }

        // button clicked to like/unlike the post
        Mvvm.Command _likeCommand;
        public Mvvm.Command LikeCommand { get { return _likeCommand ?? (_likeCommand = new Mvvm.Command(ExecuteLike)); } }
        private async void ExecuteLike()
        {
            IsUpdatingLikes = true;

            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://" ||
    App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                // we are in sample mode, don't send commands to a server just inform
                if (!App.TestModeInfoAlreadyShown)
                {
                    // when no settings we cannot send likes/dislikes to a server - inform user once about this
                    string errorMsg;
                    errorMsg = loader.GetString("messageDialogLikesNotSentToServer");
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);

                    App.TestModeInfoAlreadyShown = true;
                }

                var postActivity = new PostFriendicaActivities();
                if (IsLikedByMe)
                    postActivity.Activity = FriendicaActivity.unlike;
                else
                    postActivity.Activity = FriendicaActivity.like;
                ChangeListAfterSuccessfulUpdate(postActivity);
                IsUpdatingLikes = false;
            }
            else
            {
                // update on server
                if (ActivitiesNotSupported)
                    return;
                UpdateActivityOnServer(FriendicaActivity.like);
            }
        }

        // button clicked to like/unlike the post
        Mvvm.Command _dislikeCommand;
        public Mvvm.Command DislikeCommand { get { return _dislikeCommand ?? (_dislikeCommand = new Mvvm.Command(ExecuteDislike)); } }
        private async void ExecuteDislike()
        {
            IsUpdatingDislikes = true;

            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://" ||
    App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null)
            {
                // we are in sample mode, don't send commands to a server just inform
                if (!App.TestModeInfoAlreadyShown)
                {
                    // when no settings we cannot send likes/dislikes to a server - inform user once about this
                    string errorMsg;
                    errorMsg = loader.GetString("messageDialogLikesNotSentToServer");
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);

                    App.TestModeInfoAlreadyShown = true;
                }

                var postActivity = new PostFriendicaActivities();
                if (IsDislikedByMe)
                    postActivity.Activity = FriendicaActivity.undislike;
                else
                    postActivity.Activity = FriendicaActivity.dislike;
                ChangeListAfterSuccessfulUpdate(postActivity);
                IsUpdatingDislikes = false;
            }
            else
            {
                // update on server
                if (ActivitiesNotSupported)
                    return;
                UpdateActivityOnServer(FriendicaActivity.dislike);
            }
        }

        private void UpdateActivityOnServer(FriendicaActivity activity)
        {
            var postActivity = new PostFriendicaActivities();
            postActivity.FriendicaActivitySent += PostActivity_FriendicaActivitySent;
            if (activity == FriendicaActivity.like)
            {
                if (IsLikedByMe)
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.unlike);
                else
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.like);
            }
            else if (activity == FriendicaActivity.dislike)
            {
                if (IsDislikedByMe)
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.undislike);
                else
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.dislike);
            }

        }

        public void UpdateActivityOnServerRetweet(FriendicaActivity activity)
        {
            var postActivity = new PostFriendicaActivities();
            if (activity == FriendicaActivity.like)
            {
                if (IsLikedByMe)
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.unlike);
                else
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.like);
            }
            else if (activity == FriendicaActivity.dislike)
            {
                if (IsDislikedByMe)
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.undislike);
                else
                    postActivity.PostFriendicaActivity(Post.PostId, FriendicaActivity.dislike);
            }
        }


        private async void PostActivity_FriendicaActivitySent(object sender, EventArgs e)
        {
            var postActivity = sender as PostFriendicaActivities;
            string errorMsg;
            MessageDialogMessage dialog;

            switch (postActivity.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.Ok:
                    var success = ChangeListAfterSuccessfulUpdate(postActivity);
                    if (!success)
                    {
                        // an error has occurred, info to user with error message
                        errorMsg = loader.GetString("messageDialogErrorOnActivitySettingBadRequest");
                        errorMsg = String.Format(errorMsg, "error on updating lists in app");
                        dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                        await dialog.ShowDialog(0, 0);
                    }
                    break;
                case Windows.Web.Http.HttpStatusCode.BadGateway:
                    // server is not available, ask user if he wants to retry
                    errorMsg = loader.GetString("messageDialogErrorOnActivitySetting");
                    dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);
                    // retry
                    if (dialog.Result == 0)
                    {
                        if (postActivity.Activity == FriendicaActivity.unlike)
                            UpdateActivityOnServer(FriendicaActivity.like);
                        else if (postActivity.Activity == FriendicaActivity.undislike)
                            UpdateActivityOnServer(FriendicaActivity.dislike);
                        else
                            UpdateActivityOnServer(postActivity.Activity);
                    }
                    break;
                case Windows.Web.Http.HttpStatusCode.BadRequest:
                default:
                    // an error has occurred, info to user with error message
                    errorMsg = loader.GetString("messageDialogErrorOnActivitySettingBadRequest");
                    errorMsg = String.Format(errorMsg, postActivity.ReturnString);
                    dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                    break;
            }

            if (postActivity.Activity == FriendicaActivity.like || postActivity.Activity == FriendicaActivity.unlike)
                IsUpdatingLikes = false;
            else if (postActivity.Activity == FriendicaActivity.dislike || postActivity.Activity == FriendicaActivity.undislike)
                IsUpdatingDislikes = false;
        }

        private bool ChangeListAfterSuccessfulUpdate(PostFriendicaActivities postActivity)
        {
            switch (postActivity.Activity)
            {
                case FriendicaActivity.like:
                    // add authenticated user
                    var user = new FriendicaUserExtended();
                    user.IsAuthenticatedUser = true;
                    Post.PostFriendicaActivities.ActivitiesLike.Add(user);
                    break;
                case FriendicaActivity.dislike:
                    // add authenticated user
                    user = new FriendicaUserExtended();
                    user.IsAuthenticatedUser = true;
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

        public FriendicaPostExtended()
        {
            IsVisible = true;
            App.Settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "OrientationDevice")
                OnPropertyChanged("FlowButton");
        }

        public void ConvertHtmlToParagraph(string html)
        {
            var htmlToRichTextBlock = new clsHtmlToRichTextBlock(html);
            ContentTransformed = htmlToRichTextBlock.ApplyHtmlToParagraph();
        }

        public FriendicaPostExtended(string jsonString) : this()
        {
            Post = new FriendicaPost(jsonString);

            ConvertHtmlToParagraph(Post.PostStatusnetHtml);

            if (Post.PostInReplyToStatusId > 0)
                IsComment = true;
            else
                IsComment = false;

            DateTime createdAtDate = DateTime.ParseExact(Post.PostCreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
            CreatedAtLocalized = createdAtDate.ToString("ddd") + " " + createdAtDate.ToString("d") + " " + createdAtDate.ToString("t");

            if (Post.PostRetweetedStatus != null && Post.PostRetweetedStatus.PostUser.User.UserName != Post.PostUser.User.UserName)
                //SenderNameConcat = Post.PostRetweetedStatus.PostUser.UserName + " -> " + Post.PostUser.UserName;
                SenderNameConcat = Post.PostRetweetedStatus.PostUser.User.UserName + "  " + System.Net.WebUtility.HtmlDecode("&#x27AF;") + "  " + Post.PostUser.User.UserName;
            else
                SenderNameConcat = Post.PostUser.User.UserName;

            // set flag for showing textblock with location and add globe icon to the location
            if (Post.PostLocation == "" || Post.PostLocation == null)
                EntryHasLocation = false;
            else
                EntryHasLocation = true;
            LocationWithIcon = Post.PostLocation + " " + System.Net.WebUtility.HtmlDecode("&#x1F30D;");

            // mark new entries with a border of 4px
            if (Post.PostId > App.Settings.LastReadNetworkPost)
                NewEntryIndicatorBorder = new Thickness(4, 4, 4, 4);
            else
                NewEntryIndicatorBorder = new Thickness(0, 0, 0, 0);
        }


        // start Bing Maps application and show the location on the map
        Mvvm.Command<FriendicaPostExtended> _loadMapCommand;
        public Mvvm.Command<FriendicaPostExtended> LoadMapCommand { get { return _loadMapCommand ?? (_loadMapCommand = new Mvvm.Command<FriendicaPostExtended>(ExecuteLoadMap)); } }

        private async void ExecuteLoadMap(FriendicaPostExtended post)
        {
            var geo = post.Post.PostGeo;
            // bing expects the location name with %20 for spaces and other escapes
            var location = Uri.EscapeDataString(post.Post.PostLocation);
            // bing expects coordinates with a point instead of a comma
            var latitude = geo.FriendicaGeoCoordinates[0].ToString();
            latitude = latitude.Replace(",", ".");
            var longitude = geo.FriendicaGeoCoordinates[1].ToString();
            longitude = longitude.Replace(",", ".");

            // point to exact coordinates if available, otherwise use Location query
            string url;
            if (geo.FriendicaGeoType == "Point" && geo.FriendicaGeoCoordinates.Count > 0)
                url = String.Format("bingmaps:?collection=point.{0}_{1}_{2}", latitude, longitude, location);
            //url = String.Format("bingmaps:?cp={0}~{1}", geo.FriendicaGeoCoordinates[0], geo.FriendicaGeoCoordinates[1]);
            else
                url = String.Format("bingmaps:?where={0}", location);

            // launch bing maps application
            await Launcher.LaunchUriAsync(new Uri(url));
        }

        // fire event for button clicked to show thread in chronological mode
        Mvvm.Command<FriendicaPostExtended> _showThreadCommand;
        public Mvvm.Command<FriendicaPostExtended> ShowThreadCommand { get { return _showThreadCommand ?? (_showThreadCommand = new Mvvm.Command<FriendicaPostExtended>(ExecuteShowThread)); } }

        private void ExecuteShowThread(FriendicaPostExtended post)
        {
            if (ButtonShowThreadClicked != null)
                ButtonShowThreadClicked(this, EventArgs.Empty);
        }

        // fire event for button clicked to add a comment to the thread
        Mvvm.Command<FriendicaPostExtended> _addCommentCommand;
        public Mvvm.Command<FriendicaPostExtended> AddCommentCommand { get { return _addCommentCommand ?? (_addCommentCommand = new Mvvm.Command<FriendicaPostExtended>(ExecuteAddComment)); } }

        private void ExecuteAddComment(FriendicaPostExtended post)
        {
            if (ButtonAddCommentClicked != null)
                ButtonAddCommentClicked(this, EventArgs.Empty);
        }

        // fire event for button clicked to show profile of the author
        Mvvm.Command<FriendicaPostExtended> _showProfileCommand;
        public Mvvm.Command<FriendicaPostExtended> ShowProfileCommand { get { return _showProfileCommand ?? (_showProfileCommand = new Mvvm.Command<FriendicaPostExtended>(ExecuteShowProfile)); } }

        private void ExecuteShowProfile(FriendicaPostExtended post)
        {
            if (ButtonShowProfileClicked != null)
                ButtonShowProfileClicked(this, EventArgs.Empty);
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
                LikesForDisplay = new List<FriendicaUserExtended>();
                var likes = Post.PostFriendicaActivities.ActivitiesLike.Where(u => u.IsAuthenticatedUser == false);
                foreach (var like in likes)
                    LikesForDisplay.Add(like);

                // list of all dislikes except the authenticated user for displaying in flyout
                DislikesForDisplay = new List<FriendicaUserExtended>();
                var dislikes = Post.PostFriendicaActivities.ActivitiesDislike.Where(u => u.IsAuthenticatedUser == false);
                foreach (var dislike in dislikes)
                    DislikesForDisplay.Add(dislike);
            }
        }

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

        private bool ActivityContainsAuthenticatedUser(List<FriendicaUserExtended> list)
        {
            if (list == null)
                return false;

            var count = list.Count(u => u.IsAuthenticatedUser == true);
            if (count == 0)
                return false;
            else
                return true;
        }

        private string SetCountText(List<FriendicaUserExtended> list)
        {
            if (list == null)
                return null;
            var count = list.Count(u => u.IsAuthenticatedUser == false);
            if (ActivityContainsAuthenticatedUser(list))
            {
                if (count == 0)
                    return loader.GetString("textblockPostsLikedByMe");
                else
                    return count + " " + loader.GetString("textblockPostsLikedByMeAndOthers");
            }
            else
            {
                if (count == 0)
                    return "---";
                else
                    return count.ToString();
            }
        }
    }
}