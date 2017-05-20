using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Friendica_Mobile.HttpRequests
{
    public class GetFriendicaPhotos : clsHttpRequests
    {
        AppSettings appSettings = new AppSettings();
        public enum PhotoErrors { OK, NoServerSupport, AuthenticationFailed, ServerNotAnswered, NoPhotosAvailable, AclDataInvalid,
                            NoPhotoDataReturned, NoPhotoFound, NoAlbumnameSpecified, NoPhotoIdSpecified, AlbumNotAvailable,
                            NoMediaDataSubmitted, ImageSizeLimitsExceeded, ProcessImageDataFailed, ImageUploadFailed, UnknownError };
        public bool IsErrorOccurred { get; set; }
        public PhotoErrors ErrorPhotoFriendica { get; set; }

        // class data specific properties 
        public List<FriendicaPhotolist> PhotolistReturned { get; set; }
        public FriendicaPhoto PhotoReturned { get; set; }
        public FriendicaUser UserReturned { get; set; }

        // local error generated when parsing
        private string _errorMessageOnParsing;
        public string ErrorMessageOnParsing
        {
            get { return _errorMessageOnParsing; }
        }


        public event EventHandler FriendicaPhotosLoaded;
        protected virtual void OnFriendicaPhotosLoaded()
        {
            FriendicaPhotosLoaded?.Invoke(this, EventArgs.Empty);
        }


        public GetFriendicaPhotos()
        {
        }


        public async Task<GetFriendicaPhotos> CheckServerSupportAsync()
        {
            // we will use account/update_profile_image here, as any call starting with /api/friendica/photo 
            // will be answered on old versions, too. So new calls with friendica/photoalbum and new methods 
            // photos/create, photos/delete or photos/update don't get a NotImplemented return
            var url = String.Format("{0}/api/account/update_profile_image.json?timestamp={1}",
                appSettings.FriendicaServer,
                DateTime.Now.ToString());
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);

            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                default:
                    IsErrorOccurred = false;
                    ErrorPhotoFriendica = PhotoErrors.OK;
                    break;
            }

            return this;
        }


        public async Task<GetFriendicaPhotos> LoadPhotoFromServerAsync(string photoId)
        {
            var url = String.Format("{0}/api/friendica/photo.json?photo_id={1}&timestamp={2}",
                appSettings.FriendicaServer,
                photoId,
                DateTime.Now.ToString());
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);

            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.Ok:
                    PhotoReturned = new FriendicaPhoto(this.ReturnString);
                    break;
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                default:
                    IsErrorOccurred = false;
                    ErrorPhotoFriendica = PhotoErrors.OK;
                    break;
            }

            return this;
        }


        public async Task<GetFriendicaPhotos> LoadPhotoFromServerAsync(string photoId, string scale)
        {
            var url = String.Format("{0}/api/friendica/photo.json?photo_id={1}&scale={2}&timestamp={3}",
                appSettings.FriendicaServer,
                photoId,
                scale,
                DateTime.Now.ToString());
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);

            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.Ok:
                    PhotoReturned = new FriendicaPhoto(this.ReturnString);
                    if (PhotoReturned.PhotoData == null)
                    {
                        IsErrorOccurred = true;
                        ErrorPhotoFriendica = PhotoErrors.NoPhotoDataReturned;
                    }
                    else
                    {
                        IsErrorOccurred = false;
                        ErrorPhotoFriendica = PhotoErrors.OK;
                    }
                    break;
                case Windows.Web.Http.HttpStatusCode.NotFound:
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoPhotoFound;
                    break;
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                default:
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    break;
            }

            return this;
        }


        public async void LoadPhotoalbums()
        {
            var url = String.Format("{0}/api/friendica/photos/list.json?timestamp={1}",
                appSettings.FriendicaServer,
                DateTime.Now.ToString());
            this.RequestFinished += GetFriendicaPhotos_RequestFinished;
            await this.GetString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);
        }


        private void GetFriendicaPhotos_RequestFinished(object sender, EventArgs e)
        {
            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.Ok:
                    IsErrorOccurred = false;
                    try
                    {
                        PhotolistReturned = ConvertJsonReturnToPhotolist();
                        if (PhotolistReturned.Count == 0)
                        {
                            IsErrorOccurred = true;
                            ErrorPhotoFriendica = PhotoErrors.NoPhotosAvailable;
                        }
                    }
                    catch (Exception ex)
                    {
                        _errorMessageOnParsing = ex.Message;
                        ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    }
                    break;
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                default:
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    break;
            }
            OnFriendicaPhotosLoaded();
        }


        private List<FriendicaPhotolist> ConvertJsonReturnToPhotolist(JsonArray array = null)
        {
            JsonArray resultArray;

            if (array != null)
                resultArray = array;
            else
                resultArray = JsonArray.Parse(this.ReturnString);

            var list = new List<FriendicaPhotolist>();
            int arraySize = resultArray.Count;
            for (int i = 0; i < arraySize; i++)
            {
                IJsonValue element = resultArray.GetArray()[i];
                switch (element.ValueType)
                {
                    case JsonValueType.Object:
                        var result = new FriendicaPhotolist(element.ToString());
                        list.Add(result);
                        break;
                }
            }
            return list;
        }


        public async Task<GetFriendicaPhotos> DeletePhotoalbumAsync(string album)
        {
            var url = String.Format("{0}/api/friendica/photoalbum/delete.json?album={1}",
                appSettings.FriendicaServer,
                album);
            await this.DeleteString(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);

            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                case Windows.Web.Http.HttpStatusCode.BadRequest:
                    // api return BadRequest because no albumname or wrong albumname
                    if (this.ReturnString.Contains("no albumname specified"))
                        ErrorPhotoFriendica = PhotoErrors.NoAlbumnameSpecified;
                    else if (this.ReturnString.Contains("album not available"))
                        ErrorPhotoFriendica = PhotoErrors.AlbumNotAvailable;
                    IsErrorOccurred = true;
                    break;
                case Windows.Web.Http.HttpStatusCode.InternalServerError:
                    // "problem with deleting items occured" when no entry in items was found
                    // "unknown error - deleting from database failed" --> should never occur as api checks against this and throw BadRequest
                    ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    IsErrorOccurred = true;
                    break;
                default:
                    IsErrorOccurred = false;
                    ErrorPhotoFriendica = PhotoErrors.OK;
                    break;
            }

            return this;
        }


        public async Task<GetFriendicaPhotos> UpdatePhotoalbumAsync(string album, string albumNew)
        {
            var url = String.Format("{0}/api/friendica/photoalbum/update.json?album={1}&album_new={2}",
                appSettings.FriendicaServer,
                album,
                albumNew);
            await this.PostStringAsync(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword, "");

            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                case Windows.Web.Http.HttpStatusCode.BadRequest:
                    // api return BadRequest because no albumname or wrong albumname
                    if (this.ReturnString.Contains("no albumname specified") || this.ReturnString.Contains("no new albumname specified"))
                        ErrorPhotoFriendica = PhotoErrors.NoAlbumnameSpecified;
                    else if (this.ReturnString.Contains("album not available"))
                        ErrorPhotoFriendica = PhotoErrors.AlbumNotAvailable;
                    IsErrorOccurred = true;
                    break;
                case Windows.Web.Http.HttpStatusCode.InternalServerError:
                    // "unknown error - updating in database failed" --> should never occur, checks throw BadRequest before
                    ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    IsErrorOccurred = true;
                    break;
                default:
                    IsErrorOccurred = false;
                    ErrorPhotoFriendica = PhotoErrors.OK;
                    break;
            }

            return this;
        }

        public async Task<GetFriendicaPhotos> PostUpdatePhotoAsync(Dictionary<string, object> data, string contentType, string filename)
        {
            var url = String.Format("{0}/api/friendica/photo/update.json",
                appSettings.FriendicaServer);
            return await PostPhotoDataAsync(url, data, contentType, filename);
        }

        public async Task<GetFriendicaPhotos> PostCreatePhotoAsync(Dictionary<string, object> data, string contentType, string filename)
        {
            var url = String.Format("{0}/api/friendica/photo/create.json",
                appSettings.FriendicaServer);
            return await PostPhotoDataAsync(url, data, contentType, filename);
        }

        public async Task<GetFriendicaPhotos> PostCreateProfileimageAsync(Dictionary<string, object> data, string contentType, string filename)
        {
            var url = String.Format("{0}/api/account/update_profile_image.json",
                appSettings.FriendicaServer);
            return await PostPhotoDataAsync(url, data, contentType, filename);
        }

        private async Task<GetFriendicaPhotos> PostPhotoDataAsync(string url, Dictionary<string, object> data, string contentType, string filename)
        {
            await this.PostMultipartAsync(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword, data, contentType, filename);

            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                case Windows.Web.Http.HttpStatusCode.BadRequest:
                    // api return BadRequest because no albumname or wrong albumname
                    if (this.ReturnString.Contains("no media data submitted"))
                        ErrorPhotoFriendica = PhotoErrors.NoMediaDataSubmitted;
                    else if (this.ReturnString.Contains("no albumname specified"))
                        ErrorPhotoFriendica = PhotoErrors.NoAlbumnameSpecified;
                    else if (this.ReturnString.Contains("photo not available"))
                        ErrorPhotoFriendica = PhotoErrors.NoPhotoFound;
                    else if (this.ReturnString.Contains("acl data invalid"))
                        ErrorPhotoFriendica = PhotoErrors.AclDataInvalid;
                    IsErrorOccurred = true;
                    break;
                case Windows.Web.Http.HttpStatusCode.InternalServerError:
                    // api return InternalServerError on errors with loading photo data into database
                    if (this.ReturnString.Contains("image size exceeds Friendica config setting") ||
                        this.ReturnString.Contains("image size exceeds PHP config settings"))
                        ErrorPhotoFriendica = PhotoErrors.ImageSizeLimitsExceeded;
                    else if (this.ReturnString.Contains("unable to process image data"))
                        ErrorPhotoFriendica = PhotoErrors.ProcessImageDataFailed;
                    else if (this.ReturnString.Contains("image upload failed"))
                        ErrorPhotoFriendica = PhotoErrors.ImageUploadFailed;
                    else if (this.ReturnString.Contains("unknown error"))
                        ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    IsErrorOccurred = true;
                    break;
                default:
                    if (this.ReturnString == "")
                    {
                        IsErrorOccurred = true;
                        ErrorPhotoFriendica = PhotoErrors.UnknownError;
                        PhotoReturned = null;
                    }
                    else
                    {
                        IsErrorOccurred = false;
                        ErrorPhotoFriendica = PhotoErrors.OK;
                        // profileimage returns a user object, different reaction needed
                        if (url.Contains("/api/friendica/photo/update.json"))
                            PhotoReturned = null;
                        else if (url.Contains("/api/account/update_profile_image.json"))
                            UserReturned = new FriendicaUser(this.ReturnString);
                        else
                            PhotoReturned = new FriendicaPhoto(this.ReturnString);
                    }
                    break;
            }
            return this;
        }

        public async Task<GetFriendicaPhotos> DeletePhotoAsync(string photo_id)
        {
            var url = String.Format("{0}/api/friendica/photo/delete.json?photo_id={1}",
                        appSettings.FriendicaServer, 
                        photo_id);

            await this.DeleteStringAsync(url, appSettings.FriendicaUsername, appSettings.FriendicaPassword);

            switch (this.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.None:
                    // server has not answered
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.ServerNotAnswered;
                    break;
                case Windows.Web.Http.HttpStatusCode.NotImplemented:
                    // api call is not available
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.NoServerSupport;
                    break;
                case Windows.Web.Http.HttpStatusCode.Forbidden:
                    // api return 403 Forbidden (user not logged in)
                    IsErrorOccurred = true;
                    ErrorPhotoFriendica = PhotoErrors.AuthenticationFailed;
                    break;
                case Windows.Web.Http.HttpStatusCode.BadRequest:
                    // api return BadRequest because no albumname or wrong albumname
                    if (this.ReturnString.Contains("no photo_id specified"))
                        ErrorPhotoFriendica = PhotoErrors.NoPhotoIdSpecified;
                    else if (this.ReturnString.Contains("photo not available"))
                        ErrorPhotoFriendica = PhotoErrors.NoPhotoFound;
                    IsErrorOccurred = true;
                    break;
                case Windows.Web.Http.HttpStatusCode.InternalServerError:
                    // api return InternalServerError on errors with loading photo data into database
                    if (this.ReturnString.Contains("unknown error on deleting photo") ||
                        this.ReturnString.Contains("problem with deleting items occured"))
                        ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    IsErrorOccurred = true;
                    break;
                default:
                    if (this.ReturnString == "")
                    {
                        IsErrorOccurred = true;
                        ErrorPhotoFriendica = PhotoErrors.UnknownError;
                    }
                    else
                    {
                        IsErrorOccurred = false;
                        ErrorPhotoFriendica = PhotoErrors.OK;
                    }
                    PhotoReturned = null;
                    break;
            }
            return this;
        }

    }
}