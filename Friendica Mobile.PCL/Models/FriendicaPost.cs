using Friendica_Mobile.PCL.HttpRequests;
using Friendica_Mobile.PCL.Strings;
using Friendica_Mobile.PCL.Viewmodels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace Friendica_Mobile.PCL.Models
{
    public class FriendicaPost : BindableClass
    {
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

                if (Post.PostInReplyToStatusId > 0)
                    IsComment = true;
                else
                    IsComment = false;

                CreatedAtDateTime = DateTime.ParseExact(Post.PostCreatedAt, "ddd MMM dd HH:mm:ss zzzz yyyy", System.Globalization.CultureInfo.InvariantCulture);
                CreatedAtLocalized = CreatedAtDateTime.ToString("ddd") + " " + CreatedAtDateTime.ToString("d") + " " + CreatedAtDateTime.ToString("t");

                // combine usernames if we have a retweeted status
                if (RetweetedStatus != null && RetweetedStatus.Post.PostUser != null && RetweetedStatus.Post.PostUser.UserName != Post.PostUser.UserName)
                    SenderNameConcat = RetweetedStatus.Post.PostUser.UserName + "  " + WebUtility.HtmlDecode("&#x27AF;") + "  " + Post.PostUser.UserName;
                else if (Post.PostUser != null)
                    SenderNameConcat = Post.PostUser.UserName;
                else
                    SenderNameConcat = "";
                
                // set flag for showing textblock with location and add globe icon to the location
                if (Post.PostLocation == "" || Post.PostLocation == null)
                    PostHasLocation = false;
                else
                    PostHasLocation = true;
                LocationWithIcon = Post.PostLocation + " " + WebUtility.HtmlDecode("&#x1F30D;");

                SetIsNewEntry();
            }
        }

        // property containing the converted user (extended by commands, events etc.)
        private FriendicaUser _user;
        public FriendicaUser User
        {
            get { return _user; }
            set { _user = value; }
        }

        // property containing the converted retweeted status (extended by commands, events etc.)
        private FriendicaPost _retweetedStatus;
        public FriendicaPost RetweetedStatus
        {
            get { return _retweetedStatus; }
            set { _retweetedStatus = value; }
        }

        // property containing the converted activities (extended by commands, events etc.)
        private FriendicaActivities _activities;
        public FriendicaActivities Activities
        {
            get { return _activities; }
            set { _activities = value;
                SetActivitiesParameters();
            }
        }

        // store activity on newsfeed items where user wants to like/dislike them
        private FriendicaActivity _intendedActivity;
        public FriendicaActivity IntendedActivity
        {
            get { return _intendedActivity; }
            set { _intendedActivity = value; }
        }


        // property containing the type of the post
        private PostTypes _postType;
        public PostTypes PostType
        {
            get { return _postType; }
            set { _postType = value;
                OnPropertyChanged("PostType"); }
        }

        // property for indicating that post is a new one
        private bool _isNewEntry;
        public bool IsNewEntry
        {
            get { return _isNewEntry; }
            set { _isNewEntry = value;
                OnPropertyChanged("IsNewEntry");
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
            set { _senderNameConcat = value; }
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
            set
            {
                _activitiesNotSupported = value;
            }
        }

        // indicator for showing like symbol in color or not
        private bool _isLikedByMe;
        public bool IsLikedByMe
        {
            get { return _isLikedByMe; }
            set
            {
                _isLikedByMe = value;
                OnPropertyChanged("IsLikedByMe");
            }
        }

        // indicator for showing dislike symbol in color or not
        private bool _isDislikedByMe;
        public bool IsDislikedByMe
        {
            get { return _isDislikedByMe; }
            set
            {
                _isDislikedByMe = value;
                OnPropertyChanged("IsDislikedByMe");
            }
        }

        // string showing the count of likes
        private string _countLikes;
        public string CountLikes
        {
            get { return _countLikes; }
            set
            {
                _countLikes = value;
                OnPropertyChanged("CountLikes");
            }
        }

        // string showing the count of dislikes
        private string _countDislikes;
        public string CountDislikes
        {
            get { return _countDislikes; }
            set
            {
                _countDislikes = value;
                OnPropertyChanged("CountDislikes");
            }
        }

        // List with Likes
        private ObservableCollection<FriendicaUser> _likesForDisplay;
        public ObservableCollection<FriendicaUser> LikesForDisplay
        {
            get { return _likesForDisplay; }
            set { _likesForDisplay = value; }
        }

        private void LikesForDisplay_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        // List with Likes
        private ObservableCollection<FriendicaUser> _dislikesForDisplay;
        public ObservableCollection<FriendicaUser> DislikesForDisplay
        {
            get { return _dislikesForDisplay; }
            set { _dislikesForDisplay = value; }
        }

        // indicator for showing progress ring for like button
        private bool _isUpdatingLikes;
        public bool IsUpdatingLikes
        {
            get { return _isUpdatingLikes; }
            set
            {
                _isUpdatingLikes = value;
                OnPropertyChanged("IsUpdatingLikes");
            }
        }

        // indicator for showing progress ring for dislike button
        private bool _isUpdatingDislikes;
        public bool IsUpdatingDislikes
        {
            get { return _isUpdatingDislikes; }
            set
            {
                _isUpdatingDislikes = value;
                OnPropertyChanged("IsUpdatingDislikes");
            }
        }


        // eventhandlers
        public event EventHandler ButtonAddCommentClicked;
        public event EventHandler ButtonShowProfileClicked;
        public event EventHandler ButtonRetweetClicked;
        public event EventHandler LikeNewsfeedClicked;
        public event EventHandler ButtonShowMapClicked;


        // constructor transferring the post returned from server into this extended class
        public FriendicaPost(JsonFriendicaPost post)
        {
            if (post != null)
            {
                Post = post;
                if (post.PostUser != null)
                    User = new FriendicaUser(post.PostUser);
                if (post.PostRetweetedStatus != null)
                    RetweetedStatus = new FriendicaPost(post.PostRetweetedStatus);
                if (post.PostFriendicaActivities != null)
                    Activities = new FriendicaActivities(post.PostFriendicaActivities);
                IsVisible = true;
            }
        }

        public FriendicaPost()
        { }

        // button clicked to like/unlike the post
        Command _likeCommand;
        public Command LikeCommand { get { return _likeCommand ?? (_likeCommand = new Command(ExecuteLike)); } }
        private async void ExecuteLike()
        {
            IsUpdatingLikes = true;

            if (CheckSettings.ServerSettingsAvailable())
            {
                // check if we are on a newsfeed-item; we cannot like this item so that contacts will see the like
                if (PostType == PostTypes.Newsfeed)
                {
                    if (ActivitiesNotSupported)
                        return;
                    RetweetandSetActivity(FriendicaActivity.like);
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
            else
            {
                // we are in sample mode, don't send commands to a server just inform
                if (!StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown)
                {
                    // when no settings we cannot send likes/dislikes to a server - inform user once about this
                    string errorMsg;
                    errorMsg = AppResources.messageDialogLikesNotSentToServer;
                    await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK);

                    StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown = true;
                }

                var postActivity = new PostFriendicaActivities();
                if (IsLikedByMe)
                    postActivity.Activity = FriendicaActivity.unlike;
                else
                    postActivity.Activity = FriendicaActivity.like;
                ChangeListAfterSuccessfulUpdate(postActivity);
                IsUpdatingLikes = false;
            }
        }

        // button clicked to like/unlike the post
        Command _dislikeCommand;
        public Command DislikeCommand { get { return _dislikeCommand ?? (_dislikeCommand = new Command(ExecuteDislike)); } }
        private async void ExecuteDislike()
        {
            IsUpdatingDislikes = true;

            if (CheckSettings.ServerSettingsAvailable())
            {
                // check if we are on a newsfeed-item; we cannot like this item so that contacts will see the like
                if (PostType == PostTypes.Newsfeed)
                {
                    if (ActivitiesNotSupported)
                        return;
                    RetweetandSetActivity(FriendicaActivity.dislike);
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
            else
            {
                // we are in sample mode, don't send commands to a server, just inform
                if (!StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown)
                {
                    // when no settings we cannot send likes/dislikes to a server - inform user once about this
                    string errorMsg;
                    errorMsg = AppResources.messageDialogLikesNotSentToServer;
                    await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK);

                    StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown = true;
                }

                var postActivity = new PostFriendicaActivities();
                if (IsDislikedByMe)
                    postActivity.Activity = FriendicaActivity.undislike;
                else
                    postActivity.Activity = FriendicaActivity.dislike;
                ChangeListAfterSuccessfulUpdate(postActivity);
                IsUpdatingDislikes = false;

            }
        }


        // start Bing Maps application and show the location on the map
        Command<FriendicaPost> _loadMapCommand;
        public Command<FriendicaPost> LoadMapCommand { get { return _loadMapCommand ?? (_loadMapCommand = new Command<FriendicaPost>(ExecuteLoadMap)); } }

        private void ExecuteLoadMap(FriendicaPost post)
        {
            ButtonShowMapClicked?.Invoke(this, EventArgs.Empty);
        }


        // fire event for button clicked to add a comment to the thread
        Command<FriendicaPost> _addCommentCommand;
        public Command<FriendicaPost> AddCommentCommand { get { return _addCommentCommand ?? (_addCommentCommand = new Command<FriendicaPost>(ExecuteAddComment)); } }
        private void ExecuteAddComment(FriendicaPost post)
        {
            ButtonAddCommentClicked?.Invoke(this, EventArgs.Empty);
        }


        // fire event for button clicked to show profile of the author
        Command<FriendicaPost> _showProfileCommand;
        public Command<FriendicaPost> ShowProfileCommand { get { return _showProfileCommand ?? (_showProfileCommand = new Command<FriendicaPost>(ExecuteShowProfile)); } }
        private void ExecuteShowProfile(FriendicaPost post)
        {
            ButtonShowProfileClicked?.Invoke(this, EventArgs.Empty);
        }


        // fire event for button clicked to retweet the post
        Command<FriendicaPost> _retweetCommand;
        public Command<FriendicaPost> RetweetCommand { get { return _retweetCommand ?? (_retweetCommand = new Command<FriendicaPost>(ExecuteRetweet)); } }
        private void ExecuteRetweet(FriendicaPost post)
        {
            ButtonRetweetClicked?.Invoke(this, EventArgs.Empty);
        }


        // get thread id of the post
        public int GetThreadId()
        {
            // retrieve PostInReplyToStatusIdStr as the PostInReplyToStatusId might link to different id's which are not in database
            // change 2017-MAR-16: PostInReplyToStatusIdStr is now always the same as ...id since Friendica 3.5.1
            // use statusnet_conversation_id instead
            int id = 0;
            if (Post.PostInReplyToStatusId == 0)
                id = Convert.ToInt32(Post.PostId);
            else if (Post.PostStatusnetConversationId != "0")
                id = Convert.ToInt32(Post.PostStatusnetConversationId);
            return id;
        }


        private async void UpdateActivityOnServer(FriendicaActivity activity)
        {
            var postActivity = new PostFriendicaActivities();
            if (activity == FriendicaActivity.like)
            {
                if  (IsLikedByMe)
                    await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.unlike);
                else
                    await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.like);
            }
            else if (activity == FriendicaActivity.dislike)
            {
                if (IsDislikedByMe)
                    await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.undislike);
                else
                    await postActivity.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.dislike);
            }

            string errorMsg = "";
            switch (postActivity.StatusCode)
            {
                case HttpStatusCode.OK:
                    var success = ChangeListAfterSuccessfulUpdate(postActivity);
                    if (!success)
                    {
                        // an error has occurred, info to user with error message
                        errorMsg = AppResources.messageDialogErrorOnActivitySettingBadRequest;
                        errorMsg = String.Format(errorMsg, "error on updating lists in app");
                        await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK, null, null, 0, 0);
                    }
                    break;
                case HttpStatusCode.BadGateway:
                    // server is not available, ask user if he wants to retry
                    errorMsg = AppResources.messageDialogErrorOnActivitySetting;
                    var result = await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonYes, AppResources.buttonNo, null, 0, 1);

                    // retry
                    if (result == 0)
                    {
                        if (postActivity.Activity == FriendicaActivity.unlike)
                            UpdateActivityOnServer(FriendicaActivity.like);
                        else if (postActivity.Activity == FriendicaActivity.undislike)
                            UpdateActivityOnServer(FriendicaActivity.dislike);
                        else
                            UpdateActivityOnServer(postActivity.Activity);
                    }
                    break;
                case HttpStatusCode.BadRequest:
                default:
                    // an error has occurred, info to user with error message
                    errorMsg = AppResources.messageDialogErrorOnActivitySettingBadRequest;
                    errorMsg = String.Format(errorMsg, postActivity.ReturnString);
                    await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK);
                    break;
            }

            if (postActivity.Activity == FriendicaActivity.like || postActivity.Activity == FriendicaActivity.unlike)
                IsUpdatingLikes = false;
            else if (postActivity.Activity == FriendicaActivity.dislike || postActivity.Activity == FriendicaActivity.undislike)
                IsUpdatingDislikes = false;

        }

        private async void RetweetandSetActivity(FriendicaActivity activity)
        {
            // inform user that we cannot like the item directly
            string errorMsg;
            string defaultSettings = "";

            // check if we have a default ACL setting available
            if (Settings.ACLPublicPost || Settings.ACLPrivatePost)
            {
                if (Settings.ACLPublicPost)
                    defaultSettings = AppResources.radiobuttonPublicPost_Content;
                else if (Settings.ACLPrivatePost)
                {
                    // show numbers of contacts/groups if available, currently not possible to post names instead of the numbers as the contacts are not yet in the PCL
                    var contacts = (Settings.ACLPrivateSelectedContacts == "") ? "---" : Settings.ACLPrivateSelectedContacts;
                    var groups = (Settings.ACLPrivateSelectedGroups == "") ? "---" : Settings.ACLPrivateSelectedGroups;
                    defaultSettings = AppResources.radiobuttonPrivatePost_Content + ": " + contacts + "/" + groups;
                }
                errorMsg = String.Format(AppResources.messageDialogNewsfeedNotLikable, defaultSettings);
                var result = await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonYes, AppResources.buttonNo, null, 0, 1);

                // user wants to continue with the retweet
                if (result == 0)
                {
                    // save the activity in this class and move on to Newsfeed.xaml.cs as the PCL currently perform the retweet in background
                    IntendedActivity = activity;
                    LikeNewsfeedClicked?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                // like/dislike not possible because there is no standard acl
                errorMsg = AppResources.messageDialogNewsfeedLikesNoDefault;
                await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK);
            }
        }

        private bool ChangeListAfterSuccessfulUpdate(PostFriendicaActivities postActivity)
        {
            switch (postActivity.Activity)
            {
                case FriendicaActivity.like:
                    // add authenticated user
                    var user = new FriendicaUser()
                    {
                        IsAuthenticatedUser = true
                    };
                    Activities.ActivitiesLike.Add(user);
                    break;
                case FriendicaActivity.dislike:
                    // add authenticated user
                    user = new FriendicaUser()
                    {
                        IsAuthenticatedUser = true
                    };
                    Activities.ActivitiesDislike.Add(user);
                    break;
                case FriendicaActivity.unlike:
                    // remove authenticated user
                    var post = Activities.ActivitiesLike.SingleOrDefault(u => u.IsAuthenticatedUser == true);
                    Activities.ActivitiesLike.Remove(post);
                    break;
                case FriendicaActivity.undislike:
                    // remove authenticated user
                    post = Activities.ActivitiesDislike.SingleOrDefault(u => u.IsAuthenticatedUser == true);
                    Activities.ActivitiesDislike.Remove(post);
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
                IsLikedByMe = ActivityContainsAuthenticatedUser(Activities.ActivitiesLike);
                IsDislikedByMe = ActivityContainsAuthenticatedUser(Activities.ActivitiesDislike);
                CountLikes = SetCountText(Activities.ActivitiesLike);
                CountDislikes = SetCountText(Activities.ActivitiesDislike);

                // list of all likes except the autheticated user for displaying in flyout
                LikesForDisplay = new ObservableCollection<FriendicaUser>();
                var likes = Activities.ActivitiesLike.Where(u => u.IsAuthenticatedUser == false);
                foreach (var like in likes)
                    LikesForDisplay.Add(like);

                // list of all dislikes except the authenticated user for displaying in flyout
                DislikesForDisplay = new ObservableCollection<FriendicaUser>();
                var dislikes = Activities.ActivitiesDislike.Where(u => u.IsAuthenticatedUser == false);
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
                // TODO: wenn id > LastReadNetworkPost dann jedenfalls IsNewEntry = true;
                // TODO: anderenfalls: ist der post in der Liste der nicht gelesenen Posts aufgeführt dann true, sonst false
                // TODO: Achtung: auf Windows möchten wir hier nicht auf die LocalSettings sondern auf die RemoteSettings zurückgreifen,
                // TODO: falls User das nicht deaktiviert hat, bei Android kann man das auch irgendwie synchronisieren (siehe Wear und Phone)
                // TODO: Achtung: Liste der Id's alleine genügt nicht, es könnte auf einem anderen Gerät ein anderes Friendica-Konto eingerichtet sein
                IsNewEntry = (Post.PostId > Settings.LastReadNetworkPost) ? true : false;
            }
        }
    }
}
