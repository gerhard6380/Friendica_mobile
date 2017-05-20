using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using Friendica_Mobile.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;


namespace Friendica_Mobile.Mvvm
{
    public class PhotosViewmodel : BindableClass
    {
        #region properties

        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum PhotosViewStates { Fullmode, OnlyAlbums, OnlyPhotos };

        public Thickness BottomAppBarMargin { get { return App.Settings.BottomAppBarMargin; } }
        public double ListViewWidth { get { return App.Settings.ShellWidth; } }
        public double ListViewHeight {  get { return App.Settings.ShellHeight; } }

        // status of current view
        private PhotosViewStates _photosView;
        public PhotosViewStates PhotosView
        {
            get { return _photosView; }
            set { _photosView = value;
                if (value == PhotosViewStates.Fullmode)
                    App.PhotosNavigatedIntoAlbum = false;
                OnPropertyChanged("PhotosView"); }
        }

        // indicator if a server operation is currently in progress (avoid user to make other changes until operations are finished)
        private bool _isServerOperationPending;
        public bool IsServerOperationPending
        {
            get { return _isServerOperationPending; }
            set { _isServerOperationPending = value;
                OnPropertyChanged("IsServerOperationPending");
                AddPhotoalbumCommand.RaiseCanExecuteChanged();
                EditPhotoalbumCommand.RaiseCanExecuteChanged();
                DeletePhotoalbumCommand.RaiseCanExecuteChanged();
            }
        }

        // indicator if app is loading new photos into view for uploading (later started by user)
        private bool _isLoadingNewPhotos;
        public bool IsLoadingNewPhotos
        {
            get { return _isLoadingNewPhotos; }
            set { _isLoadingNewPhotos = value;
                OnPropertyChanged("IsLoadingNewPhotos"); }
        }


        // local indicators if different processes are already running
        private bool _isSavingChangedAlbumname;

        // indicator on top for loading albums
        private bool _isLoadingPhotoalbums;
        public bool IsLoadingPhotoalbums
        {
            get { return _isLoadingPhotoalbums; }
            set
            {
                _isLoadingPhotoalbums = value;
                AddPhotoalbumCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingPhotoalbums");
            }
        }

        // property with the localized captions for the profileimages album
        private Dictionary<string, string> _profileimagesCaption;
        public Dictionary<string, string> ProfileimagesCaption
        {
            get { return _profileimagesCaption; }
            set { _profileimagesCaption = value; }
        }

        // indicator if server is not supporting messages API for App
        private bool _noServerSupport;
        public bool NoServerSupport
        {
            get { return _noServerSupport; }
            set
            {
                _noServerSupport = value;
                ReloadPhotosCommand.RaiseCanExecuteChanged();
                AddPhotoalbumCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("NoServerSupport");
            }
        }

        // indicator if no settings available (then display sample conversations)
        private bool _noSettings;
        public bool NoSettings
        {
            get { return _noSettings; }
            set
            {
                _noSettings = value;
                ReloadPhotosCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("NoSettings");
            }
        }

        // true if server contains no photos of the authenticated user
        private bool _noPhotosAvailable;
        public bool NoPhotosAvailable
        {
            get { return _noPhotosAvailable; }
            set { _noPhotosAvailable = value;
                OnPropertyChanged("NoPhotosAvailable");
            }
        }

        // contains the albums from the server
        private ObservableCollection<FriendicaPhotoalbum> _albums;
        public ObservableCollection<FriendicaPhotoalbum> Albums
        {
            get { return _albums; }
            set
            {
                _albums = value;
                OnPropertyChanged("Albums");
            }
        }

        // contains the selected album
        private FriendicaPhotoalbum _selectedPhotoalbum;
        public FriendicaPhotoalbum SelectedPhotoalbum
        {
            get { return _selectedPhotoalbum; }
            set
            {
                if (SelectedPhotoalbum != null)
                {
                    IsSelectedPhotoalbum = false;
                    SelectedPhotoalbum.PrintButtonClicked -= SelectedPhotoalbum_PrintButtonClicked;
                    SelectedPhotoalbum.LastPhotoDeleted -= SelectedPhotoalbum_LastPhotoDeleted;
                    SelectedPhotoalbum.NewProfileimageRequested -= SelectedPhotoalbum_NewProfileimageRequested;
                    SelectedPhotoalbum.MovePhotoToAlbumRequested -= SelectedPhotoalbum_MovePhotoToAlbumRequested;
                    SelectedPhotoalbum.CheckLoadingStatusRequested -= SelectedPhotoalbum_CheckLoadingStatusRequested;
                }
                _selectedPhotoalbum = value;
                if (value == null)
                    IsSelectedPhotoalbum = false;
                else
                {
                    IsSelectedPhotoalbum = true;
                    SelectedPhotoalbum.PrintButtonClicked += SelectedPhotoalbum_PrintButtonClicked;
                    SelectedPhotoalbum.LastPhotoDeleted += SelectedPhotoalbum_LastPhotoDeleted;
                    SelectedPhotoalbum.NewProfileimageRequested += SelectedPhotoalbum_NewProfileimageRequested;
                    SelectedPhotoalbum.MovePhotoToAlbumRequested += SelectedPhotoalbum_MovePhotoToAlbumRequested;
                    SelectedPhotoalbum.CheckLoadingStatusRequested += SelectedPhotoalbum_CheckLoadingStatusRequested;
                    SetSelectablePhotoalbums();
                    PhotoalbumUpdated?.Invoke(value, EventArgs.Empty);
                }
                EditPhotoalbumCommand.RaiseCanExecuteChanged();
                DeletePhotoalbumCommand.RaiseCanExecuteChanged();
                AddFromDeviceCommand.RaiseCanExecuteChanged();
                AddFromCameraCommand.RaiseCanExecuteChanged();
                AddEmptyInkCanvasCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedPhotoalbum");
            }
        }

        // indicator if user has selected something
        private bool _isSelectedPhotoalbum;
        public bool IsSelectedPhotoalbum
        {
            get { return _isSelectedPhotoalbum; }
            set
            {
                _isSelectedPhotoalbum = value;
                OnPropertyChanged("IsSelectedPhotoalbum");
            }
        }

        // indicator that user enabled the albumname editing mode
        private bool _albumEditingEnabled;
        public bool AlbumEditingEnabled
        {
            get { return _albumEditingEnabled; }
            set { _albumEditingEnabled = value;
                OnPropertyChanged("AlbumEditingEnabled"); }
        }

        // indicator on top for probing servers support
        private bool _isCheckingServerSupport;
        public bool IsCheckingServerSupport
        {
            get { return _isCheckingServerSupport; }
            set
            {
                _isCheckingServerSupport = value;
                OnPropertyChanged("IsCheckingServerSupport");
            }
        }

        // indicator which is set to true if there are still photos which are not uploaded to server
        private bool _hasUnsavedContent;
        public bool HasUnsavedContent
        {
            get { return _hasUnsavedContent; }
            set { _hasUnsavedContent = value; }
        }

        #endregion


        #region commands

        // add album button
        Mvvm.Command _addPhotoalbumCommand;
        public Mvvm.Command AddPhotoalbumCommand { get { return _addPhotoalbumCommand ?? (_addPhotoalbumCommand = new Mvvm.Command(ExecuteAddPhotoalbum, CanAddPhotoalbum)); } }
        private bool CanAddPhotoalbum()
        {
            if (IsLoadingPhotoalbums || NoServerSupport || IsServerOperationPending)
                return false;
            else
                return true;
        }

        private void ExecuteAddPhotoalbum()
        {
            // create new photoalbum
            var newAlbum = new FriendicaPhotoalbum();

            // get initial name of new album ("Neues Album"; if already existing add a " (x)" with the next free number)
            newAlbum.Albumname = GetNewAlbumname();
            newAlbum.NewAlbumVisible = true;

            // insert at first place and select the Album
            Albums.Insert(0, newAlbum);
            SelectedPhotoalbum = newAlbum;
            NewPhotoalbumAdded?.Invoke(newAlbum, EventArgs.Empty);
        }


        // refresh button
        Mvvm.Command _reloadPhotosCommand;
        public Mvvm.Command ReloadPhotosCommand { get { return _reloadPhotosCommand ?? (_reloadPhotosCommand = new Mvvm.Command(ExecuteReloadPhotos, CanReloadPhotos)); } }
        private bool CanReloadPhotos()
        {
            return (!NoServerSupport);
        }

        private async void ExecuteReloadPhotos()
        {
            // if there is a new album not yet saved to server we cannot reload the data
            if (Albums.Any(a => a.NewAlbumVisible))
            {
                // ask user: new album created, but not yet save. Discard new album and continue reload?
                string errorMsg;
                errorMsg = String.Format(loader.GetString("messageDialogPhotosNewAlbumsNotSaved"));
                var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                await dialog.ShowDialog(0, 1);

                if (dialog.Result == 1)
                    return;  // cancel means aborting reload
            }

            // remove new albums from list before we continue, otherwise we will get exceptions
            var list = Albums.Where(a => a.NewAlbumVisible).ToList();
            foreach (var album in list)
                Albums.Remove(album);
             
            // load photos new from server
            LoadContentFromServer();
        }


        // add photo from Device button
        Mvvm.Command<string> _addFromDeviceCommand;
        public Mvvm.Command<string> AddFromDeviceCommand { get { return _addFromDeviceCommand ?? (_addFromDeviceCommand = new Mvvm.Command<string>(ExecuteAddFromDevice, CanAddFromDevice)); } }
        private bool CanAddFromDevice(string imageType)
        {
            // only active if user has selected an album
            if (SelectedPhotoalbum == null)
                return false;
            else
            {
                // retrieve correct localized profileimages folder
                var localizedProfileimagesName = ProfileimagesCaption.Where(l => l.Key == CultureInfo.CurrentCulture.TwoLetterISOLanguageName).ToList();
                string albumProfileimages = "Profile Photos";
                if (localizedProfileimagesName.Count == 1)
                    albumProfileimages = localizedProfileimagesName[0].Value;

                if (SelectedPhotoalbum.Albumname == albumProfileimages && imageType.ToLower() == "photo")
                    return false;
                else if (SelectedPhotoalbum.Albumname != albumProfileimages && imageType.ToLower() == "profileimage")
                    return false;
                else
                    return true;
            }
        }

        private async void ExecuteAddFromDevice(string imageType)
        {
            // show dialog to open one or more images
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");

            if (imageType == "profileimage")
            {
                var image = await openPicker.PickSingleFileAsync();
                if (image == null)
                    return;
                IsLoadingNewPhotos = true;
                await PreparePhotoFileForUploadAsync(image, imageType);
            }
            else
            {
                var fileList = await openPicker.PickMultipleFilesAsync();
                IsLoadingNewPhotos = true;
                foreach (var image in fileList)
                {
                    await PreparePhotoFileForUploadAsync(image, imageType);
                }
            }
            IsLoadingNewPhotos = false;

            // navigate to cropper if we are working on a profileimage
            if (imageType == "profileimage")
            {
                var frame = App.GetFrameForNavigation();
                frame.Navigate(typeof(A7_PhotosCropping));
            }
        }



        private void Photo_NewPhotoUploaded(object sender, EventArgs e)
        {
            // set selected photo to the newly uploaded photo
            var photo = sender as FriendicaPhotoExtended;
            SelectedPhotoalbum.SelectedPhoto = photo;
            if (SelectedPhotoalbum.NewAlbumVisible)
                SelectedPhotoalbum.NewAlbumVisible = false;

            // recreate 
            SelectedPhotoalbum.SetPhotosForAlbumView();
        }


        // add photo from Camera button
        Mvvm.Command<string> _addFromCameraCommand;
        public Mvvm.Command<string> AddFromCameraCommand { get { return _addFromCameraCommand ?? (_addFromCameraCommand = new Mvvm.Command<string>(ExecuteAddFromCamera, CanAddFromCamera)); } }
        private bool CanAddFromCamera(string imageType)
        {
            // check if an album is selected
            if (SelectedPhotoalbum == null)
                return false;
            else
            {
                // retrieve correct localized profileimages folder
                var localizedProfileimagesName = ProfileimagesCaption.Where(l => l.Key == CultureInfo.CurrentCulture.TwoLetterISOLanguageName).ToList();
                string albumProfileimages = "Profile Photos";
                if (localizedProfileimagesName.Count == 1)
                    albumProfileimages = localizedProfileimagesName[0].Value;

                if (SelectedPhotoalbum.Albumname == albumProfileimages && imageType.ToLower() == "photo")
                    return false;
                else if (SelectedPhotoalbum.Albumname != albumProfileimages && imageType.ToLower() == "profileimage")
                    return false;
                else
                    return App.DeviceHasCamera;
            }
        }

        private async void ExecuteAddFromCamera(string imageType)
        {
            // show dialog to shoot photo with camera
            var cameraCapture = new CameraCaptureUI();
            cameraCapture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            var file = await cameraCapture.CaptureFileAsync(CameraCaptureUIMode.Photo);

            // operate on returned image
            if (file != null)
            {
                IsLoadingNewPhotos = true;
                await PreparePhotoFileForUploadAsync(file, imageType);
                IsLoadingNewPhotos = false;
            }

            // navigate to cropper if we are working on a profileimage
            if (imageType == "profileimage")
            {
                var frame = App.GetFrameForNavigation();
                frame.Navigate(typeof(A7_PhotosCropping));
            }
        }


        // add empty ink canvas button
        Mvvm.Command _addEmptyInkCanvasCommand;
        public Mvvm.Command AddEmptyInkCanvasCommand { get { return _addEmptyInkCanvasCommand ?? (_addEmptyInkCanvasCommand = new Mvvm.Command(ExecuteAddEmptyInkCanvas, CanAddEmptyInkCanvas)); } }
        private bool CanAddEmptyInkCanvas()
        {
            // only provide option if an album has been selected
            if (SelectedPhotoalbum == null)
                return false;
            else
                return true;
        }

        private void ExecuteAddEmptyInkCanvas()
        {
            // adding a new empty photo and set album and acl
            var photo = new FriendicaPhotoExtended();
            photo.NewPhotoUploaded += Photo_NewPhotoUploaded;
            photo.CheckLoadingStatusRequested += Photo_CheckLoadingStatusRequested;
            photo.NewUploadPlanned = true;
            photo.Photo.PhotoAlbum = SelectedPhotoalbum.Albumname;
            photo.GetDefaultACL();
            SelectedPhotoalbum.PhotosInAlbum.Add(photo);
            SelectedPhotoalbum.SelectedPhoto = photo;

            // navigate to A5_InkCanvas
            var frame = App.GetFrameForNavigation();
            frame.Navigate(typeof(A5_InkCanvas), photo);
        }

        private void Photo_CheckLoadingStatusRequested(object sender, EventArgs e)
        {
            CheckLoadingStatus();
        }


        // edit photoalbum button
        Mvvm.Command _editPhotoalbumCommand;
        public Mvvm.Command EditPhotoalbumCommand { get { return _editPhotoalbumCommand ?? (_editPhotoalbumCommand = new Mvvm.Command(ExecuteEditPhotoalbum, CanEditPhotoalbum)); } }
        private bool CanEditPhotoalbum()
        {
            // don't allow user to change the album name if other server operations (uploads, deletes etc.) are currently running
            // this avoids that user wants to change a meanwhile deleted album or change the name during a mass upload
            return (!IsServerOperationPending && SelectedPhotoalbum != null) ? true : false;
        }
        
        private void ExecuteEditPhotoalbum()
        {
            // copy current album name for editing into variable for new text and enable editor
            SelectedPhotoalbum.AlbumnameNew = SelectedPhotoalbum.Albumname;
            AlbumEditingEnabled = true;
        }


        // save changed albumname to server
        Mvvm.Command _saveChangedAlbumnameCommand;
        public Mvvm.Command SaveChangedAlbumnameCommand { get { return _saveChangedAlbumnameCommand ?? (_saveChangedAlbumnameCommand = new Mvvm.Command(ExecuteSaveChangedAlbumname, CanSaveChangedAlbumname)); } }
        private bool CanSaveChangedAlbumname()
        {
            return (!_isSavingChangedAlbumname) ? true : false;
        }

        private async void ExecuteSaveChangedAlbumname()
        {
            // indicator to avoid double calling the function
            _isSavingChangedAlbumname = true;

            // user wants to save an empty string, we inform him that is not possible
            if (SelectedPhotoalbum.AlbumnameNew == "")
            {
                string errorMsg = loader.GetString("messageDialogPhotosNewAlbumnameEmpty");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                _isSavingChangedAlbumname = false;
                return;
            }

            // check if text has not changed, then cancel and go back to editing
            if (SelectedPhotoalbum.Albumname == SelectedPhotoalbum.AlbumnameNew)
            {
                if (!SelectedPhotoalbum.NewAlbumVisible)
                {
                    // message makes only sense if album exists already on server
                    string errorMsg = loader.GetString("messageDialogPhotosNewAlbumnameUnchanged");
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                }
                AlbumEditingEnabled = false;
                _isSavingChangedAlbumname = false;
                return;
            }

            // if we have a new album (not yet uploaded to server) just change the name
            if (SelectedPhotoalbum.NewAlbumVisible)
            {
                SelectedPhotoalbum.Albumname = SelectedPhotoalbum.AlbumnameNew;
                AlbumEditingEnabled = false;
                _isSavingChangedAlbumname = false;
                return;
            }

            // if we are in sample mode just inform user
            if (NoSettings)
            {
                // "this would now perform a server operation, in sample mode not possible"
                var errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                SelectedPhotoalbum.Albumname = SelectedPhotoalbum.AlbumnameNew;
                AlbumEditingEnabled = false;
                _isSavingChangedAlbumname = false;
                return;
            }

            // now we are in the loop to change the albumname on server
            IsServerOperationPending = true;
            var getHttpUpdatePhotoalbum = new GetFriendicaPhotos();
            await getHttpUpdatePhotoalbum.UpdatePhotoalbumAsync(SelectedPhotoalbum.Albumname, SelectedPhotoalbum.AlbumnameNew);

            switch (getHttpUpdatePhotoalbum.ErrorPhotoFriendica)
            {
                case GetFriendicaPhotos.PhotoErrors.OK:
                    // successfully changed name on server, change it now in the app too
                    var newName = SelectedPhotoalbum;
                    SelectedPhotoalbum.Albumname = SelectedPhotoalbum.AlbumnameNew;
                    // change the name within all photos in the album too
                    foreach (var photo in SelectedPhotoalbum.PhotosInAlbum)
                        photo.Photo.PhotoAlbum = SelectedPhotoalbum.Albumname;
                    SelectedPhotoalbum.AlbumnameNew = "";
                    SortAlbums();
                    PhotoalbumUpdated?.Invoke(newName, EventArgs.Empty);
                    break;
                case GetFriendicaPhotos.PhotoErrors.ServerNotAnswered:
                    // ask for retry
                    var errorMsgRetry = String.Format(loader.GetString("messageDialogPhotosAlbumUpdatingNoReaction"), SelectedPhotoalbum.Albumname);
                    var dialogRetry = new MessageDialogMessage(errorMsgRetry, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialogRetry.ShowDialog(0, 1);

                    if (dialogRetry.Result == 0)
                        await getHttpUpdatePhotoalbum.UpdatePhotoalbumAsync(SelectedPhotoalbum.Albumname, SelectedPhotoalbum.AlbumnameNew);
                    break;
                case GetFriendicaPhotos.PhotoErrors.NoServerSupport:
                case GetFriendicaPhotos.PhotoErrors.AuthenticationFailed:
                case GetFriendicaPhotos.PhotoErrors.AlbumNotAvailable:
                case GetFriendicaPhotos.PhotoErrors.NoAlbumnameSpecified:
                case GetFriendicaPhotos.PhotoErrors.UnknownError:
                default:
                    // message to user that we had an unexpected error
                    var errorMsgProblem = String.Format(loader.GetString("messageDialogPhotosAlbumUpdatingUnknownError"), SelectedPhotoalbum.Albumname);
                    var dialogError = new MessageDialogMessage(errorMsgProblem, "", "OK", null);
                    await dialogError.ShowDialog(0, 0);
                    break;
            }
            IsServerOperationPending = false;
            AlbumEditingEnabled = false;
            _isSavingChangedAlbumname = false;
        }


        // delete photoalbum button
        Mvvm.Command _deletePhotoalbumCommand;
        public Mvvm.Command DeletePhotoalbumCommand { get { return _deletePhotoalbumCommand ?? (_deletePhotoalbumCommand = new Mvvm.Command(ExecuteDeletePhotoalbum, CanDeletePhotoalbum)); } }
        private bool CanDeletePhotoalbum()
        {
            // don't allow user to delete the album if other server operations (uploads, deletes etc.) are currently running
            // this avoids that user wants to delete a meanwhile deleted album
            return (!IsServerOperationPending && SelectedPhotoalbum != null) ? true : false;
        }

        private async void ExecuteDeletePhotoalbum()
        {
            // ask user: "the album {0} will be deleted, no chance to get data back, are you sure -> YES | NO
            var errorMsg = String.Format(loader.GetString("messageDialogPhotosConfirmAlbumDelete"), SelectedPhotoalbum.Albumname);
            var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
            await dialog.ShowDialog(0, 1);

            if (dialog.Result == 1)
                return;  // cancel means aborting delete

            if (SelectedPhotoalbum.NewAlbumVisible)
            {
                // delete the new album locally
                var album = SelectedPhotoalbum;
                Albums.Remove(album);
            }
            else
            {
                // delete the album from server
                if (!NoSettings && !NoServerSupport)
                {
                    // delete selected album from server
                    IsServerOperationPending = true;
                    var getHttpDeletePhotoalbum = new GetFriendicaPhotos();
                    await getHttpDeletePhotoalbum.DeletePhotoalbumAsync(SelectedPhotoalbum.Albumname);

                    switch (getHttpDeletePhotoalbum.ErrorPhotoFriendica)
                    {
                        case GetFriendicaPhotos.PhotoErrors.OK:
                            // delete cached photos from device
                            DeletePhotosFromDevice(SelectedPhotoalbum);
                            // inform user about successful deletion;
                            errorMsg = String.Format(loader.GetString("messageDialogPhotosAlbumDeleted"), SelectedPhotoalbum.Albumname);
                            dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                            await dialog.ShowDialog(0, 0);
                            // remove album from Albums (after confirmation of user dialog, as string is used there)
                            Albums.Remove(SelectedPhotoalbum);
                            SelectedPhotoalbum = null;
                            break;
                        case GetFriendicaPhotos.PhotoErrors.ServerNotAnswered:
                            // ask for retry
                            var errorMsgRetry = String.Format(loader.GetString("messageDialogPhotosAlbumDeletingNoReaction"), SelectedPhotoalbum.Albumname);
                            var dialogRetry = new MessageDialogMessage(errorMsgRetry, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                            await dialogRetry.ShowDialog(0, 1);

                            if (dialogRetry.Result == 0)
                                await getHttpDeletePhotoalbum.DeletePhotoalbumAsync(SelectedPhotoalbum.Albumname);
                            break;
                        case GetFriendicaPhotos.PhotoErrors.NoAlbumnameSpecified:
                        case GetFriendicaPhotos.PhotoErrors.AlbumNotAvailable:
                        case GetFriendicaPhotos.PhotoErrors.UnknownError:
                            // message to user that we had an unexpected error
                            var errorMsgProblem = String.Format(loader.GetString("messageDialogPhotosAlbumDeletingUnknownError"), SelectedPhotoalbum.Albumname);
                            var dialogError = new MessageDialogMessage(errorMsgProblem, "", "OK", null);
                            await dialogError.ShowDialog(0, 0);
                            break;
                    }
                    IsServerOperationPending = false;
                }
                else
                {
                    // "this would now perform a server operation, in sample mode not possible"
                    errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                    dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);

                    // anyway, we can remove album from Albums here on app
                    Albums.Remove(SelectedPhotoalbum);
                    SelectedPhotoalbum = null;
                }
            }

            if (PhotosView == PhotosViewStates.OnlyPhotos)
                PhotosView = PhotosViewStates.OnlyAlbums;
        }

        #endregion


        #region Events

        // event used for handling photos
        public event EventHandler PrintButtonClicked;
        // event fired when user adds a new empty album to scroll listview to top
        public event EventHandler NewPhotoalbumAdded;
        // event fired when album has been renamed for reselecting it after new sorting
        public event EventHandler PhotoalbumUpdated;

        #endregion


        public PhotosViewmodel()
        {
            Albums = new ObservableCollection<FriendicaPhotoalbum>();

            // build ProfileimageCaptions with localized strings
            ProfileimagesCaption = new Dictionary<string, string>();
            ProfileimagesCaption.Add("en", "Profile Photos");
            ProfileimagesCaption.Add("de", "Profilbilder");
            ProfileimagesCaption.Add("es", "Foto del perfil");
            ProfileimagesCaption.Add("it", "Foto del profilo");
            ProfileimagesCaption.Add("pt", "Fotos do perfil");
            ProfileimagesCaption.Add("fr", "Photos du profil");

            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;
            SetPhotosView();

            // check if there is a setting for the server, otherwise we will use sample data for the user
            CheckServerSettings();
        }


        #region functions

        // set PhotosView which is used for triggering the different view kinds
        public void SetPhotosView()
        {
            if (App.Settings.OrientationDevice == OrientationDeviceFamily.DesktopLandscape ||
                App.Settings.OrientationDevice == OrientationDeviceFamily.MobileContinuum)
            {
                PhotosView = PhotosViewStates.Fullmode;
            }
            else
            {
                if (SelectedPhotoalbum != null)
                {
                    App.PhotosNavigatedIntoAlbum = true;
                    PhotosView = PhotosViewStates.OnlyPhotos;
                }
                else
                    PhotosView = PhotosViewStates.OnlyAlbums;
            }
        }


        // check if we have a defined server url or if we are in sample view mode
        public void CheckServerSettings()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null
                    || App.Settings.FriendicaUsername == null || App.Settings.FriendicaPassword == null 
                    || App.Settings.FriendicaUsername == "" || App.Settings.FriendicaPassword == "" )
            {
                NoSettings = true;
                PrepareSampleData();
            }
            else
            {
                NoSettings = false;
            }
        }


        // load sample data from separate class and save it in Observable Collection
        private void PrepareSampleData()
        {
            var sampleData = new FriendicaPhotoalbumSamples();
            Albums = sampleData.PhotoalbumSamples;
        }


        // check if we have a server already supporting the new required API calls for this module (Friendica > 3.5.2)
        private async Task CheckServerSupportAsync()
        {
            if (!NoSettings && !App.HasServerSupportChecked)
            {
                // call one of the new apis on the server, if response is positive we have a server we can work with, otherwise give hint to user
                IsCheckingServerSupport = true;
                var getHttpCheckServer = new GetFriendicaPhotos();
                await getHttpCheckServer.CheckServerSupportAsync();

                if (getHttpCheckServer.ErrorPhotoFriendica == GetFriendicaPhotos.PhotoErrors.NoServerSupport)
                    NoServerSupport = true;
                else if (getHttpCheckServer.ErrorPhotoFriendica == GetFriendicaPhotos.PhotoErrors.AuthenticationFailed)
                {
                    NoServerSupport = false;
                    // very unexpected case that server returns a Forbidden answer, message to user
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogHttpStatusForbidden"));
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                }
                else
                    NoServerSupport = false;

                // set indicator in App to true to avoid another check on reloading data
                App.HasServerSupportChecked = true;

                // delete samples if user has defined a server which is not supporting the API calls
                if (NoServerSupport && Albums.Count != 0)
                    Albums.Clear();

                IsCheckingServerSupport = false;
            }
        }


        // create a new album with a localized album name and a counter if name is already used on server
        private string GetNewAlbumname()
        {
            string newAlbumName = loader.GetString("textPhotosNewAlbumName");
            int i = 0;
            do
            {
                if (i > 0)
                    newAlbumName = loader.GetString("textPhotosNewAlbumname") + String.Format(" ({0})", i);
                i++;
            }
            while (Albums.Any(a => a.Albumname == newAlbumName));

            return newAlbumName;
        }


        // load photo list from server or sample data if not yet defined a server
        public async void LoadContentFromServer()
        {
            await CheckServerSupportAsync();

            if (!NoSettings && !NoServerSupport)
            {
                // load data from server
                IsLoadingPhotoalbums = true;
                var getHttpLoadPhotoalbums = new GetFriendicaPhotos();
                getHttpLoadPhotoalbums.FriendicaPhotosLoaded += GetHttpLoadPhotoalbums_FriendicaPhotosLoaded;
                getHttpLoadPhotoalbums.LoadPhotoalbums();
            }
            else if (NoSettings)
                PrepareSampleData();
        }

        private async void GetHttpLoadPhotoalbums_FriendicaPhotosLoaded(object sender, EventArgs e)
        {
            var getHttpLoadPhotoalbums = sender as GetFriendicaPhotos;

            // set back NoPhotosAvailable, if it was present
            NoPhotosAvailable = (NoPhotosAvailable) ? !NoPhotosAvailable : NoPhotosAvailable;
            
            switch (getHttpLoadPhotoalbums.ErrorPhotoFriendica)
            {
                case GetFriendicaPhotos.PhotoErrors.OK:
                    // we got a result from the server and now we are working with this list of photos, first clear current Albums list
                    if (Albums != null && Albums.Count != 0)
                        Albums.Clear();

                    // extract albumnames from list of photos from server
                    GetAlbumsFromPhotolist(getHttpLoadPhotoalbums.PhotolistReturned);

                    // sort photo information from photolist into Albums and sort photos by edited date descending
                    await GetPhotosFromPhotolistAsync(getHttpLoadPhotoalbums.PhotolistReturned);

                    // randomize images for the images stack of each album
                    GetPhotosForAlbumViewAsync();
                    break;
                case GetFriendicaPhotos.PhotoErrors.NoPhotosAvailable:
                    // photo/list has not returned any fotos - nothing yet on server
                    IsLoadingPhotoalbums = false;
                    NoPhotosAvailable = true;
                    break;
                case GetFriendicaPhotos.PhotoErrors.ServerNotAnswered:
                    // message to user: "the server has not answered on loading the photo list"
                    IsLoadingPhotoalbums = false;
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogPhotosServerNotAnswered"));
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadContentFromServer();
                    break;
                case GetFriendicaPhotos.PhotoErrors.NoServerSupport:
                case GetFriendicaPhotos.PhotoErrors.AuthenticationFailed:
                case GetFriendicaPhotos.PhotoErrors.UnknownError:
                default:
                    // message to user: "there was an error on loading the photo list"
                    IsLoadingPhotoalbums = false;
                    var errorDetail = (getHttpLoadPhotoalbums.ErrorMessageOnParsing != "") ? getHttpLoadPhotoalbums.ErrorMessageOnParsing : getHttpLoadPhotoalbums.ErrorMessage;
                    errorMsg = String.Format(loader.GetString("messageDialogPhotosErrorOnLoading"), errorDetail);
                    dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadContentFromServer();
                    break;
            }
        }


        // create list of albums from photolist returned by server
        private void GetAlbumsFromPhotolist(List<FriendicaPhotolist> photolist)
        {
            // select all albumnames and sort them ascending by alphabet
            var albums = photolist.GroupBy(photo => photo.PhotolistAlbum)
                            .Select(album => album.First())
                            .ToList();
            var albumsSorted = albums.OrderBy(album => album.PhotolistAlbum);

            // create Photoalbum elements from the sorted album list
            foreach (var album in albumsSorted)
            {
                var photoalbum = new FriendicaPhotoalbum();
                photoalbum.Albumname = album.PhotolistAlbum;
                Albums.Add(photoalbum);
            }
        }


        // sort albums by name and reorder list of albums
        private void SortAlbums()
        {
            var albumSorted = Albums.OrderBy(album => album.Albumname);
            int i = 0;
            foreach (var album in albumSorted)
            {
                var indexOld = Albums.IndexOf(album);
                Albums.Move(indexOld, i);
                i++;
            }
        }


        // sort photos in an album by date descending and reorder list of photos
        private void SortPhotos(FriendicaPhotoalbum album)
        { 
            var photosSorted = album.PhotosInAlbum.OrderByDescending(photo => photo.PhotoEditedDateTime);
            int i = 0;
            foreach (var photo in photosSorted)
            {
                var indexOld = album.PhotosInAlbum.IndexOf(photo);
                album.PhotosInAlbum.Move(indexOld, i);
                i++;
            }
        }


        // sort photos from photolist of server into created albums
        private async Task GetPhotosFromPhotolistAsync(List<FriendicaPhotolist> photolist)
        {
            foreach (var photo in photolist)
            {
                var photoClass = new FriendicaPhotoExtended();
                photoClass.CheckLoadingStatusRequested += PhotoClass_CheckLoadingStatusRequested;
                photoClass.Photo.PhotoId = photo.PhotolistId;
                photoClass.Photo.PhotoFilename = photo.PhotolistFilename;
                photoClass.Photo.PhotoType = photo.PhotolistType;
                photoClass.Photo.PhotoAlbum = photo.PhotolistAlbum;
                photoClass.Photo.PhotoCreated = photo.PhotolistCreated;
                photoClass.Photo.PhotoEdited = photo.PhotolistEdited;
                photoClass.Photo.PhotoDesc = photo.PhotolistDesc;
                photoClass.Photo = photoClass.Photo;
                photoClass.PhotolistThumb = photo.PhotolistThumb;

                photoClass.ThumbSizeData = new BitmapImage(new Uri(photoClass.PhotolistThumb, UriKind.RelativeOrAbsolute));
                photoClass.MediumSizeData = photoClass.ThumbSizeData;
                photoClass.FullSizeData = photoClass.ThumbSizeData;

                // trigger loading of the remaining data (no await needed to speed up parallel loadings)
                photoClass.GetPhotoFromServer();

                // add photoclass to the album named in photo
                var album = Albums.Single(alb => alb.Albumname == photo.PhotolistAlbum);
                album.PhotosInAlbum.Add(photoClass);
            }

            // sort photos descending by edited date
            foreach (var album in Albums)
                SortPhotos(album);
        }

        private void PhotoClass_CheckLoadingStatusRequested(object sender, EventArgs e)
        {
            CheckLoadingStatus();
        }


        // perform a check if now all photos have been loaded or all uploads/updates have been done to remove red indicator for user
        public void CheckLoadingStatus()
        {
            // activities on photos etc. can request this function to check if there are still loading photos or not-uploaded photos
            int countInitialLoading = 0;
            foreach (var album in Albums)
                countInitialLoading += album.PhotosInAlbum.Count(p => p.IsLoadingThumbSize || p.IsLoadingMediumSize || p.IsLoadingFullSize);

            IsLoadingPhotoalbums = (countInitialLoading > 0);
            if (IsLoadingPhotoalbums)
                return;

            // check if there are still open uploads or updates
            int countWorkingItems = 0;
            foreach (var album in Albums)
                countWorkingItems += album.PhotosInAlbum.Count(p => p.NewUploadPlanned || p.UpdatePlanned);

            HasUnsavedContent = (countWorkingItems > 0);
        }


        // remove all new but not performed uploads or updates (called when user navigates away and wants to discard changes)
        public void ResetAllChanges()
        {
            foreach (var album in Albums)
            {
                foreach (var photo in album.PhotosInAlbum)
                {
                    if (photo.NewUploadPlanned || photo.UpdatePlanned)
                        photo.ResetChangedData();
                }
            }
        }


        // select the first unsaved element so user can start the upload/update
        public void SetFirstUnsavedElement()
        {
            foreach (var album in Albums)
            {
                try
                {
                    var photo = album.PhotosInAlbum.First(p => p.NewUploadPlanned || p.UpdatePlanned);
                    SelectedPhotoalbum = album;
                    SelectedPhotoalbum.SelectedPhoto = photo;
                    return;
                }
                catch { continue; }
            }
        }


        // select up to three new photos from album randomly for viewing in stack representing the album
        private void GetPhotosForAlbumViewAsync()
        {
            foreach (var album in Albums)
            {
                album.SetPhotosForAlbumView();
            }
        }


        // create list of selectable albums for moving a photo away from the current album
        private void SetSelectablePhotoalbums()
        {
            // get all albums from list without currently selected one and order ascending
            var albums = Albums.Where(a => a.Albumname != SelectedPhotoalbum.Albumname).OrderBy(a => a.Albumname);

            var albumList = new List<string>();
            foreach (var album in albums)
            {
                // test if albumname is existing in list of translations for "profile photos"
                var lang = ProfileimagesCaption.Where(l => l.Value == album.Albumname).ToList();
                // in this case we ignore this album for the list of selectable target albums
                if (lang.Count > 0)
                    continue;
                albumList.Add(album.Albumname);
            }

            // add "New Album" (localized) as first list element
            albumList.Insert(0, loader.GetString("stringPhotoNewAlbumForMoving"));

            // insert this list into a property in the selected album from where we can fill each of the containing photo
            SelectedPhotoalbum.SelectableAlbums = albumList;
        }


        // user wants to upload a profile photo from an existing photo
        private void DuplicatePhotoForProfileImage(FriendicaPhotoExtended original)
        {
            var photo = new FriendicaPhotoExtended();
            photo.Photo = original.Photo;
            photo.NewPhotoUploaded += Photo_NewPhotoUploaded;
            // transfer more information if we want to crop a sample photo
            if (NoSettings)
            {
                photo.PhotolistThumb = original.PhotolistThumb;
                photo.FullSizeData = original.FullSizeData;
                photo.FullSizeLoaded = original.FullSizeLoaded;
                photo.MediumSizeData = original.MediumSizeData;
                photo.MediumSizeLoaded = original.MediumSizeLoaded;
                photo.ThumbSizeData = original.ThumbSizeData;
                photo.ThumbSizeLoaded = original.ThumbSizeLoaded;
            }
            photo.NewUploadPlanned = true;

            // retrieve correct localized profileimages folder
            var localizedProfileimagesName = ProfileimagesCaption.Where(l => l.Key == CultureInfo.CurrentCulture.TwoLetterISOLanguageName).ToList();
            string albumProfileimages = "Profile Photos";
            if (localizedProfileimagesName.Count == 1)
                albumProfileimages = localizedProfileimagesName[0].Value;
            photo.PhotoCategory = FriendicaPhotoExtended.PhotoCategories.FriendicaProfileImage;
            photo.Photo.PhotoAlbum = albumProfileimages;

            var profileimagesAlbum = Albums.SingleOrDefault(a => a.Albumname == albumProfileimages);
            if (profileimagesAlbum == null)
            {
                profileimagesAlbum = new FriendicaPhotoalbum() { Albumname = albumProfileimages, NewAlbumVisible = true };
                Albums.Insert(0, profileimagesAlbum);
            }
            profileimagesAlbum.PhotosInAlbum.Add(photo);
            SelectedPhotoalbum = profileimagesAlbum;
            // change view to the newly updated photo
            SelectedPhotoalbum.SelectedPhoto = photo;
        }


        // preparing for the upload
        private async Task PreparePhotoFileForUploadAsync(StorageFile image, string imageType)
        {
            var photo = new FriendicaPhotoExtended();
            // add selection for albums if user wants to change album afterwards
            photo.SelectableAlbums = new ObservableCollection<string>();
            foreach (var album in SelectedPhotoalbum.SelectableAlbums)
                photo.SelectableAlbums.Add(album);

            photo.NewPhotoStorageFile = image;
            photo.NewUploadPlanned = true;

            if (imageType.ToLower() == "photo")
            {
                photo.PhotoCategory = FriendicaPhotoExtended.PhotoCategories.FriendicaPhoto;
                await photo.PrepareNewPhotoFromDeviceAsync(image, SelectedPhotoalbum.Albumname);
                SelectedPhotoalbum.PhotosInAlbum.Add(photo);
            }
            else if (imageType.ToLower() == "profileimage")
            {
                // retrieve correct localized profileimages folder
                var localizedProfileimagesName = ProfileimagesCaption.Where(l => l.Key == CultureInfo.CurrentCulture.TwoLetterISOLanguageName).ToList();
                string albumProfileimages = "Profile Photos";
                if (localizedProfileimagesName.Count == 1)
                    albumProfileimages = localizedProfileimagesName[0].Value;
                photo.PhotoCategory = FriendicaPhotoExtended.PhotoCategories.FriendicaProfileImage;
                await photo.PrepareNewPhotoFromDeviceAsync(image, albumProfileimages);
                var profileimagesAlbum = Albums.SingleOrDefault(a => a.Albumname == albumProfileimages);
                if (profileimagesAlbum == null)
                {
                    profileimagesAlbum = new FriendicaPhotoalbum() { Albumname = albumProfileimages, NewAlbumVisible = true };
                    Albums.Insert(0, profileimagesAlbum);
                }
                profileimagesAlbum.PhotosInAlbum.Add(photo);

                SelectedPhotoalbum = profileimagesAlbum;
            }

            // change view to the newly updated photo
            SelectedPhotoalbum.SelectedPhoto = photo;
        }


        // delete all photos with all scales of a specified album from device
        private void DeletePhotosFromDevice(FriendicaPhotoalbum album)
        {
            if (album != null && album.PhotosInAlbum != null)
            {
                foreach (var photo in album.PhotosInAlbum)
                {
                    photo.DeleteAllScalesFromDevice();
                }
            }
        }

        #endregion


        #region eventhandlers

        private async void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth")
            {
                OnPropertyChanged("ListViewWidth");
                SetPhotosView();
            }

            // reload indicators if user has changed the settings for the server
            if (e.PropertyName == "FriendicaServer" || e.PropertyName == "FriendicaUsername" || e.PropertyName == "FriendicaPassword")
            {
                CheckServerSettings();
                await CheckServerSupportAsync();
                LoadContentFromServer();
            }
        }


        private void SelectedPhotoalbum_CheckLoadingStatusRequested(object sender, EventArgs e)
        {
            CheckLoadingStatus();
        }


        private async void SelectedPhotoalbum_MovePhotoToAlbumRequested(object sender, EventArgs e)
        {
            var photo = sender as FriendicaPhotoExtended;
            var oldAlbumname = photo.Photo.PhotoAlbum;

            // string "New album"
            if (photo.NewAlbumName == loader.GetString("stringPhotoNewAlbumForMoving"))
            {
                // create a new album and add it into the list
                ExecuteAddPhotoalbum();
                photo.NewAlbumName = SelectedPhotoalbum.Albumname;
            }

            // if we are im sample mode we cannot execute something on server
            if (NoSettings)
            {
                // simulate what server operation would normally do
                photo.Photo.PhotoAlbum = photo.NewAlbumName;
            }
            else
            {
                // upload image to server, cancel if upload produces an error
                var result = await photo.UpdatePhotoOnServerAsync();
                if (result != GetFriendicaPhotos.PhotoErrors.OK)
                    return;
            }

            // insert photo in newly selected album
            SelectedPhotoalbum = Albums.Single(a => a.Albumname == photo.Photo.PhotoAlbum);
            SelectedPhotoalbum.PhotosInAlbum.Add(photo);
            SetSelectablePhotoalbums();
            SelectedPhotoalbum.SelectedPhoto = photo;

            // change identifier for a new album if necessary
            if (SelectedPhotoalbum.NewAlbumVisible)
                SelectedPhotoalbum.NewAlbumVisible = false;

            // recreate preview stack
            SelectedPhotoalbum.SetPhotosForAlbumView();

            // remove from old album
            var oldAlbum = Albums.Single(a => a.Albumname == oldAlbumname);
            oldAlbum.PhotosInAlbum.Remove(photo);
            // remove album from list if there is no more photo left
            if (oldAlbum.PhotosInAlbum.Count == 0)
                Albums.Remove(oldAlbum);
            oldAlbum.SetPhotosForAlbumView();
        }


        private void SelectedPhotoalbum_NewProfileimageRequested(object sender, EventArgs e)
        {
            var photo = sender as FriendicaPhotoExtended;
            DuplicatePhotoForProfileImage(photo);
            // navigate to cropper
            var frame = App.GetFrameForNavigation();
            frame.Navigate(typeof(A7_PhotosCropping));
        }


        private void SelectedPhotoalbum_LastPhotoDeleted(object sender, EventArgs e)
        {
            var album = sender as FriendicaPhotoalbum;
            Albums.Remove(album);

            if (PhotosView == PhotosViewStates.OnlyPhotos)
                PhotosView = PhotosViewStates.OnlyAlbums;
        }


        private void SelectedPhotoalbum_PrintButtonClicked(object sender, EventArgs e)
        {
            PrintButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        #endregion

    }
}