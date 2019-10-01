using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Strings;
using Xamarin.Forms;

namespace Friendica_Mobile.Models
{
    public class NewPostModel : BindableClass
    {
        public event EventHandler SendNewPostFinished;

        private HttpFriendicaPosts httpRequest;

        // title of the post
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        // status text of the post
        public string Status { get; set;}
        // status html of the post
        public string HtmlStatus { get; set; }
        // id of the status which is replied to
        public double? InReplyToStatusId { get; set; }

        // image data in byte array
        private byte[] _media;
        public byte[] Media
        {
            get { return _media; }
            set { SetProperty(ref _media, value); }
        }

        // latitude of post (taken from image if possible)
        private double _latitude;
        public double Latitude
        {
            get { return _latitude; }
            set { SetProperty(ref _latitude, value); }
        }

        // longitude of post (taken from image if possible)
        private double _longitude;
        public double Longitude
        {
            get { return _longitude; }
            set { SetProperty(ref _longitude, value); }
        }

        // indicator of source of post
        public string Source { get; set; }

        // strings with the group and contact id's allowed or denied for this new post
        public string GroupAllow { get; set; }
        public string GroupDeny { get; set; }
        public string ContactAllow { get; set; }
        public string ContactDeny { get; set; }

        // indicator of network
        public string Network { get; set; }


        // constructor
        public NewPostModel()
        {
            Source = "Friendica Mobile";
            Network = "dfrn";
        }


        /// <summary>
        /// Send a new post to server and react on the different possible error types
        /// </summary>
        /// <returns></returns>
        public async Task SendNewPost()
        {
            if (httpRequest == null)
                httpRequest = new HttpFriendicaPosts(Settings.FriendicaServer, Settings.FriendicaUsername, Settings.FriendicaPassword);

            var result = await httpRequest.PostNewPostAsync(this);

            switch (result)
            {
                case HttpFriendicaPosts.PostFriendicaPostsResults.NotAnswered:
                    // retry sending if user wishes to do so, otherwise fire event to remove new post
                    var answer = await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleNewPost_Text,
                                        AppResources.MessageDialogNewPostNotAnsweredError,
                                        AppResources.buttonYes, AppResources.buttonNo);
                    if (answer)
                    {
                        await SendNewPost();
                    }
                    else
                        SendNewPostFinished?.Invoke(this, EventArgs.Empty);
                    break;
                case HttpFriendicaPosts.PostFriendicaPostsResults.ImagickError:
                    // photo might be too big for server, or some other error occurred on saving the image on server
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleNewPost_Text,
                                        AppResources.MessageDialogNewPostImageUploadError,
                                        AppResources.buttonOK);
                    SendNewPostFinished?.Invoke(this, EventArgs.Empty);
                    break;
                case HttpFriendicaPosts.PostFriendicaPostsResults.TooManyRequests:
                    // Friendica server is checking counts of daily, weekly and monthly posts via API
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleNewPost_Text,
                                        AppResources.MessageDialogNewPostTooManyRequestsError,
                                        AppResources.buttonOK);
                    SendNewPostFinished?.Invoke(this, EventArgs.Empty);
                    break;
                case HttpFriendicaPosts.PostFriendicaPostsResults.OK:
                    // create an internal notification informing that sending was successfull (displayed via MasterDetailControl.xaml.cs)
                    // notification has always the same title, but body is new post title (if available) or new post body (if available)
                    // if new post contained a photo, we will display a small thumbnail
                    string notificationBody = "";
                    if (!string.IsNullOrEmpty(Title))
                        notificationBody = Title;
                    else if (!string.IsNullOrEmpty(HtmlStatus))
                    {
                        var html = new HtmlToXamarinViews(HtmlStatus);
                        var plainText = html.ConvertHtmlToPlainText();
                        notificationBody = (plainText.Length > 100) ? plainText.Substring(0, 100) + "..." : plainText;
                    }
                    App.Notification = new InternalNotification() {
                        Title = AppResources.MessageDialogNewPostSuccessfullSent,
                        Text = notificationBody,
                        Image = (Media != null) ? ImageSource.FromStream(() => new MemoryStream(Media)) : null };
                    SendNewPostFinished?.Invoke(this, EventArgs.Empty);
                    break;
                case HttpFriendicaPosts.PostFriendicaPostsResults.SampleMode:
                case HttpFriendicaPosts.PostFriendicaPostsResults.NotAuthenticated:
                case HttpFriendicaPosts.PostFriendicaPostsResults.SerializationError:
                case HttpFriendicaPosts.PostFriendicaPostsResults.UnexpectedError:
                default:
                    // display a general error message on all other returned errors
                    await Application.Current.MainPage.DisplayAlert(AppResources.pageTitleNewPost_Text,
                                string.Format(AppResources.MessageDialogGeneralErrorMessage, "HTTP/" + httpRequest.StatusCode, httpRequest.ReturnString),
                                AppResources.buttonOK);
                    SendNewPostFinished?.Invoke(this, EventArgs.Empty);
                    break;
            }
            
        }
    }
}
