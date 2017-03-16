using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Media.Capture;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;


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
                _selectedPhotoalbum = value;
                if (value == null)
                    IsSelectedPhotoalbum = false;
                else
                {
                    IsSelectedPhotoalbum = true;
                    SelectedPhotoalbum.PrintButtonClicked += SelectedPhotoalbum_PrintButtonClicked;
                }
                EditPhotoalbumCommand.RaiseCanExecuteChanged();
                DeletePhotoalbumCommand.RaiseCanExecuteChanged();
                AddFromDeviceCommand.RaiseCanExecuteChanged();
                AddFromCameraCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("SelectedPhotoalbum");
            }
        }

        private void SelectedPhotoalbum_PrintButtonClicked(object sender, EventArgs e)
        {
            PrintButtonClicked?.Invoke(this, EventArgs.Empty);
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
            return (!NoSettings && !NoServerSupport);
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
            // only active if user has selected an album (for profileimage always true as album there will be profileimages)
            if (imageType.ToLower() == "photo" && SelectedPhotoalbum == null)
                return false;
            else
                return true;
        }

        private async void ExecuteAddFromDevice(string imageType)
        {
            // TODO: if new album -> change indicator NewAlbumVisible to false and add photo to preview stack
            // TODO: if profilimage adding, change to the album after uploading, provide rectangle selector before

            // show dialog to open one or more images
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            var fileList = await openPicker.PickMultipleFilesAsync();

            // operate on picked images
            IsServerOperationPending = true;

            foreach (var image in fileList)
            {
                var photo = new FriendicaPhotoExtended();
                photo.NewPhotoUploaded += Photo_NewPhotoUploaded;
                photo.NewPhotoStorageFile = image;

                if (imageType.ToLower() == "photo")
                {
                    await photo.PrepareNewPhotoFromDeviceAsync(image, SelectedPhotoalbum.Albumname);
                    SelectedPhotoalbum.PhotosInAlbum.Add(photo);
                }
                else if (imageType.ToLower() == "profileimage")
                {
                    // TODO: retrieve correct localized profileimages folder
                    await photo.PrepareNewPhotoFromDeviceAsync(image, "Profilbilder");
                    var profileimagesAlbum = Albums.SingleOrDefault(a => a.Albumname == "Profilbilder");
                    if (profileimagesAlbum == null)
                    {
                        profileimagesAlbum = new FriendicaPhotoalbum() { Albumname = "Profilbilder", NewAlbumVisible = true };
                        Albums.Insert(0, profileimagesAlbum);
                    }
                    profileimagesAlbum.PhotosInAlbum.Add(photo);
                }

                // await here for each photo, so we can set IsServerOperationPending after the last uploaded picture
                await photo.UploadNewPhotoToServerAsync();
            }
            IsServerOperationPending = false;
        }

        private void Photo_NewPhotoUploaded(object sender, EventArgs e)
        {
            // set selected photo to the newly uploaded photo
            var photo = sender as FriendicaPhotoExtended;
            SelectedPhotoalbum.SelectedPhoto = photo;
        }


        // add photo from Camera button
        Mvvm.Command<string> _addFromCameraCommand;
        public Mvvm.Command<string> AddFromCameraCommand { get { return _addFromCameraCommand ?? (_addFromCameraCommand = new Mvvm.Command<string>(ExecuteAddFromCamera, CanAddFromCamera)); } }
        private bool CanAddFromCamera(string imageType)
        {
            // check if an album is selected when photo (profileimage has always the same album profilimages)
            if (imageType.ToLower() == "photo" && SelectedPhotoalbum == null)
                return false;
            else
                return App.DeviceHasCamera;

            // TODO: Prüfung ob Gerät überhaupt eine Kamera hat (muss noch um Erlaubnis-Prüfung ergänzt werden).
        }

        private async void ExecuteAddFromCamera(string imageType)
        {
            // TODO: if new album -> change indicator NewAlbumVisible to false and add photo to preview stack
            // TODO: if profilimage adding, change to the album after uploading, provide rectangle selector before

            // show dialog to shoot photo with camera
            var cameraCapture = new CameraCaptureUI();
            cameraCapture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            var file = await cameraCapture.CaptureFileAsync(CameraCaptureUIMode.Photo);

            // operate on returned image
            if (file != null)
            {
                IsServerOperationPending = true;

                var photo = new FriendicaPhotoExtended();
                photo.NewPhotoUploaded += Photo_NewPhotoUploaded;
                photo.NewPhotoStorageFile = file;

                if (imageType.ToLower() == "photo")
                {
                    await photo.PrepareNewPhotoFromDeviceAsync(file, SelectedPhotoalbum.Albumname);
                    SelectedPhotoalbum.PhotosInAlbum.Add(photo);
                }
                else if (imageType.ToLower() == "profileimage")
                {
                    // TODO: retrieve correct localized profileimages folder
                    await photo.PrepareNewPhotoFromDeviceAsync(file, "Profilbilder");
                    var profileimagesAlbum = Albums.SingleOrDefault(a => a.Albumname == "Profilbilder");
                    if (profileimagesAlbum == null)
                    {
                        profileimagesAlbum = new FriendicaPhotoalbum() { Albumname = "Profilbilder", NewAlbumVisible = true };
                        Albums.Insert(0, profileimagesAlbum);
                    }
                    profileimagesAlbum.PhotosInAlbum.Add(photo);
                }

                // await here for finishing the upload photo, so we can set IsServerOperationPending after upload
                await photo.UploadNewPhotoToServerAsync();

                IsServerOperationPending = false;
            }


            string errorMsg = "Fehler!";
            if (imageType.ToLower() == "photo")
                errorMsg = "Damit wird ein neues Foto mit der Kamera erstellt.";
            else if (imageType.ToLower() == "profileimage")
                errorMsg = "Damit wird ein neues Profilbild mit der Kamera erstellt.";

            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }


        // add empty ink canvas button
        Mvvm.Command _addEmptyInkCanvasCommand;
        public Mvvm.Command AddEmptyInkCanvasCommand { get { return _addEmptyInkCanvasCommand ?? (_addEmptyInkCanvasCommand = new Mvvm.Command(ExecuteAddEmptyInkCanvas, CanAddEmptyInkCanvas)); } }
        private bool CanAddEmptyInkCanvas()
        {
            // TODO: prüfung ob überhaupt ein Album ausgewählt wurde
            return true;
        }

        private async void ExecuteAddEmptyInkCanvas()
        {
            // TODO implement correct function for adding a new empty ink canvas (navigate to InkCanvas view but without providing a background image)

            string errorMsg = "Damit wird ein leerer InkCanvas-Arbeitsbereich geöffnet. Der Anwender kann dort kreativ werden.";
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
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
            }

            if (PhotosView == PhotosViewStates.OnlyPhotos)
                PhotosView = PhotosViewStates.OnlyAlbums;
        }

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
            _saveAlbums = new List<FriendicaPhotoalbum>(); // TODO: brauchen wir das?

            // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
            App.Settings.PropertyChanged += Settings_PropertyChanged;
            SetPhotosView();

            // check if there is a setting for the server, otherwise we will use sample data for the user
            CheckServerSettings();
                      

            // TODO: remove the following old code
            SearchConversations = new ObservableCollection<FriendicaConversation>();
            RetrievedMessages = new List<FriendicaMessage>();
            IsSendingNewMessage = App.IsSendingNewMessage;
            App.ContactsLoaded += App_ContactsLoaded;
            App.SendingNewMessageChanged += App_SendingNewMessageChanged;
            _updateConversationsEvent += MessagesViewmodel__updateConversationsEvent;
        }


        #region functions

        // set PhotosView which is used for triggering the different view kinds
        private void SetPhotosView()
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


        private void PrepareSampleData()
        {
            // load sample data from separate class and save it in Observable Collection
            var sampleData = new FriendicaPhotoalbumSamples();
            Albums = sampleData.PhotoalbumSamples;

            // TODO: brauchen wir das?
            foreach (var album in sampleData.PhotoalbumSamples)
                _saveAlbums.Add(album);
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


        private string GetNewAlbumname()
        {
            string newAlbumName = loader.GetString("textPhotosNewAlbumName");
            int i = 0;
            do
            {
                if (i > 0)
                    newAlbumName = loader.GetString("textPhotosNewAlbumname") + String.Format(" ({0})", i);
                var test = Albums.Any(a => a.Albumname == newAlbumName);
                i++;
            }
            while (Albums.Any(a => a.Albumname == newAlbumName));

            return newAlbumName;
        }


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

                    // sort photo information from photolist into Albums
                    await GetPhotosFromPhotolistAsync(getHttpLoadPhotoalbums.PhotolistReturned);

                    // randomize images for the images stack of each album
                    await GetPhotosForAlbumViewAsync();

                    // sortiere nach jüngstem Bilddatum; scheiße: wir kennen das datum der Bilder ja jetzt noch nicht
                    // Sortieren nach jüngstem Bilddatum machen wir also erst wenn wir alle Bilder geladen haben
                    // alle Bilder jedesmal laden ist sicher aufwendig, macht also nur sinn wenn wir cachen dürfen (dann aber auch die Photo-Infos cachen)
                    // Test ob das nachträgliche Sortieren funktioniert

                    // TODO nächsten Schritt mit weiteren Codeschnippseln weiterschieben
                    IsLoadingPhotoalbums = false;
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
                    errorMsg = String.Format(loader.GetString("messageDialogPhotosErrorOnLoading"), getHttpLoadPhotoalbums.ErrorMessage);
                    dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadContentFromServer();
                    break;
            }
        }


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

        private async Task GetPhotosFromPhotolistAsync(List<FriendicaPhotolist> photolist)
        {
            foreach (var photo in photolist)
            {
                var photoClass = new FriendicaPhotoExtended();
                photoClass.Photo.PhotoId = photo.PhotolistId;
                photoClass.Photo.PhotoFilename = photo.PhotolistFilename;
                photoClass.Photo.PhotoType = photo.PhotolistType;
                photoClass.Photo.PhotoAlbum = photo.PhotolistAlbum;
                photoClass.PhotolistThumb = photo.PhotolistThumb;

                // TODO: Probleme wenn Image.Source auf 192.168.1.3 zeigt, daher für diesen Test ersetzen
                photoClass.PhotolistThumb = photo.PhotolistThumb.Replace("192.168.1.3", "mozartweg.dyndns.org");

                photoClass.ThumbSizeData = new BitmapImage(new Uri(photoClass.PhotolistThumb, UriKind.RelativeOrAbsolute));
                photoClass.MediumSizeData = photoClass.ThumbSizeData;
                photoClass.FullSizeData = photoClass.ThumbSizeData;

                // trigger loading of the remaining data (no await needed to speed up parallel loadings)
                photoClass.GetPhotoFromServer();

                // add photoclass to the album named in photo
                var album = Albums.Single(alb => alb.Albumname == photo.PhotolistAlbum);
                album.PhotosInAlbum.Add(photoClass);
            }
        }
        
        private async Task GetPhotosForAlbumViewAsync()
        {
            foreach (var album in Albums)
            {
                // TODO: we have still an issue with private images (forbidden sign instead of image)
                // TODO: take cached image if available or load photo from server with authentication 

                // fallback if collection is null, which should not occur
                if (album.PhotosInAlbum == null)
                    return;

                album.Photo1Visible = false;
                album.Photo2Visible = false;
                album.Photo3Visible = false;

                if (album.PhotosInAlbum.Count > 0)
                {
                    album.Photo1Visible = true;
                    album.StackPhoto1 = album.PhotosInAlbum[0];
                }
                if (album.PhotosInAlbum.Count > 1)
                {
                    album.Photo2Visible = true;
                    album.StackPhoto2 = album.PhotosInAlbum[1];
                }
                if (album.PhotosInAlbum.Count > 2)
                {
                    // if we have 3 or more sort list of photos randomly and take first three entries 
                    // random does not make sense with only 3 but code can be reused
                    album.Photo3Visible = true;
                    var randomPhotos = RandomImageSelector(album.PhotosInAlbum);
                    album.StackPhoto1 = randomPhotos[0];
                    album.StackPhoto2 = randomPhotos[1];
                    album.StackPhoto3 = randomPhotos[2];
                }
            }
        }

        private List<FriendicaPhotoExtended> RandomImageSelector(ObservableCollection<FriendicaPhotoExtended> photosInAlbum)
        {
            var list = new List<FriendicaPhotoExtended>();

            return list = (from photo in photosInAlbum
                       orderby Guid.NewGuid()
                       select photo).ToList();
        }

        #endregion





















        // TODO: bis hierher bereits überarbeitet





        // contains the albums from the server
        private ObservableCollection<FriendicaConversation> _conversations;
        public ObservableCollection<FriendicaConversation> Conversations
        {
            get { return _conversations; }
            set
            {
                _conversations = value;
                OnPropertyChanged("Conversations");
            }
        }

        // contains the returned messages from server as a list (to have original returns from server stored)
        private List<FriendicaMessage> _retrievedMessages;
        public List<FriendicaMessage> RetrievedMessages
        {
            get { return _retrievedMessages; }
            set { _retrievedMessages = value; }
        }


        // following used to save conversations on navigating from and restoring on navigating to messages.xaml
        private List<FriendicaPhotoalbum> _saveAlbums;



        

        // selected conversation for updating on server
        private List<FriendicaConversation> _conversationsForUpdating = new List<FriendicaConversation>();
        private FriendicaConversation _conversationUpdating;


        // indicator if no conversations are available
        private bool _noMessagesAvailable;
        public bool NoMessagesAvailable
        {
            get { return _noMessagesAvailable; }
            set { _noMessagesAvailable = value;
                OnPropertyChanged("NoMessagesAvailable"); }
        }

        // indicator if searchmode is enabled (then display input box and search results instead of conversation list)
        private bool _isSearchModeEnabled;
        public bool IsSearchModeEnabled
        {
            get { return _isSearchModeEnabled; }
            set { _isSearchModeEnabled = value;
                if (!value)
                {
                    SearchConversations = new ObservableCollection<FriendicaConversation>();
                    SearchResults = new ObservableCollection<FriendicaMessage>();
                    SelectedPhotoalbum = null;
                }
                OnPropertyChanged("IsSearchModeEnabled");
            }
        }


        // search string
        private string _searchString;
        public string SearchString
        {
            get { return _searchString; }
            set { _searchString = value;
                OnPropertyChanged("SearchString"); }
        }

        // contains the search results from the server
        private ObservableCollection<FriendicaMessage> _searchResults;
        public ObservableCollection<FriendicaMessage> SearchResults
        {
            get { return _searchResults; }
            set { _searchResults = value;
                SearchResults.CollectionChanged += SearchResults_CollectionChanged;
                OnPropertyChanged("SearchResults"); }
        }

        private void SearchResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (FriendicaMessage message in e.NewItems)
                SearchResults.Single(m => m.MessageId == message.MessageId).ConvertHtmlToParagraph();
        }

        // contains the conversations from the server
        private ObservableCollection<FriendicaConversation> _searchConversations;
        public ObservableCollection<FriendicaConversation> SearchConversations
        {
            get { return _searchConversations; }
            set
            {
                _searchConversations = value;
                OnPropertyChanged("SearchConversations");
            }
        }

        // indicator if no search results are returned
        private bool _noSearchResults;
        public bool NoSearchResults
        {
            get { return _noSearchResults; }
            set { _noSearchResults = value;
                OnPropertyChanged("NoSearchResults"); }
        }


        // indicator on top for loading older conversations
        private bool _isLoadingOlderMessages;
        public bool IsLoadingOlderMessages
        {
            get { return _isLoadingOlderMessages; }
            set { _isLoadingOlderMessages = value;
                OnPropertyChanged("IsLoadingOlderMessages"); }
        }

        // indicator that all messages have been loaded from server
        private bool _allMessagesLoaded;
        public bool AllMessagesLoaded
        {
            get { return _allMessagesLoaded; }
            set { _allMessagesLoaded = value;
                OnPropertyChanged("AllMessagesLoaded"); }
        }

        // set Property if refresh is currently in progress (showing progress bar and preventing from clicking Refresh again)
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }

        // indicator showing that app is querying server for the searchstring
        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set { _isSearching = value;
                OnPropertyChanged("IsSearching"); }
        }

        // indicator for showing the editor for new messages
        private bool _isEditorEnabled;
        public bool IsEditorEnabled
        {
            get { return _isEditorEnabled; }
            set { _isEditorEnabled = value;
                OnPropertyChanged("IsEditorEnabled"); }
        }

        // indicator for the editor mode
        private bool _isEditorInFullscreenMode;
        public bool IsEditorInFullscreenMode
        {
            get { return _isEditorInFullscreenMode; }
            set
            {
                _isEditorInFullscreenMode = value;
                OnPropertyChanged("IsEditorInFullscreenMode");
            }
        }

        // indicator for starting a new conversation (for add button)
        private bool _isStartingNewConversation;
        public bool IsStartingNewConversation
        {
            get { return _isStartingNewConversation; }
            set { _isStartingNewConversation = value;
                OnPropertyChanged("IsStartingNewConversation"); }
        }

        // list of possible contacts for selecting one
        public ObservableCollection<FriendicaUserExtended> Contacts
        {
            get { return App.ContactsFriends; }
        }

        // save selected contact
        private FriendicaUserExtended _selectedContact;
        public FriendicaUserExtended SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                SetNavigationStatus();
                OnPropertyChanged("SelectedContact");
            }
        }

        // string with title of new conversation
        private string _newMessageTitle;
        public string NewMessageTitle
        {
            get { return _newMessageTitle; }
            set { _newMessageTitle = value;
                SetNavigationStatus();
                OnPropertyChanged("NewMessageTitle"); }
        }

        // string with message content
        private string _newMessageContent;
        public string NewMessageContent
        {
            get { return _newMessageContent; }
            set { _newMessageContent = value;
                SetNavigationStatus();  }
        }

        // indicator showing that app is sending a new message to server
        private bool _isSendingNewMessage;
        public bool IsSendingNewMessage
        {
            get { return _isSendingNewMessage; }
            set { _isSendingNewMessage = value;
                OnPropertyChanged("IsSendingNewMessage"); }
        }

        // indicator that user came from clicking on a toast notification
        private bool _isNavigationFromToast;
        public bool IsNavigationFromToast
        {
            get { return _isNavigationFromToast; }
            set { _isNavigationFromToast = value; }
        }

        // saving conversation parenturi from toast notification
        private string _toastConversationUri;
        public string ToastConversationUri
        {
            get { return _toastConversationUri; }
            set { _toastConversationUri = value; }
        }

        // event handlers for 
        //      starting a new conversation
        public event EventHandler ButtonAddConversationClicked;
        //      enabling search mode
        public event EventHandler ButtonEnableSearchClicked;
        //      updating messages of a conversation (only used in this class)
        private event EventHandler _updateConversationsEvent;
        //      fired when a new conversation has been started to update the listview selection
        public event EventHandler NewMessageAdded;
        //      fired when we want to clear out the richeditboxes
        public event EventHandler ClearOutRicheditboxesRequested;
        //      fired when user clicks the enable editor button to scroll down to bottom
        public event EventHandler GoToBottomListViewRequested;



        
        
        

        // toggle button - enabling editor
        Mvvm.Command _enableEditorCommand;
        public Mvvm.Command EnableEditorCommand { get { return _enableEditorCommand ?? (_enableEditorCommand = new Mvvm.Command(ExecuteEnableEditor, CanEnableEditor)); } }
        private bool CanEnableEditor()
        {
            if (IsSendingNewMessage)
                return false;
            else
                return true;
        }

        private void ExecuteEnableEditor()
        {
            GoToBottomListViewRequested?.Invoke(this, EventArgs.Empty);
            //IsEditorEnabled = !IsEditorEnabled;
        }


        // send message command
        Mvvm.Command _sendMessageCommand;
        public Mvvm.Command SendMessageCommand { get { return _sendMessageCommand ?? (_sendMessageCommand = new Mvvm.Command(ExecuteSendMessage, CanSendMessage)); } }
        private bool CanSendMessage()
        {
            if (NewMessageContent != null && NewMessageContent != "\r\r")
            {
                if (IsStartingNewConversation && SelectedContact == null)
                    return false;
                else
                {
                    App.NavStatus = NavigationStatus.NewMessageChanged;
                    return true;
                }
            }
            else
                return false;
        }

        private async void ExecuteSendMessage()
        {
            // if we are displaying sample data we cannot send a message to the server
            if (NoSettings)
            {
                // message to user: "no sending possible in test mode"
                string errorMsg;
                errorMsg = loader.GetString("messageDialogMessagesNewNoSettings");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
            else
            {
                var message = CreateMessage();
                ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);

                IsSendingNewMessage = true;
                // sending new message to server
                var postMessage = new PostFriendicaMessage();
                postMessage.FriendicaMessageSent += PostMessage_FriendicaMessageSent;
                postMessage.PostFriendicaMessageNew(message);
            }
        }

        private async void PostMessage_FriendicaMessageSent(object sender, EventArgs e)
        {
            var postMessage = sender as PostFriendicaMessage;
            if (!postMessage.IsSuccessStateCode)
            {
                IsSendingNewMessage = false;
                // message to user: "there was an error in sending the message"
                string errorMsg;
                errorMsg = String.Format(loader.GetString("messageDialogMessagesNewErrorSending"), postMessage.ErrorMessage);
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
            else
            {
                IsSendingNewMessage = false;
                IsEditorEnabled = false;
                App.NavStatus = NavigationStatus.OK;
                ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);

                if (postMessage.NewMessage.NewMessageReplyTo != "")
                {
                    // we were answering an existing message - reload conversation messages
                    //SelectedConversation.NewMessageAdded += Conv_NewMessageAdded;
                    //SelectedConversation.ReloadConversation();
                }
                else
                {
                    // we were creating a new conversation - load new conversations and set selection
                    LoadMessagesNew();
                    IsStartingNewConversation = false;
                }
            }
        }


        //public PhotosViewmodel()
        //{
        //    Conversations = new ObservableCollection<FriendicaConversation>();
        //    _saveConversations = new List<FriendicaConversation>();
        //    SearchConversations = new ObservableCollection<FriendicaConversation>();
        //    RetrievedMessages = new List<FriendicaMessage>();

        //    // react on changes in App.Settings.BottomAppBarMargin (otherwise not recognized by XAML)
        //    App.Settings.PropertyChanged += Settings_PropertyChanged;
        //    SetPhotosView();
            
        //    // check if there is a setting for the server, otherwise we will use sample data for the user
        //    CheckServerSettings();
        //    // TODO: implement check if server is supporting (the initial load call /api/friendica/photos/list is on older machine present, we need to check newer calls like /api/friendica/photo/create
             

        //    IsSendingNewMessage = App.IsSendingNewMessage;
        //    App.ContactsLoaded += App_ContactsLoaded;
        //    App.SendingNewMessageChanged += App_SendingNewMessageChanged;
        //    _updateConversationsEvent += MessagesViewmodel__updateConversationsEvent;
        //}




        private void SetNavigationStatus()
        {
            if (NewMessageTitle != null || (NewMessageContent != null && NewMessageContent != "\r\r") || SelectedContact != null)
            {
                App.NavStatus = NavigationStatus.NewMessageChanged;
            }
            else
            {
                if (App.NavStatus == NavigationStatus.NewMessageChanged)
                    App.NavStatus = NavigationStatus.OK;
            }
            SendMessageCommand.RaiseCanExecuteChanged();
        }

        private FriendicaMessageNew CreateMessage()
        {
            var newMessage = new FriendicaMessageNew();
            //if (SelectedConversation != null)
            //{
            //    FriendicaUser user = null;
            //    if (SelectedConversation.NewestMessage.MessageSenderScreenName.ToLower() == App.Settings.FriendicaUsername.ToLower())
            //        user = SelectedConversation.NewestMessage.MessageRecipient;
            //    else if (SelectedConversation.NewestMessage.MessageRecipientScreenName.ToLower() == App.Settings.FriendicaUsername.ToLower())
            //        user = SelectedConversation.NewestMessage.MessageSender;
            //    newMessage.NewMessageUserUrl = user.UserUrl;

            //    newMessage.NewMessageReplyTo = SelectedConversation.NewestMessage.MessageId;
            //    newMessage.NewMessageTitle = SelectedConversation.Title;
            //}
            //else
            //{
            //    newMessage.NewMessageUserUrl = SelectedContact.User.UserUrl;
            //    newMessage.NewMessageTitle = NewMessageTitle;
            //}
            //newMessage.NewMessageText = NewMessageContent;
            return newMessage;
        }

        private async void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BottomAppBarMargin")
                OnPropertyChanged("BottomAppBarMargin");
            if (e.PropertyName == "ShellWidth")
            {
                OnPropertyChanged("ListViewWidth");
                SetPhotosView();
            }
            if (e.PropertyName == "OrientationDevice")
            {
                OnPropertyChanged("ListViewWidth");
            }

            // reload indicators if user has changed the settings for the server
            if (e.PropertyName == "FriendicaServer" || e.PropertyName == "FriendicaUsername" || e.PropertyName == "FriendicaPassword")
            {
                CheckServerSettings();
                await CheckServerSupportAsync();
                LoadContentFromServer();
            }
        }


        private void App_ContactsLoaded(object sender, EventArgs e)
        {
            OnPropertyChanged("Contacts");
        }


        private void App_SendingNewMessageChanged(object sender, EventArgs e)
        {
            IsSendingNewMessage = App.IsSendingNewPost;
        }


        public void SaveAlbums()
        {
            _saveAlbums = new List<FriendicaPhotoalbum>();
            foreach (var album in Albums)
                _saveAlbums.Add(album);
            Albums = new ObservableCollection<FriendicaPhotoalbum>();
        }


        public void RestoreAlbums()
        {
            Albums = new ObservableCollection<FriendicaPhotoalbum>();
            foreach (var album in _saveAlbums)
                Albums.Add(album);
        }




        //private void GetMessagesTestServer_FriendicaMessagesLoaded(object sender, EventArgs e)
        //{
        //    var getMessagesInitial = sender as GetFriendicaMessages;

        //    if (getMessagesInitial.StatusCode == HttpStatusCode.Ok && !getMessagesInitial.IsErrorOccurred)
        //    {
        //        LoadConte();
        //    }
        //    else
        //    {
        //        IsLoadingMessages = false;

        //        if (getMessagesInitial.StatusCode == HttpStatusCode.NotImplemented || getMessagesInitial.StatusCode == HttpStatusCode.NotFound)
        //        {
        //            // message to user: "server is not supporting private messages for the app - please update server to Friendica 3.5"
        //            NoServerSupport = true;
        //            //string errorMsg;
        //            //errorMsg = String.Format(loader.GetString("messageDialogMessagesNotImplemented"));
        //            //var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
        //            //await dialog.ShowDialog(0, 0);
        //        }
        //    }
        //}

        public async void UpdateStatusOnServer()
        {
            // save currently SelectedConversation for into queue of conversations for updating the messages
            //if (!_conversationsForUpdating.Contains(SelectedConversation))
              //  _conversationsForUpdating.Add(SelectedConversation);

            // give user time to see the new messages
            await Task.Delay(3000);

            // fire event which is working on the queue of conversations
            if (_updateConversationsEvent != null)
                _updateConversationsEvent.Invoke(this, EventArgs.Empty);
        }


        private void MessagesViewmodel__updateConversationsEvent(object sender, EventArgs e)
        {
            // load next conversation for updating
            var conv = _conversationsForUpdating.FirstOrDefault();
            if (conv != null)
            {
                // select the corresponding conversation in Conversations list for setting indicator
                _conversationUpdating = Conversations.Single(c => c.ConversationUri == conv.ConversationUri);
                _conversationUpdating.IsUpdatingServerStatus = true;

                // go through all new message for updating them on server
                var MessagesUpdate = _conversationUpdating.MessagesForUpdate();

                if (MessagesUpdate.Count == 0)
                    _conversationUpdating.IsUpdatingServerStatus = false;

                foreach (var message in MessagesUpdate)
                {
                    if (App.Settings.FriendicaServer != null)
                    {

                        var getMessages = new GetFriendicaMessages();
                        getMessages.RequestFinished += GetMessages_RequestFinished;
                        getMessages.SetSeenMessage(message.MessageId);

                    }
                    else
                    {
                        message.MessageSeen = "1";
                    }
                }

                if (App.Settings.FriendicaServer == null)
                {
                    _conversationUpdating.IsUpdatingServerStatus = false;
                    _conversationUpdating.CounterMessagesUnseen = _conversationUpdating.Messages.Count(m => m.MessageSeen == "0");
                }

                // we can now remove the conversation as we have started all updating processes
                _conversationsForUpdating.Remove(conv);
            }
        }


        private void GetMessages_RequestFinished(object sender, EventArgs e)
        {
            var getMessages = sender as GetFriendicaMessages;
            if (!getMessages.IsErrorOccurred)
            {
                foreach (var conv in Conversations)
                {
                    try
                    {
                        var message = conv.Messages.Single(m => m.MessageId == getMessages.MessageId);
                        message.MessageSeen = "1";
                        conv.CounterMessagesUnseen = conv.Messages.Count(m => m.MessageSeen == "0");
                    }
                    catch { }
                }

            }
            CheckMessagesUpdating();
        }




        private void Conv_NewMessageAdded(object sender, EventArgs e)
        {
            IsSendingNewMessage = false;
        }

        private void Conv_ConversationDeleted(object sender, EventArgs e)
        {
            var conv = sender as FriendicaConversation;
            //if (SelectedConversation != null && conv.ConversationUri == SelectedConversation.ConversationUri)
            //    SelectedConversation = null;
            // remove the messages from the retrievedMessages otherwise we have a problem with loading new messages
            RetrievedMessages.RemoveAll(m => m.MessageParentUri == conv.ConversationUri);
            Conversations.Remove(conv);
        }

        private void GetMessagesConv_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesConv = sender as GetFriendicaMessages;
            if (!getMessagesConv.IsErrorOccurred)
            {
                var conv = Conversations.Single(c => c.ConversationUri == getMessagesConv.ConversationUri);
                if (conv.Messages == null)
                    conv.Messages = new ObservableCollection<FriendicaMessage>();

                if (getMessagesConv.MessagesReturned == null)
                    return;

                var messages = getMessagesConv.MessagesReturned.OrderBy(m => m.MessageCreatedAtDateTime);
                foreach (var message in messages)
                {
                    conv.Messages.Add(message);
                }
                conv.IsLoaded = true;
                conv.IsLoading = false;
            }
            CheckConversationsLoading();
        }


        private void CheckConversationsLoading()
        {
            bool isStillLoading = false;
            foreach (var conv in Conversations)
                isStillLoading |= conv.IsLoading;

            //if (IsLoadingMessages)
            //    IsLoadingMessages = isStillLoading;
            //else if (IsRefreshing)
            //    IsRefreshing = isStillLoading;
            //else if (IsLoadingOlderMessages)
            //{
            //    IsLoadingOlderMessages = isStillLoading;
            //    LoadOlderMessagesCommand.RaiseCanExecuteChanged();
            //}

            if (!isStillLoading && IsNavigationFromToast)
            {
                //SelectedConversation = Conversations.SingleOrDefault(c => c.ConversationUri == ToastConversationUri);
            }
        }

        private void CheckMessagesUpdating()
        {
            // retrieve correct conversation
            var conv = Conversations.Single(c => c.ConversationUri == _conversationUpdating.ConversationUri);
            if (conv.IsUpdatingServerStatus)
            {
                // only false if all messages are seen
                bool isStillUpdating = false;
                foreach (var message in _conversationUpdating.Messages)
                    isStillUpdating |= (message.MessageSeen == "0" ? true : false);
                conv.IsUpdatingServerStatus = isStillUpdating;

                if (!isStillUpdating)
                {
                    // all messages have been seen - we can change the indicator in the conversation
                    conv.HasNewMessages = false;
                    _conversationUpdating = null;

                    // fire event if still open conversations for updating available
                    if (_conversationsForUpdating.Count != 0)
                    {
                        if (_updateConversationsEvent != null)
                            _updateConversationsEvent.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public void LoadMessagesNext()
        {
            if (!IsLoadingOlderMessages)
            {
                var getMessagesNext = new GetFriendicaMessages();
                getMessagesNext.FriendicaMessagesLoaded += GetMessagesNext_FriendicaMessagesLoaded;

                // reduce minimum ID by 1 to avoid retrieving the oldest post again
                //var oldestId = RetrievedMessages.Min(m => m.MessageIdInt) - 1;
                int oldestId;
                if (Conversations.Count == 0)
                    oldestId = 0;
                else
                    oldestId = Conversations.SelectMany(m => m.Messages).Min(m => m.MessageIdInt) - 1;
                // oldestId may not be negative or zero, otherwise API returns the newest posts again
                if (oldestId > 0)
                {
                    IsLoadingOlderMessages = true;
                    getMessagesNext.LoadMessagesNext(oldestId, 20);
                }
                else if (oldestId == 0)
                    AllMessagesLoaded = true;
            }
        }


        private async void GetMessagesNext_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesNext = sender as GetFriendicaMessages;

            if (getMessagesNext.StatusCode == HttpStatusCode.Ok && !getMessagesNext.IsErrorOccurred)
            {
                var convs = getMessagesNext.RetrieveConversations();
                foreach (var conv in convs)
                {
                    conv.IsLoading = true;
                    conv.ConversationDeleted += Conv_ConversationDeleted;
                    var getMessagesConv = new GetFriendicaMessages();
                    getMessagesConv.FriendicaMessagesLoaded += GetMessagesConv_FriendicaMessagesLoaded;
                    getMessagesConv.LoadConversation(conv.ConversationUri);
                    ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);
                    Conversations.Add(conv);
                }
            }
            else
            {
                if (getMessagesNext.ErrorMessageFriendica != GetFriendicaMessages.MessageErrors.NoMailsAvailable)
                {
                    // message to user: "there was an error on loading the private messages"
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogMessagesErrorLoading"), getMessagesNext.ErrorMessage);
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadMessagesNext();
                }
                else
                    AllMessagesLoaded = true;

                IsLoadingOlderMessages = false;
            }

        }


        public void LoadMessagesNew()
        {
            var getMessagesNew = new GetFriendicaMessages();
            getMessagesNew.FriendicaMessagesLoaded += GetMessagesNew_FriendicaMessagesLoaded;

            double newestId = 0;
            if (RetrievedMessages.Count != 0)
                newestId = Conversations.SelectMany(m => m.Messages).Max(m => m.MessageIdInt);
            IsRefreshing = true;
            getMessagesNew.LoadMessagesNew(newestId, 20);
        }


        private async void GetMessagesNew_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getMessagesNew = sender as GetFriendicaMessages;

            if (getMessagesNew.StatusCode == HttpStatusCode.Ok && !getMessagesNew.IsErrorOccurred)
            {
                // save loaded messages into variable
                foreach (var message in getMessagesNew.MessagesReturned)
                    RetrievedMessages.Insert(0, message);

                // set indicator back if we had no conversation in the list before
                if (NoMessagesAvailable)
                    NoMessagesAvailable = false;

                var convs = getMessagesNew.RetrieveConversations();
                foreach (var conv in convs)
                {
                    if (Conversations.Where(c => c.ConversationUri == conv.ConversationUri).Any())
                    {
                        var conversation = Conversations.Single(c => c.ConversationUri == conv.ConversationUri);
                        conversation.IsLoading = true;
                        conversation.NewMessageAdded += Conv_NewMessageAdded;
                        conversation.ReloadConversation();
                        //if (PhotosView != PhotosViewStates.OnlyAlbums)
                            //SelectedConversation = conversation;
                    }
                    else
                    {
                        conv.IsLoading = true;
                        conv.ConversationDeleted += Conv_ConversationDeleted;
                        var getMessagesConv = new GetFriendicaMessages();
                        getMessagesConv.FriendicaMessagesLoaded += GetMessagesConv_FriendicaMessagesLoaded;
                        getMessagesConv.LoadConversation(conv.ConversationUri);
                        Conversations.Insert(0, conv);
                        //if (PhotosView != PhotosViewStates.OnlyAlbums)
                            //SelectedConversation = conv;
                    }
                    ClearOutRicheditboxesRequested?.Invoke(this, EventArgs.Empty);
                    if (PhotosView == PhotosViewStates.OnlyPhotos)
                    {
                        App.MessagesNavigatedIntoConversation = true;
                    }
                    if (NewMessageAdded != null)
                        NewMessageAdded.Invoke(this, EventArgs.Empty);
                }
                IsRefreshing = false;
            }
            else
            {
                if (getMessagesNew.ErrorMessageFriendica != GetFriendicaMessages.MessageErrors.NoMailsAvailable)
                {
                    // message to user: "there was an error on loading the private messages"
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogMessagesErrorLoading"), getMessagesNew.ErrorMessage);
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadMessagesNew();
                }
                IsRefreshing = false;
            }

        }


        public void SearchMessages()
        {
            var searchMessages = new GetFriendicaMessages();
            searchMessages.FriendicaMessagesLoaded += SearchMessages_FriendicaMessagesLoaded;

            IsSearching = true;
            SearchResults = new ObservableCollection<FriendicaMessage>();
            SearchConversations = new ObservableCollection<FriendicaConversation>();
            searchMessages.SearchMessage(SearchString.ToLower());
        }

        private async void SearchMessages_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var searchMessages = sender as GetFriendicaMessages;

            if (searchMessages.StatusCode == HttpStatusCode.Ok && !searchMessages.IsErrorOccurred)
            {
                if (searchMessages.NoSearchResultsReturned)
                    NoSearchResults = true;
                else
                {
                    NoSearchResults = false;
                    foreach (var message in searchMessages.SearchResults)
                        SearchResults.Add(message);

                    // retrieve conversations of search results
                    var convs = searchMessages.RetrieveSearchConversations();
                    foreach (var conv in convs)
                    {
                        conv.IsLoading = true;
                        conv.NewMessageAdded += Conv_NewMessageAdded;
                        var getSearchConv = new GetFriendicaMessages();
                        getSearchConv.FriendicaMessagesLoaded += GetSearchConv_FriendicaMessagesLoaded;
                        getSearchConv.LoadConversation(conv.ConversationUri);
                        SearchConversations.Add(conv);
                    }
                }
            }
            else
            {
                if (searchMessages.NoSearchResultsReturned)
                {
                    NoSearchResults = true;
                }
                else
                {
                    // message to user: "there was an error on loading the private messages"
                    string errorMsg;
                    errorMsg = String.Format(loader.GetString("messageDialogMessagesErrorLoading"), searchMessages.ErrorMessage);
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    if (dialog.Result == 0)
                        LoadMessagesNew();
                }
            }
            IsSearching = false;

        }

        private void GetSearchConv_FriendicaMessagesLoaded(object sender, EventArgs e)
        {
            var getSearchConv = sender as GetFriendicaMessages;
            if (!getSearchConv.IsErrorOccurred)
            {
                var conv = SearchConversations.Single(c => c.ConversationUri == getSearchConv.ConversationUri);
                if (conv.Messages == null)
                    conv.Messages = new ObservableCollection<FriendicaMessage>();

                if (getSearchConv.MessagesReturned == null)
                    return;

                var messages = getSearchConv.MessagesReturned.OrderBy(m => m.MessageCreatedAtDateTime);
                foreach (var message in messages)
                {
                    conv.Messages.Add(message);
                }
                conv.IsLoaded = true;
                conv.IsLoading = false;
            }
        }
    }
}