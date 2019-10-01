using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
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
                // TODO: missing location in Posts
                PostHasLocation = !(string.IsNullOrEmpty(Post.PostLocation));
                if (PostHasLocation)
                    LocationWithIcon = Post.PostLocation + " " + WebUtility.HtmlDecode("&#x1F30D;");

                SetIsNewEntry();
                SetActivitiesParameters();
            }
        }

        // TODO: delete
        //// property containing the converted user (extended by commands, events etc.)
        //private FriendicaUser _user;
        //public FriendicaUser User
        //{
        //    get { return _user; }
        //    set { _user = value; }
        //

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

        // TODO: delete
        //// property containing the converted activities (extended by commands, events etc.)
        //private FriendicaActivities _activities;
        //public FriendicaActivities Activities
        //{
        //    get { return _activities; }
        //    set { _activities = value;
        //        SetActivitiesParameters();
        //    }
        //}

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
        public bool IsDislikedByMe
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
            set { _countLikes = value; }
        }

        // string showing the count of dislikes
        private string _countDislikes;
        public string CountDislikes
        {
            get { return _countDislikes; }
            set { _countDislikes = value; }
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
            set { _isUpdatingLikes = value; }
        }

        // indicator for showing progress ring for dislike button
        private bool _isUpdatingDislikes;
        public bool IsUpdatingDislikes
        {
            get { return _isUpdatingDislikes; }
            set { _isUpdatingDislikes = value; }
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
            // TODO: implement function - Achtung: like setzen und zurücknehmen in einem Command
            await Application.Current.MainPage.DisplayAlert("### Like ###", "### Funktion ist noch nicht implementiert ###", AppResources.buttonOK);
        }

        private ICommand _dislikeCommand;
        public ICommand DislikeCommand => _dislikeCommand ?? (_dislikeCommand = new Command(Dislike));
        private async void Dislike()
        {
            // TODO: implement function - Achtung: dislike setzen und zurücknehmen in einem Command
            await Application.Current.MainPage.DisplayAlert("### Dislike ###", "### Funktion ist noch nicht implementiert ###", AppResources.buttonOK);
        }
#endregion


        // constructor transferring the post returned from server into this extended class
        public FriendicaPost(JsonFriendicaPost post)
        {
            if (post != null)
            {
                Post = post;
                //if (post.PostUser != null)
                    //User = new FriendicaUser(post.PostUser);
                    // TODO: missing PostRetweetedStatus in conversation data
                if (post.PostRetweetedStatus != null)
                    RetweetedStatus = new FriendicaPost(post.PostRetweetedStatus);
                //if (post.PostFriendicaActivities != null)
                    //Activities = new FriendicaActivities(post.PostFriendicaActivities);
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

        // TODO: delete
        //public FriendicaPost()
        //{ }
// TODO: bis hierher geprüft
        // button clicked to like/unlike the post
        //Command _likeCommand;
        //public Command LikeCommand { get { return _likeCommand ?? (_likeCommand = new Command(ExecuteLike)); } }
        //private async void ExecuteLike()
        //{
        //    IsUpdatingLikes = true;

        //    if (Settings.IsFriendicaLoginDefined())
        //    {
        //        // check if we are on a newsfeed-item; we cannot like this item so that contacts will see the like
        //        if (PostType == PostTypes.Newsfeed)
        //        {
        //            if (ActivitiesNotSupported)
        //                return;
        //            RetweetandSetActivity(FriendicaActivity.like);
        //            IsUpdatingLikes = false;
        //        }
        //        else
        //        {
        //            // update on server
        //            if (ActivitiesNotSupported)
        //                return;
        //            UpdateActivityOnServer(FriendicaActivity.like);
        //        }
        //    }
        //    else
        //    {
        //        // we are in sample mode, don't send commands to a server just inform
        //        if (!StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown)
        //        {
        //            // when no settings we cannot send likes/dislikes to a server - inform user once about this
        //            string errorMsg;
        //            errorMsg = AppResources.messageDialogLikesNotSentToServer;
        //            await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK);

        //            StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown = true;
        //        }

        //        var postActivity = new PostFriendicaActivities();
        //        if (IsLikedByMe)
        //            postActivity.Activity = FriendicaActivity.unlike;
        //        else
        //            postActivity.Activity = FriendicaActivity.like;
        //        ChangeListAfterSuccessfulUpdate(postActivity);
        //        IsUpdatingLikes = false;
        //    }
        //}

        //// button clicked to like/unlike the post
        //Command _dislikeCommand;
        //public Command DislikeCommand { get { return _dislikeCommand ?? (_dislikeCommand = new Command(ExecuteDislike)); } }
        //private async void ExecuteDislike()
        //{
        //    IsUpdatingDislikes = true;

        //    if (Settings.IsFriendicaLoginDefined())
        //    {
        //        // check if we are on a newsfeed-item; we cannot like this item so that contacts will see the like
        //        if (PostType == PostTypes.Newsfeed)
        //        {
        //            if (ActivitiesNotSupported)
        //                return;
        //            RetweetandSetActivity(FriendicaActivity.dislike);
        //            IsUpdatingDislikes = false;
        //        }
        //        else
        //        {
        //            // update on server
        //            if (ActivitiesNotSupported)
        //                return;
        //            UpdateActivityOnServer(FriendicaActivity.dislike);
        //        }
        //    }
        //    else
        //    {
        //        // we are in sample mode, don't send commands to a server, just inform
        //        if (!StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown)
        //        {
        //            // when no settings we cannot send likes/dislikes to a server - inform user once about this
        //            string errorMsg;
        //            errorMsg = AppResources.messageDialogLikesNotSentToServer;
        //            await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK);

        //            StaticGlobalParameters.FriendicaPostTestModeInfoAlreadyShown = true;
        //        }

        //        var postActivity = new PostFriendicaActivities();
        //        if (IsDislikedByMe)
        //            postActivity.Activity = FriendicaActivity.undislike;
        //        else
        //            postActivity.Activity = FriendicaActivity.dislike;
        //        ChangeListAfterSuccessfulUpdate(postActivity);
        //        IsUpdatingDislikes = false;

        //    }
        //}




        //// fire event for button clicked to show profile of the author
        //Command<FriendicaPost> _showProfileCommand;
        //public Command<FriendicaPost> ShowProfileCommand { get { return _showProfileCommand ?? (_showProfileCommand = new Command<FriendicaPost>(ExecuteShowProfile)); } }
        //private void ExecuteShowProfile(FriendicaPost post)
        //{
        //    ButtonShowProfileClicked?.Invoke(this, EventArgs.Empty);
        //}


        //// fire event for button clicked to retweet the post
        //Command<FriendicaPost> _retweetCommand;
        //public Command<FriendicaPost> RetweetCommand { get { return _retweetCommand ?? (_retweetCommand = new Command<FriendicaPost>(ExecuteRetweet)); } }
        //private void ExecuteRetweet(FriendicaPost post)
        //{
        //    ButtonRetweetClicked?.Invoke(this, EventArgs.Empty);
        //}


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


        private async void UpdateActivityOnServer(FriendicaActivity activity)
        {
            //if (App.Posts == null)
            //    App.Posts = new HttpRequests.HttpFriendicaPosts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);

            //PostFriendicaActivityResults result = PostFriendicaActivityResults.UnexpectedError;
            //if (activity == FriendicaActivity.like)
            //{
            //    if  (IsLikedByMe)
            //        result = await App.Posts.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.unlike);
            //    else
            //        result = await App.Posts.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.like);
            //}
            //else if (activity == FriendicaActivity.dislike)
            //{
            //    if (IsDislikedByMe)
            //        result = await App.Posts.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.undislike);
            //    else
            //        result = await App.Posts.PostFriendicaActivityAsync(Post.PostId, FriendicaActivity.dislike);
            //}

            //switch (result)
            //{
                //case PostFriendicaActivityResults.NotAnswered:
                //case PostFriendicaActivityResults.NotAuthenticated:
                //case PostFriendicaActivityResults.OK:   
                //case PostFriendicaActivityResults.UnexpectedError:
                //default:
                    //break;
                    
                //case HttpStatusCode.OK:
                //    var success = ChangeListAfterSuccessfulUpdate(postActivity);
                //    if (!success)
                //    {
                //        // an error has occurred, info to user with error message
                //        errorMsg = AppResources.messageDialogErrorOnActivitySettingBadRequest;
                //        errorMsg = String.Format(errorMsg, "error on updating lists in app");
                //        await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK, null, null, 0, 0);
                //    }
                //    break;
                //case HttpStatusCode.BadGateway:
                //    // server is not available, ask user if he wants to retry
                //    errorMsg = AppResources.messageDialogErrorOnActivitySetting;
                //    var result = await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonYes, AppResources.buttonNo, null, 0, 1);

                //    // retry
                //    if (result == 0)
                //    {
                //        if (postActivity.Activity == FriendicaActivity.unlike)
                //            UpdateActivityOnServer(FriendicaActivity.like);
                //        else if (postActivity.Activity == FriendicaActivity.undislike)
                //            UpdateActivityOnServer(FriendicaActivity.dislike);
                //        else
                //            UpdateActivityOnServer(postActivity.Activity);
                //    }
                //    break;
                //case HttpStatusCode.BadRequest:
                //default:
                    //// an error has occurred, info to user with error message
                    //errorMsg = AppResources.messageDialogErrorOnActivitySettingBadRequest;
                    //errorMsg = String.Format(errorMsg, postActivity.ReturnString);
                    //await StaticMessageDialog.ShowDialogAsync("", errorMsg, AppResources.buttonOK);
                    //break;
            //}

            //if (postActivity.Activity == FriendicaActivity.like || postActivity.Activity == FriendicaActivity.unlike)
            //    IsUpdatingLikes = false;
            //else if (postActivity.Activity == FriendicaActivity.dislike || postActivity.Activity == FriendicaActivity.undislike)
                //IsUpdatingDislikes = false;

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
                var result = await Application.Current.MainPage.DisplayAlert("", errorMsg, AppResources.buttonYes, AppResources.buttonNo);

                // user wants to continue with the retweet
                if (result)
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
                await Application.Current.MainPage.DisplayAlert("", errorMsg, AppResources.buttonOK);
            }
        }

        private bool ChangeListAfterSuccessfulUpdate()
        {
            //switch (App.Posts.Activity)
            //{
            //    case FriendicaActivity.like:
            //        // add authenticated user
            //        var user = new FriendicaUser()
            //        {
            //            IsAuthenticatedUser = true
            //        };
            //        Post.PostFriendicaActivities.ActivitiesLike.Add(user);
            //        break;
            //    case FriendicaActivity.dislike:
            //        // add authenticated user
            //        user = new FriendicaUser()
            //        {
            //            IsAuthenticatedUser = true
            //        };
            //        Post.PostFriendicaActivities.ActivitiesDislike.Add(user);
            //        break;
            //    case FriendicaActivity.unlike:
            //        // remove authenticated user
            //        var post = Post.PostFriendicaActivities.ActivitiesLike.SingleOrDefault(u => u.IsAuthenticatedUser == true);
            //        Post.PostFriendicaActivities.ActivitiesLike.Remove(post);
            //        break;
            //    case FriendicaActivity.undislike:
            //        // remove authenticated user
            //        post = Post.PostFriendicaActivities.ActivitiesDislike.SingleOrDefault(u => u.IsAuthenticatedUser == true);
            //        Post.PostFriendicaActivities.ActivitiesDislike.Remove(post);
            //        break;
            //    default:
            //        return false;
            //}
            //SetActivitiesParameters();
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
                // TODO: Achtung: auf Windows möchten wir hier nicht auf die LocalSettings sondern auf die RemoteSettings zurückgreifen,
                // TODO: falls User das nicht deaktiviert hat, bei Android kann man das auch irgendwie synchronisieren (siehe Wear und Phone)
                // TODO: Achtung: Liste der Id's alleine genügt nicht, es könnte auf einem anderen Gerät ein anderes Friendica-Konto eingerichtet sein
                IsNewEntry = (Post.PostId > Settings.LastReadNetworkPost) ? true : false;
                if (Settings.UnseenNewsfeedItems.Contains((int)Post.PostId))
                    IsNewEntry = true;
            }
        }
    }
}
