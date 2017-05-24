using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Friendica_Mobile.Models
{
    public class FriendicaPhotoExtended : BindableClass
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        public enum PhotoCategories { FriendicaPhoto, FriendicaProfileImage };

        // properties for extended photo class
        private FriendicaPhoto _photo;
        public FriendicaPhoto Photo
        {
            get { return _photo; }
            set { _photo = value;
                if (value != null)
                {
                    if (value.PhotoEdited != null && value.PhotoEdited != "")
                    {
                        // parse database date utc to local system time
                        var editedDateTime = DateTime.ParseExact(value.PhotoEdited, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        PhotoEditedDateTime = editedDateTime.ToLocalTime();
                        PhotoEditedLocalized = _photoEditedDateTime.ToString("d") + " " + _photoEditedDateTime.ToString("t");
                    }
                    if (value.PhotoAllowCid == "" && value.PhotoDenyCid == "" && value.PhotoAllowGid == "" && value.PhotoDenyGid == "")
                        IsPubliclyVisible = true;
                    else
                        IsPubliclyVisible = false;

                    // user should see colored symbol if there are likes, dislikes or comments
                    IsConversationStarted = (value.PhotoComments.Count > 0 || 
                        value.PhotoActivities.ActivitiesLike.Count > 0 ||
                        value.PhotoActivities.ActivitiesDislike.Count > 0) ? true : false;

                    // initialize new right values with the current settings
                    NewPhotoAllowCid = value.PhotoAllowCid;
                    NewPhotoAllowGid = value.PhotoAllowGid;
                    NewPhotoDenyCid = value.PhotoDenyCid;
                    NewPhotoDenyGid = value.PhotoDenyGid;
                    NewPhotoDesc = value.PhotoDesc;
                }
            }
        }

        // change Photo.PhotoEdited to a localized version of the text
        private DateTime _photoEditedDateTime;
        public DateTime PhotoEditedDateTime
        {
            get { return _photoEditedDateTime; }
            set { _photoEditedDateTime = value; }
        }

        private string _photoEditedLocalized;
        public string PhotoEditedLocalized
        {
            get { return _photoEditedLocalized; }
            set
            {
                _photoEditedLocalized = value;
                OnPropertyChanged("PhotoEditedLocalized");
            }
        }

        // contains date extracted from the image file data (original shooting date)
        private DateTime _photoDateShotDateTime;
        private string _photoDateShotLocalized = "---";
        public string PhotoDateShotLocalized
        {
            get { return _photoDateShotLocalized; }
            set { _photoDateShotLocalized = value;
                OnPropertyChanged("PhotoDateShotLocalized");
            }
        }



        // enable indicator in different color if the photo is visible to anyone
        private bool _isPubliclyVisible;
        public bool IsPubliclyVisible
        {
            get { return _isPubliclyVisible; }
            set
            {
                _isPubliclyVisible = value;
                PubliclyVisibleColor = (value) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Transparent);
                OnPropertyChanged("IsPubliclyVisible");
            }
        }

        // define the color in which the ACL button is shown for the photo (red = visible for all, standard = closed audience)
        private Brush _publiclyVisibleColor;
        public Brush PubliclyVisibleColor
        {
            get { return _publiclyVisibleColor; }
            set { _publiclyVisibleColor = value;
                OnPropertyChanged("PubliclyVisibleColor");
            }
        }

        // indicator if there is an existing conversation to this photo (aka someone has commented or liked)
        private bool _isConversationStarted;
        public bool IsConversationStarted
        {
            get { return _isConversationStarted; }
            set { _isConversationStarted = value; }
        }

        // indicator that user enabled the photo description editing mode
        private bool _photoEditingEnabled;
        public bool PhotoEditingEnabled
        {
            get { return _photoEditingEnabled; }
            set
            {
                _photoEditingEnabled = value;
                OnPropertyChanged("PhotoEditingEnabled");
            }
        }

        // new albumname if user has changed the album
        private string _newAlbumName;
        public string NewAlbumName
        {
            get { return _newAlbumName; }
            set { _newAlbumName = value; }
        }

        // property for filling combobox with selectable albums
        private ObservableCollection<string> _selectableAlbums;
        public ObservableCollection<string> SelectableAlbums
        {
            get { return _selectableAlbums; }
            set { _selectableAlbums = value;
                OnPropertyChanged("SelectableAlbums");
            }
        }


        // new description text if user is changing the description
        private string _newPhotoDesc;
        public string NewPhotoDesc
        {
            get { return _newPhotoDesc; }
            set
            {
                _newPhotoDesc = value;
                OnPropertyChanged("NewPhotoDesc");
            }
        }

        // new access rights for photo - allow_cid
        private string _newPhotoAllowCid;
        public string NewPhotoAllowCid
        {
            get { return _newPhotoAllowCid; }
            set { _newPhotoAllowCid = value; }
        }

        // new access rights for photo - allow_gid
        private string _newPhotoAllowGid;
        public string NewPhotoAllowGid
        {
            get { return _newPhotoAllowGid; }
            set { _newPhotoAllowGid = value; }
        }

        // new access rights for photo - deny_cid
        private string _newPhotoDenyCid;
        public string NewPhotoDenyCid
        {
            get { return _newPhotoDenyCid; }
            set { _newPhotoDenyCid = value; }
        }

        // new access rights for photo - deny_gid
        private string _newPhotoDenyGid;
        public string NewPhotoDenyGid
        {
            get { return _newPhotoDenyGid; }
            set { _newPhotoDenyGid = value; }
        }

        // save type of image 
        private PhotoCategories _photoCategory;
        public PhotoCategories PhotoCategory
        {
            get { return _photoCategory; }
            set { _photoCategory = value;
                IsFriendicaPhoto = (value == PhotoCategories.FriendicaPhoto);
                MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
            }
        }

        // true if photo is a profileimage
        private bool _isFriendicaPhoto;
        public bool IsFriendicaPhoto
        {
            get { return _isFriendicaPhoto; }
            set { _isFriendicaPhoto = value;
                OnPropertyChanged("IsFriendicaPhoto");
            }
        }


        // thumb from photolist
        private string _photolistThumb;
        public string PhotolistThumb
        {
            get { return _photolistThumb; }
            set { _photolistThumb = value;
            }
        }

        // thumb indicators
        private bool _isLoadingThumbsize;
        public bool IsLoadingThumbSize
        {
            get { return _isLoadingThumbsize; }
            set { _isLoadingThumbsize = value;
                MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingThumbsize");
            }
        }

        private bool _thumbSizeLoaded;
        public bool ThumbSizeLoaded
        {
            get { return _thumbSizeLoaded; }
            set { _thumbSizeLoaded = value; }
        }

        private ImageSource _thumbSizeData;
        public ImageSource ThumbSizeData
        {
            get { return _thumbSizeData; }
            set { _thumbSizeData = value;
                OnPropertyChanged("ThumbSizeData");
            }
        }

        // medium indicators
        private bool _isLoadingMediumSize;
        public bool IsLoadingMediumSize
        {
            get { return _isLoadingMediumSize; }
            set { _isLoadingMediumSize = value;
                MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingMediumSize");
            }
        }

        private bool _mediumSizeLoaded;
        public bool MediumSizeLoaded
        {
            get { return _mediumSizeLoaded; }
            set { _mediumSizeLoaded = value; }
        }

        private ImageSource _mediumSizeData;
        public ImageSource MediumSizeData
        {
            get { return _mediumSizeData; }
            set
            {
                _mediumSizeData = value;
                OnPropertyChanged("MediumSizeData");
            }
        }

        // full size indicators
        private bool _isLoadingFullSize;
        public bool IsLoadingFullSize
        {
            get { return _isLoadingFullSize; }
            set { _isLoadingFullSize = value;
                MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsLoadingFullSize");
            }
        }

        private bool _fullSizeLoaded;
        public bool FullSizeLoaded
        {
            get { return _fullSizeLoaded; }
            set { _fullSizeLoaded = value; }
        }

        private ImageSource _fullSizeData;
        public ImageSource FullSizeData
        {
            get { return _fullSizeData; }
            set
            {
                _fullSizeData = value;
                OnPropertyChanged("FullSizeData");
            }
        }

        public double FlyoutWidth
        {
            get { return Window.Current.Bounds.Width; }
        }

        public double FlyoutHeight
        {
            get { return Window.Current.Bounds.Height; }
        }

        // property for StorageFile of a new image
        private StorageFile _newPhotoStorageFile;
        public StorageFile NewPhotoStorageFile
        {
            get { return _newPhotoStorageFile; }
            set { _newPhotoStorageFile = value; }
        }

        // indicator that image has not yet been uploaded/updated on server
        private bool _isServerOperationRunning;
        public bool IsServerOperationRunning
        {
            get { return _isServerOperationRunning; }
            set { _isServerOperationRunning = value;
                EditPhotoDescriptionCommand.RaiseCanExecuteChanged();
                MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsServerOperationRunning");
            }
        }


        // property for holding data of a new image
        private byte[] _newPhotoData;
        // property for holding data of a new profileimage
        private byte[] _newProfileImageData;

        // indicator that this photo is planned to be uploaded soon (not yet available on server)
        private bool _newUploadPlanned;
        public bool NewUploadPlanned
        {
            get { return _newUploadPlanned; }
            set { _newUploadPlanned = value;
                ShowUploadButton = (value || UpdatePlanned);
                MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
                CheckLoadingStatusRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        // indicator that this photo is planned to be updated soon
        private bool _updatePlanned;
        public bool UpdatePlanned
        {
            get { return _updatePlanned; }
            set { _updatePlanned = value;
                ShowUploadButton = (NewUploadPlanned || value);
                MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
                CheckLoadingStatusRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        // visible indicator for planned new uploads OR planned updates
        private bool _showUploadButton;
        public bool ShowUploadButton
        {
            get { return _showUploadButton; }
            set { _showUploadButton = value;
                ShowUploadButtonBrush = (value) ? new SolidColorBrush(Colors.Salmon) : new SolidColorBrush(Colors.Transparent);
                OnPropertyChanged("ShowUploadButton");
                OnPropertyChanged("ShowUploadButtonBrush");
            }
        }

        public Brush ShowUploadButtonBrush { get; set; }

        // indicator if change should be realised by a new upload of the photo instead of simply update dataset
        private bool _updateAsNewUpload;
        public bool UpdateAsNewUpload
        {
            get { return _updateAsNewUpload; }
            set { _updateAsNewUpload = value; }
        }


        // local indicators if different processes are already running
        private bool _isSavingChangedPhotoDescription;
        
        // eventhandlers
        public event EventHandler PrintButtonClicked;
        public event EventHandler NewPhotoUploaded;
        public event EventHandler PhotoDeleted;
        public event EventHandler NewProfileimageRequested;
        public event EventHandler UpdateAsNewUploadRequested;
        public event EventHandler MovePhotoToAlbumRequested;
        public event EventHandler CheckLoadingStatusRequested;

        #region commands

        // enable editor for photo description
        Mvvm.Command _editPhotoDescriptionCommand;
        public Mvvm.Command EditPhotoDescriptionCommand { get { return _editPhotoDescriptionCommand ?? (_editPhotoDescriptionCommand = new Mvvm.Command(ExecuteEditPhotoDescription, CanEditPhotoDescription)); } }
        private bool CanEditPhotoDescription()
        {
            if (_isServerOperationRunning)
                return false;
            else
                return true;
        }

        private void ExecuteEditPhotoDescription()
        {
            if (NewPhotoDesc == "")
                NewPhotoDesc = Photo.PhotoDesc;
            PhotoEditingEnabled = true;
        }


        // button enables user to start the upload as soon as all settings are correct
        Mvvm.Command _uploadPhotoCommand;
        public Mvvm.Command UploadPhotoCommand { get { return _uploadPhotoCommand ?? (_uploadPhotoCommand = new Mvvm.Command(ExecuteUploadPhoto, CanUploadPhoto)); } }
        private bool CanUploadPhoto()
        {
            return true;
        }

        private async void ExecuteUploadPhoto()
        {
            if (NewUploadPlanned)
            {
                // asking user only necessary on regular photos where user can change acl settings (profile images are always public)
                if (PhotoCategory == PhotoCategories.FriendicaPhoto)
                {
                    // ask user if upload to server should now be performed (last warning that changing ACL afterwards could cause issues)
                    string errorMsg = loader.GetString("messageDialogPhotosAskForUploadConfirmation");
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    // start upload if user has confirmed with yes
                    if (dialog.Result == 0)
                    {
                        if (CheckNoServerSettings())
                        {
                            // "this would now perform a server operation, in sample mode not possible"
                            errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                            dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                            await dialog.ShowDialog(0, 0);
                            return;
                        }
                        await UploadNewPhotoToServerAsync();
                    }
                    else
                        return;
                }
                else if (PhotoCategory == PhotoCategories.FriendicaProfileImage)
                {
                    if (CheckNoServerSettings())
                    {
                        // "this would now perform a server operation, in sample mode not possible"
                        var errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                        var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                        await dialog.ShowDialog(0, 0);
                        return;
                    }
                    await UploadNewPhotoToServerAsync();
                }
            }
            else if (UpdatePlanned)
                await PerformUploadToServerAsync(); // user wants to upload a previously aborted update
        }


        // save changed description to server
        Mvvm.Command _saveChangedPhotoDescriptionCommand;
        public Mvvm.Command SaveChangedPhotoDescriptionCommand { get { return _saveChangedPhotoDescriptionCommand ?? (_saveChangedPhotoDescriptionCommand = new Mvvm.Command(ExecuteSaveChangedPhotoDescription, CanSaveChangedPhotoDescription)); } }
        private bool CanSaveChangedPhotoDescription()
        {
            return (!_isSavingChangedPhotoDescription) ? true : false;
        }

        private async void ExecuteSaveChangedPhotoDescription()
        {
            // indicator to avoid double calling the function
            _isSavingChangedPhotoDescription = true;

            // we will only perform this on existing photos (new photos description can be changed until user hits the upload button)
            if (!NewUploadPlanned && !UpdatePlanned)
            {
                // check if text has not changed, then cancel and go back to editing
                if (Photo.PhotoDesc == NewPhotoDesc)
                {
                    string errorMsg = loader.GetString("messageDialogPhotosNewPhotoDescriptionUnchanged");
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                    _isSavingChangedPhotoDescription = false;
                    return;
                }

                if (CheckNoServerSettings())
                {
                    // "this would now perform a server operation, in sample mode not possible"
                    var errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                    Photo.PhotoDesc = NewPhotoDesc;
                    PhotoEditingEnabled = false;
                    _isSavingChangedPhotoDescription = false;
                    return;
                }

                // now we are in the loop to change the description on server
                await UpdatePhotoOnServerAsync();

                PhotoEditingEnabled = false;
            }
            _isSavingChangedPhotoDescription = false;
        }


        // show access rights for the image in a separate window
        Mvvm.Command _editPhotoAccessRightsCommand;
        public Mvvm.Command EditPhotoAccessRightsCommand { get { return _editPhotoAccessRightsCommand ?? (_editPhotoAccessRightsCommand = new Mvvm.Command(ExecuteEditPhotoAccessRights, CanEditPhotoAccessRights)); } }
        private bool CanEditPhotoAccessRights()
        {
            return true;
        }

        private async void ExecuteEditPhotoAccessRights()
        {
            if (PhotoCategory == PhotoCategories.FriendicaProfileImage)
            {
                // "profileimages are always public. when used in limited profile, then only allowed contacts see the profileimage"
                string errorMsg = loader.GetString("messageDialogPhotosShowRightsProfileimage");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
            else if (PhotoCategory == PhotoCategories.FriendicaPhoto)
            {
                var frame = App.GetFrameForNavigation();
                frame.Navigate(typeof(A6_PhotoRights));
            }
        }


        // navigate to page with the image as background in order to enable user to paint on the image
        Mvvm.Command _inkCanvasPhotoCommand;
        public Mvvm.Command InkCanvasPhotoCommand { get { return _inkCanvasPhotoCommand ?? (_inkCanvasPhotoCommand = new Mvvm.Command(ExecuteInkCanvasPhoto, CanInkCanvasPhoto)); } }
        private bool CanInkCanvasPhoto()
        {
            return true;
        }

        private void ExecuteInkCanvasPhoto()
        {
            // on Navigating away store PhotoViewmodel into App.PhotosVm
            var frame = App.GetFrameForNavigation();
            frame.Navigate(typeof(A5_InkCanvas), this);
        }


        // move the photo to another album
        Mvvm.Command _movePhotoToAlbumCommand;
        public Mvvm.Command MovePhotoToAlbumCommand { get { return _movePhotoToAlbumCommand ?? (_movePhotoToAlbumCommand = new Mvvm.Command(ExecuteMovePhotoToAlbum, CanMovePhotoToAlbum)); } }
        private bool CanMovePhotoToAlbum()
        {
            // don't allow changing album when we are currently loading data
            // don't allow changing album when profileimage (keep the album on profile images)
            if (IsLoadingFullSize || IsLoadingMediumSize || IsLoadingThumbSize || IsServerOperationRunning 
                    || PhotoCategory == PhotoCategories.FriendicaProfileImage
                    || NewUploadPlanned || UpdatePlanned)
                return false;
            else
                return true;
        }

        private async void ExecuteMovePhotoToAlbum()
        {
            // hint if no album was selected
            if (NewAlbumName == null)
            {
                // "Du hast kein Album ausgewählt, das Photo wurde nicht verschoben."
                string errorMsg = loader.GetString("messageDialogPhotosMovingNoAlbumSelected");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                return;
            }

            // call eventhandler (code for moving is in PhotosViewmodel.cs)
            MovePhotoToAlbumRequested?.Invoke(this, EventArgs.Empty);
        }


        // command for deleting the photo from server
        Mvvm.Command _deletePhotoCommand;
        public Mvvm.Command DeletePhotoCommand { get { return _deletePhotoCommand ?? (_deletePhotoCommand = new Mvvm.Command(ExecuteDeletePhoto, CanDeletePhoto)); } }
        private bool CanDeletePhoto()
        {
            return true;
        }

        private async void ExecuteDeletePhoto()
        {
            if (NewUploadPlanned)
            {
                // ask user if not yet uploaded image should now be deleted (no undo possible)
                string errorMsg = loader.GetString("messageDialogPhotosAskForConfirmationDeleteLocal");
                var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                await dialog.ShowDialog(0, 1);

                // start deleting if user has confirmed with yes
                if (dialog.Result == 0)
                {
                    if (CheckNoServerSettings())
                    {
                        // "this would now perform a server operation, in sample mode not possible"
                        errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                        dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                        await dialog.ShowDialog(0, 0);
                        // anyway, we can delete the photo within the app
                        PhotoDeleted?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    PhotoDeleted?.Invoke(this, EventArgs.Empty);
                    // update status after deletion of the un-uploaded image
                    CheckLoadingStatusRequested?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                // ask user if deleting image from server should now be performed (no undo possible)
                string errorMsg = loader.GetString("messageDialogPhotosAskForConfirmationDelete");
                var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                await dialog.ShowDialog(0, 1);

                // start deleting if user has confirmed with yes
                if (dialog.Result == 0)
                {
                    if (CheckNoServerSettings())
                    {
                        // "this would now perform a server operation, in sample mode not possible"
                        errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                        dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                        await dialog.ShowDialog(0, 0);
                        // anyway, we can delete the photo within the app
                        PhotoDeleted?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                    await DeletePhotoFromServerAsync();
                }
            }
        }


        // command for creating profile image from current photo
        Mvvm.Command _createProfileimageCommand;
        public Mvvm.Command CreateProfileimageCommand { get { return _createProfileimageCommand ?? (_createProfileimageCommand = new Mvvm.Command(ExecuteCreateProfileimage, CanCreateProfileimage)); } }
        private bool CanCreateProfileimage()
        {
            // only active when we have a regular photo
            return (PhotoCategory == PhotoCategories.FriendicaPhoto);
        }

        private void ExecuteCreateProfileimage()
        {
            NewProfileimageRequested?.Invoke(this, EventArgs.Empty);
        }


        // command for showing the conversation to a photo (if there is one)
        Mvvm.Command _showPhotoConversationCommand;
        public Mvvm.Command ShowPhotoConversationCommand { get { return _showPhotoConversationCommand ?? (_showPhotoConversationCommand = new Mvvm.Command(ExecuteShowPhotoConversation, CanShowPhotoConversation)); } }
        private bool CanShowPhotoConversation()
        {
            return true;
        }

        private void ExecuteShowPhotoConversation()
        {
            var frame = App.GetFrameForNavigation();
            frame.Navigate(typeof(A1_ShowThread), this.PhotolistThumb);
        }


        // command for printing the picture (if device has a capable printer)
        Mvvm.Command _printPhotoCommand;
        public Mvvm.Command PrintPhotoCommand { get { return _printPhotoCommand ?? (_printPhotoCommand = new Mvvm.Command(ExecutePrintPhoto, CanPrintPhoto)); } }
        private bool CanPrintPhoto()
        {
            // enable print button only if device is capable of printing
            try
            {
                return (Windows.Graphics.Printing.PrintManager.IsSupported());
            }
            catch { return false; }
        }

        private async void ExecutePrintPhoto()
        {
            if (Windows.Graphics.Printing.PrintManager.IsSupported())
            {
                PrintButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // printing is not supported on this device
                string errorMsg = "Drucken wird auf diesem Gerät nicht unterstützt, komisch dass die Meldung überhaupt kommt.";
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
        }


        #endregion


        public FriendicaPhotoExtended()
        {
            Photo = new FriendicaPhoto();
            SelectableAlbums = new ObservableCollection<string>();
        }


        public async Task GetPhotoFromServer()
        {
            // load basis photo details (without data for scales)
            var getHttpLoadPhoto = new GetFriendicaPhotos();
            await getHttpLoadPhoto.LoadPhotoFromServerAsync(Photo.PhotoId);
            Photo = getHttpLoadPhoto.PhotoReturned;

            await WorkOnPhotoScales();
            CheckLoadingStatusRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task WorkOnPhotoScales()
        {
            // get a list of all returned scales (ordered descending to load smaller images first)
            // api returns all scales from min to max in database -> there is always a scale=3 which is not available on server
            // same function is checking which category we have for the photo (Photo or ProfileImage)
            var scalesList = GetScalesFromPhotoLink();

            foreach (var scale in scalesList)
            {
                // cancel loop for this scale if not suitable to photo category
                if (PhotoCategory == PhotoCategories.FriendicaProfileImage && !(scale == 4 || scale == 5 || scale == 6))
                    continue;
                if (PhotoCategory == PhotoCategories.FriendicaPhoto && !(scale == 0 || scale == 1 || scale == 2))
                    continue;

                if (App.Settings.SaveLocalAllowed == "false" ||     // don't proceed on scales 0 and 1, set their imgSource to scale 2, loading triggered separately when user moves to photo
                        (scale == 0 && !App.Settings.SaveFullsizePhotosAllowed)) // don't proceed on scale 0 when user only allowed small images to be saved, loading triggered when user moves to photo
                {
                    switch (scale)
                    {
                        case 2:
                        case 6:
                            IsLoadingThumbSize = true;
                            var imgSource = await GetImageSourceFromPhotoIdAndScaleAsync(scale.ToString());
                            ThumbSizeData = imgSource;
                            MediumSizeData = imgSource;
                            FullSizeData = imgSource;
                            ThumbSizeLoaded = true;
                            // keep this on false to indicate later for the reloading need of other higher scales
                            MediumSizeLoaded = false;
                            FullSizeLoaded = false;
                            IsLoadingThumbSize = false;
                            break;
                        case 5:       //case 1 will loaded when user is opening the album and navigating to the photo
                            IsLoadingMediumSize = true;
                            imgSource = await GetImageSourceFromPhotoIdAndScaleAsync(scale.ToString());
                            MediumSizeData = imgSource;
                            MediumSizeLoaded = true;
                            IsLoadingMediumSize = false;
                            break;
                        case 4:
                            IsLoadingFullSize = true;
                            imgSource = await GetImageSourceFromPhotoIdAndScaleAsync(scale.ToString());
                            FullSizeData = imgSource;
                            FullSizeLoaded = true;
                            IsLoadingFullSize = false;
                            break;
                        case 0:
                            IsLoadingFullSize = false;
                            break;
                    }
                }
                else
                {
                    switch (scale)
                    {
                        case 0:
                        case 4:
                            IsLoadingFullSize = true;
                            var imgSource = await GetImageSourceFromPhotoIdAndScaleAsync(scale.ToString());
                            FullSizeData = imgSource;
                            FullSizeLoaded = true;
                            IsLoadingFullSize = false;
                            break;
                        case 1:
                        case 5:
                            IsLoadingMediumSize = true;
                            imgSource = await GetImageSourceFromPhotoIdAndScaleAsync(scale.ToString());
                            MediumSizeData = imgSource;
                            MediumSizeLoaded = true;
                            IsLoadingMediumSize = false;
                            break;
                        case 2:
                        case 6:
                            // indicators should start on each size because we know that the user get the image as soon as loaded
                            IsLoadingThumbSize = true;
                            IsLoadingMediumSize = true;
                            IsLoadingFullSize = true;
                            imgSource = await GetImageSourceFromPhotoIdAndScaleAsync(scale.ToString());
                            ThumbSizeData = imgSource;
                            ThumbSizeLoaded = true;
                            IsLoadingThumbSize = false;
                            break;
                    }
                }
            }
        }

        public void SaveChangedPhotoData(byte[] pixels)
        {
            if (PhotoCategory == PhotoCategories.FriendicaPhoto)
                _newPhotoData = pixels;
            else if (PhotoCategory == PhotoCategories.FriendicaProfileImage)
                _newProfileImageData = pixels;
        }


        public async Task<BitmapImage> GetInkedBitmapImage()
        {
            using (MemoryStream ms = new MemoryStream(_newPhotoData))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                IRandomAccessStream a1 = await ConvertToRandomAccessStreamAsync(ms);
                bitmapImage.SetSource(a1);
                return bitmapImage;
            }
        }

        public async Task<ImageSource> GetCroppedProfileimageImageSource()
        {
            using (MemoryStream ms = new MemoryStream(_newProfileImageData))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                IRandomAccessStream a1 = await ConvertToRandomAccessStreamAsync(ms);
                bitmapImage.SetSource(a1);
                return bitmapImage;
            }
        }


        public async Task PrepareNewPhotoFromDeviceAsync(StorageFile file, string album)
        {
            NewPhotoStorageFile = file;

            using (IRandomAccessStream inputStream = await NewPhotoStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var reader = new DataReader(inputStream);
                var bytes = new byte[inputStream.Size];
                await reader.LoadAsync((uint)inputStream.Size);
                reader.ReadBytes(bytes);
                _newPhotoData = bytes;
                Photo.PhotoAlbum = album;
            }
            await GetDateTakenFromImageAsync(NewPhotoStorageFile);
            await GetCommentFromImageAsync(NewPhotoStorageFile);

            // set the bitmapdata for fullsize/medium/thumb with the local image until upload to server has been finished
            var folder = ApplicationData.Current.TemporaryFolder;

            StorageFile tempFile = null;
            if (file.Path.Contains(folder.Path))
                tempFile = file;
            else 
                tempFile = await file.CopyAsync(folder, file.Name, NameCollisionOption.ReplaceExisting);

            var bitmapData = new BitmapImage(new Uri(tempFile.Path, UriKind.RelativeOrAbsolute));
            ThumbSizeData = bitmapData;
            MediumSizeData = bitmapData;
            FullSizeData = bitmapData;

            // photo = standard acl settings from Settings, profileimage always public
            if (PhotoCategory == PhotoCategories.FriendicaPhoto)
                GetDefaultACL();
        }

        public void GetDefaultACL()
        {
            if (App.Settings.ACLPrivatePost && (App.Settings.ACLPrivateSelectedContacts != "" || App.Settings.ACLPrivateSelectedGroups != ""))
            {
                IsPubliclyVisible = false;
                NewPhotoAllowCid = App.Settings.ACLPrivateSelectedContacts;
                NewPhotoAllowGid = App.Settings.ACLPrivateSelectedGroups;
            }
            else
            {
                IsPubliclyVisible = true;
                NewPhotoAllowCid = "";
                NewPhotoAllowGid = "";
            }
        }


        public async void CheckForNeededServerUpdate()
        {
            // show navigation again
            App.Settings.HideNavigationElements = false;

            // do nothing on new photos, user must click the upload button to finally load the settings
            if (NewUploadPlanned)
                return;
            else
            {
                await PerformUploadToServerAsync();
            }
        }

        private async Task PerformUploadToServerAsync()
        {
            // don't call this if user has not made any changes (i.e. returns from acl page etc. should not ask for update in this case)
            if (HasUserMadeChanges())
            {
                if (UpdateAsNewUpload)
                {
                    // ask user if changing acl on server should now be performed (last warning that changing ACL with new upload could have other impacts)
                    string errorMsg;
                    if (HasUserChangedACL())
                        errorMsg = loader.GetString("messageDialogPhotosAskForConfirmationACLUpdateAsNew");
                    else
                        errorMsg = loader.GetString("messageDialogPhotosAskForUploadConfirmationInked");
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    // start upload if user has confirmed with yes
                    if (dialog.Result == 0)
                    {
                        if (CheckNoServerSettings())
                        {
                            // "this would now perform a server operation, in sample mode not possible"
                            errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                            dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                            await dialog.ShowDialog(0, 0);
                            return;
                        }
                        // user wants to re-upload the image; code needs to be executed in PhotoAlbum
                        UpdateAsNewUploadRequested?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        // user has aborted, we want to give now the option to manually upload
                        UpdatePlanned = true;
                        return;
                    }
                }
                else
                {
                    // ask user if changing acl on server should now be performed (last warning that changing ACL without new upload will destroy commentary function)
                    string errorMsg;
                    if (HasUserChangedACL())
                        errorMsg = loader.GetString("messageDialogPhotosAskForConfirmationACLUpdatePhoto");
                    else
                        errorMsg = loader.GetString("messageDialogPhotosAskForUploadConfirmationInked");
                    var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialog.ShowDialog(0, 1);

                    // start upload if user has confirmed with yes
                    if (dialog.Result == 0)
                    {
                        if (CheckNoServerSettings())
                        {
                            // "this would now perform a server operation, in sample mode not possible"
                            errorMsg = loader.GetString("messageDialogPhotosNoServerSupport");
                            dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                            await dialog.ShowDialog(0, 0);
                            return;
                        }
                        await UpdatePhotoOnServerAsync();
                    }
                    else
                    {
                        // user has aborted, we want to give now the option to manually upload
                        UpdatePlanned = true;
                        return;
                    }
                }
            }
        }

        private bool CheckNoServerSettings()
        {
            if (App.Settings.FriendicaServer == "" || App.Settings.FriendicaServer == "http://"
                    || App.Settings.FriendicaServer == "https://" || App.Settings.FriendicaServer == null
                    || App.Settings.FriendicaUsername == null || App.Settings.FriendicaPassword == null
                    || App.Settings.FriendicaUsername == "" || App.Settings.FriendicaPassword == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    private bool HasUserMadeChanges()
        {
            var changed = false;

            changed |= (_newPhotoData != null && _newPhotoData.Length != 0);
            changed |= (NewAlbumName != null && Photo.PhotoAlbum != NewAlbumName);
            changed |= (NewPhotoDesc != null && Photo.PhotoDesc != NewPhotoDesc);
            changed |= (NewPhotoAllowCid != null && Photo.PhotoAllowCid != NewPhotoAllowCid);
            changed |= (NewPhotoAllowGid != null && Photo.PhotoAllowGid != NewPhotoAllowGid);
            changed |= (NewPhotoDenyCid != null && Photo.PhotoDenyCid != NewPhotoDenyCid);
            changed |= (NewPhotoDenyGid != null && Photo.PhotoDenyGid != NewPhotoDenyGid);

            return changed;
        }

        private bool HasUserChangedACL()
        {
            var changed = false;

            changed |= (NewPhotoAllowCid != null && Photo.PhotoAllowCid != NewPhotoAllowCid);
            changed |= (NewPhotoAllowGid != null && Photo.PhotoAllowGid != NewPhotoAllowGid);
            changed |= (NewPhotoDenyCid != null && Photo.PhotoDenyCid != NewPhotoDenyCid);
            changed |= (NewPhotoDenyGid != null && Photo.PhotoDenyGid != NewPhotoDenyGid);

            return changed;
        }

        public async Task DeletePhotoFromServerAsync()
        {
            // set indicator to avoid the user working on photo description etc.
            IsServerOperationRunning = true;

            // delete photo from server
            var postHttpDeletePhoto = new GetFriendicaPhotos();
            await postHttpDeletePhoto.DeletePhotoAsync(Photo.PhotoId);

            switch (postHttpDeletePhoto.ErrorPhotoFriendica)
            {
                case GetFriendicaPhotos.PhotoErrors.OK:
                    // react on positive deletion - call event to remove photo from album in app
                    PhotoDeleted?.Invoke(this, EventArgs.Empty);
                    break;
                case GetFriendicaPhotos.PhotoErrors.ServerNotAnswered:
                    // ask for retry
                    var filename = (NewPhotoStorageFile != null) ? NewPhotoStorageFile.Name : Photo.PhotoFilename;
                    var errorMsgRetry = String.Format(loader.GetString("messageDialogPhotoUploadingNoReaction"), filename);
                    var dialogRetry = new MessageDialogMessage(errorMsgRetry, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialogRetry.ShowDialog(0, 1);

                    if (dialogRetry.Result == 0)
                        await postHttpDeletePhoto.DeletePhotoAsync(Photo.PhotoId);
                    break;
                case GetFriendicaPhotos.PhotoErrors.NoServerSupport:
                case GetFriendicaPhotos.PhotoErrors.AuthenticationFailed:
                case GetFriendicaPhotos.PhotoErrors.NoPhotoIdSpecified:
                case GetFriendicaPhotos.PhotoErrors.NoPhotoFound:
                case GetFriendicaPhotos.PhotoErrors.UnknownError:
                    // message to user that we had an unexpected error
                    filename = (NewPhotoStorageFile != null) ? NewPhotoStorageFile.Name : Photo.PhotoFilename;
                    var errorMsgProblem = String.Format(loader.GetString("messageDialogPhotosUploadingUnknownError"), filename);
                    var dialogError = new MessageDialogMessage(errorMsgProblem, "", "OK", null);
                    await dialogError.ShowDialog(0, 0);
                    break;
            }

            IsServerOperationRunning = false;
        }

        public async Task<GetFriendicaPhotos.PhotoErrors> UpdatePhotoOnServerAsync()
        {
            // set indicator to avoid the user working on photo description etc.
            IsServerOperationRunning = true;

            // delete existing data if user wants to upload a changed image (strokes)
            if (_newPhotoData != null)
            {
                ThumbSizeLoaded = false;
                MediumSizeLoaded = false;
                FullSizeLoaded = false;
                await DeleteAllScalesFromDevice();
            }

            // update photo on server
            var postHttpUpdatePhoto = new GetFriendicaPhotos();
            var type = (NewPhotoStorageFile != null) ? NewPhotoStorageFile.ContentType : Photo.PhotoType;
            var filename = (NewPhotoStorageFile != null) ? NewPhotoStorageFile.Name : Photo.PhotoFilename;
            await postHttpUpdatePhoto.PostUpdatePhotoAsync(PrepareUpdateData(), type, filename);

            switch (postHttpUpdatePhoto.ErrorPhotoFriendica)
            {
                case GetFriendicaPhotos.PhotoErrors.OK:
                    // react on positive upload
                    UpdatePlanned = false;
                    if (postHttpUpdatePhoto.PhotoReturned == null)
                    {
                        // PhotoReturned == null means that we had a successfull update without picture data
                        UpdateLocalData();
                    }
                    else
                        Photo = postHttpUpdatePhoto.PhotoReturned;

                    if (_newPhotoData != null)
                    {
                        MediumSizeData = null;
                        _newPhotoData = null;
                    }

                    // load scales of photo to device if allowed
                    await WorkOnPhotoScales();
                    break;
                case GetFriendicaPhotos.PhotoErrors.ServerNotAnswered:
                    // ask for retry
                    var errorMsgRetry = String.Format(loader.GetString("messageDialogPhotoUploadingNoReaction"), filename);
                    var dialogRetry = new MessageDialogMessage(errorMsgRetry, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialogRetry.ShowDialog(0, 1);

                    if (dialogRetry.Result == 0)
                        await postHttpUpdatePhoto.PostUpdatePhotoAsync(PrepareUpdateData(), type, filename);
                    break;
                case GetFriendicaPhotos.PhotoErrors.ImageSizeLimitsExceeded:
                case GetFriendicaPhotos.PhotoErrors.ProcessImageDataFailed:
                case GetFriendicaPhotos.PhotoErrors.ImageUploadFailed:
                    // message to user that we had an server error (most likely because of size limit)
                    var errorMsgInternal = String.Format(loader.GetString("messageDialogPhotosUploadingInternalError"), filename);
                    var dialogError = new MessageDialogMessage(errorMsgInternal, "", "OK", null);
                    await dialogError.ShowDialog(0, 0);
                    break;
                case GetFriendicaPhotos.PhotoErrors.NoServerSupport:
                case GetFriendicaPhotos.PhotoErrors.AuthenticationFailed:
                case GetFriendicaPhotos.PhotoErrors.NoMediaDataSubmitted:
                case GetFriendicaPhotos.PhotoErrors.AclDataInvalid:
                case GetFriendicaPhotos.PhotoErrors.NoAlbumnameSpecified:
                case GetFriendicaPhotos.PhotoErrors.NoPhotoFound:
                case GetFriendicaPhotos.PhotoErrors.UnknownError:
                    // message to user that we had an unexpected error
                    var errorMsgProblem = String.Format(loader.GetString("messageDialogPhotosUploadingUnknownError"), filename);
                    dialogError = new MessageDialogMessage(errorMsgProblem, "", "OK", null);
                    await dialogError.ShowDialog(0, 0);
                    break;
            }

            IsServerOperationRunning = false;
            return postHttpUpdatePhoto.ErrorPhotoFriendica;
        }

        public async Task<GetFriendicaPhotos.PhotoErrors> UploadNewPhotoToServerAsync()
        {
            // set indicator to avoid the user working on photo description etc.
            IsServerOperationRunning = true;

            // upload file to server
            var postHttpUploadNewPhoto = new GetFriendicaPhotos();
            var type = (NewPhotoStorageFile != null) ? NewPhotoStorageFile.ContentType : "image/jpeg";
            var filename = (NewPhotoStorageFile != null) ? NewPhotoStorageFile.Name : "NewDrawing.jpg";
            if (PhotoCategory == PhotoCategories.FriendicaPhoto)
                await postHttpUploadNewPhoto.PostCreatePhotoAsync(PrepareNewPhotoData(), type, filename);
            else if (PhotoCategory == PhotoCategories.FriendicaProfileImage)
                await postHttpUploadNewPhoto.PostCreateProfileimageAsync(PrepareNewProfileimageData(), type, filename);

            switch (postHttpUploadNewPhoto.ErrorPhotoFriendica)
            {
                case GetFriendicaPhotos.PhotoErrors.OK:
                    NewUploadPlanned = false;

                    // react on positive upload (returned json with photo details on regular photo upload)
                    if (PhotoCategory == PhotoCategories.FriendicaPhoto)
                    {
                        Photo = postHttpUploadNewPhoto.PhotoReturned;
                        // load scales of photo to device if allowed
                        await WorkOnPhotoScales();
                        // set back data property
                        if (_newPhotoData != null)
                            _newPhotoData = null;
                    }
                    else if (PhotoCategory == PhotoCategories.FriendicaProfileImage)
                    {
                        // profileimage upload returns user element with new profileimageurl
                        var profileurl = new Uri(postHttpUploadNewPhoto.UserReturned.UserProfileImageUrl, UriKind.RelativeOrAbsolute);
                        // load this photo
                        Photo = new FriendicaPhoto();
                        var filenameUrl = profileurl.Segments[profileurl.Segments.Count() - 1];
                        var parts = Regex.Split(filenameUrl, "-");
                        if (parts[1].Contains(".jpg") || parts[1].Contains(".png"))
                            Photo.PhotoId = parts[0];
                        await GetPhotoFromServer();
                        // set back data property
                        if (_newProfileImageData != null)
                            _newProfileImageData = null;
                    }

                    // set view to this actual image
                    NewPhotoUploaded?.Invoke(this, EventArgs.Empty);
                    break;
                case GetFriendicaPhotos.PhotoErrors.ServerNotAnswered:
                    // ask for retry
                    var errorMsgRetry = String.Format(loader.GetString("messageDialogPhotoUploadingNoReaction"), NewPhotoStorageFile.Name);
                    var dialogRetry = new MessageDialogMessage(errorMsgRetry, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialogRetry.ShowDialog(0, 1);

                    if (dialogRetry.Result == 0)
                    {
                        if (PhotoCategory == PhotoCategories.FriendicaPhoto)
                            await postHttpUploadNewPhoto.PostCreatePhotoAsync(PrepareNewPhotoData(), type, filename);
                        else if (PhotoCategory == PhotoCategories.FriendicaProfileImage)
                            await postHttpUploadNewPhoto.PostCreateProfileimageAsync(PrepareNewProfileimageData(), type, filename);
                    }
                    break;
                case GetFriendicaPhotos.PhotoErrors.ImageSizeLimitsExceeded:
                case GetFriendicaPhotos.PhotoErrors.ProcessImageDataFailed:
                case GetFriendicaPhotos.PhotoErrors.ImageUploadFailed:
                    // message to user that we had an server error (most likely because of size limit)
                    var errorMsgInternal = String.Format(loader.GetString("messageDialogPhotosUploadingInternalError"), NewPhotoStorageFile.Name);
                    var dialogError = new MessageDialogMessage(errorMsgInternal, "", "OK", null);
                    await dialogError.ShowDialog(0, 0);
                    break;
                case GetFriendicaPhotos.PhotoErrors.NoServerSupport:
                case GetFriendicaPhotos.PhotoErrors.AuthenticationFailed:
                case GetFriendicaPhotos.PhotoErrors.NoMediaDataSubmitted:
                case GetFriendicaPhotos.PhotoErrors.AclDataInvalid:
                case GetFriendicaPhotos.PhotoErrors.NoAlbumnameSpecified:
                case GetFriendicaPhotos.PhotoErrors.NoPhotoFound:
                case GetFriendicaPhotos.PhotoErrors.UnknownError:
                    // message to user that we had an unexpected error
                    var errorMsgProblem = String.Format(loader.GetString("messageDialogPhotosUploadingUnknownError"), NewPhotoStorageFile.Name);
                    dialogError = new MessageDialogMessage(errorMsgProblem, "", "OK", null);
                    await dialogError.ShowDialog(0, 0);
                    break;
            }
            IsServerOperationRunning = false;
            return postHttpUploadNewPhoto.ErrorPhotoFriendica;
        }

        private Dictionary<string, object> PrepareUpdateData()
        {
            var Parameters = new Dictionary<string, object>();
            
            // always add id of photo to update calls
            Parameters.Add("photo_id", Photo.PhotoId);

            if (_newPhotoData != null && _newPhotoData.Length != 0)
                Parameters.Add("media", _newPhotoData);

            // provide always the current album name
            Parameters.Add("album", Photo.PhotoAlbum);
            if (NewAlbumName != null && Photo.PhotoAlbum != NewAlbumName)
                Parameters.Add("album_new", NewAlbumName);

            if (NewPhotoDesc != null && Photo.PhotoDesc != NewPhotoDesc)
                Parameters.Add("desc", NewPhotoDesc);

            if (NewPhotoAllowCid != null && Photo.PhotoAllowCid != NewPhotoAllowCid)
                Parameters.Add("allow_cid", NewPhotoAllowCid);
            if (NewPhotoAllowGid != null && Photo.PhotoAllowGid != NewPhotoAllowGid)
                Parameters.Add("allow_gid", NewPhotoAllowGid);
            if (NewPhotoDenyCid != null && Photo.PhotoDenyCid != NewPhotoDenyCid)
                Parameters.Add("deny_cid", NewPhotoDenyCid);
            if (NewPhotoDenyGid != null && Photo.PhotoDenyGid != NewPhotoDenyGid)
                Parameters.Add("deny_gid", NewPhotoDenyGid);

            return Parameters;
        }

        private void UpdateLocalData()
        {
            if (NewAlbumName != null && Photo.PhotoAlbum != NewAlbumName)
                Photo.PhotoAlbum = NewAlbumName;
            if (NewPhotoDesc != null && Photo.PhotoDesc != NewPhotoDesc)
                Photo.PhotoDesc = NewPhotoDesc;
            if (NewPhotoAllowCid != null && Photo.PhotoAllowCid != NewPhotoAllowCid)
                Photo.PhotoAllowCid = NewPhotoAllowCid;
            if (NewPhotoAllowGid != null && Photo.PhotoAllowGid != NewPhotoAllowGid)
                Photo.PhotoAllowGid = NewPhotoAllowGid;
            if (NewPhotoDenyCid != null && Photo.PhotoDenyCid != NewPhotoDenyCid)
                Photo.PhotoDenyCid = NewPhotoDenyCid;
            if (NewPhotoDenyGid != null && Photo.PhotoDenyGid != NewPhotoDenyGid)
                Photo.PhotoDenyGid = NewPhotoDenyGid;
        }

        public void ResetChangedData()
        {
            NewPhotoAllowCid = Photo.PhotoAllowCid;
            NewPhotoAllowGid = Photo.PhotoAllowGid;
            NewPhotoDenyCid = Photo.PhotoDenyCid;
            NewPhotoDenyGid = Photo.PhotoDenyGid;
            NewPhotoDesc = Photo.PhotoDesc;
            NewAlbumName = Photo.PhotoAlbum;
            NewPhotoStorageFile = null;
            _newPhotoData = null;
            _newProfileImageData = null;
            UpdatePlanned = false;
            NewUploadPlanned = false;
        }

        private Dictionary<string, object> PrepareNewPhotoData()
        {
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("media", _newPhotoData);
            Parameters.Add("album", Photo.PhotoAlbum);
            Parameters.Add("allow_cid", NewPhotoAllowCid);
            Parameters.Add("allow_gid", NewPhotoAllowGid);
            Parameters.Add("deny_cid", NewPhotoDenyCid);
            Parameters.Add("deny_gid", NewPhotoDenyGid);
            if (Photo.PhotoDesc != null)
                Parameters.Add("desc", NewPhotoDesc);
            //Parameters.Add("visibility", "true"); // currently not used, API could accept this to show the photo on wall if true, default is false
            return Parameters;
        }

        private Dictionary<string, object> PrepareNewProfileimageData()
        {
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("image", _newProfileImageData);
            if (Photo.PhotoDesc != null)
                Parameters.Add("desc", NewPhotoDesc);
            //Parameters.Add("visibility", "true"); // currently not used, API could accept this to show the photo on wall if true, default is false
            //Paremeters.Add("profile_id", 0); // currently not used, API could accept this to define the picture for a special profile, currently we add the photo to each existing profile
            return Parameters;
        }


        private async Task<FriendicaPhoto> LoadImageFromServerAsync(string scale)
        {
            var getHttpLoadPhotoScale = new GetFriendicaPhotos();
            await getHttpLoadPhotoScale.LoadPhotoFromServerAsync(Photo.PhotoId, scale);
            var photoData = getHttpLoadPhotoScale.PhotoReturned;

            if (getHttpLoadPhotoScale.ErrorPhotoFriendica == GetFriendicaPhotos.PhotoErrors.NoPhotoDataReturned)
            {
                // oh shit, the api has missed to return the scale=0 data, this can happen on older PHP versions where 
                // scale=0 is interpreted as boolean operator, however it works in that cases with scale=false
                await getHttpLoadPhotoScale.LoadPhotoFromServerAsync(Photo.PhotoId, "false");
                photoData = getHttpLoadPhotoScale.PhotoReturned;
            }
            return photoData;
        }


        private List<int> GetScalesFromPhotoLink()
        {
            var scalesList = new List<int>();

            if (Photo == null || Photo.PhotoLink == null)
                return scalesList;

            foreach (var link in Photo.PhotoLink)
            {
                string scale = "";
                string filename = "";
                var uri = new Uri(link);
                if (uri.Segments.Count() > 0)
                    filename = uri.Segments[uri.Segments.Count() - 1];

                if (filename.Contains(".jpg"))
                    filename = filename.Replace(".jpg", "");
                if (filename.Contains(".png"))
                    filename = filename.Replace(".png", "");

                if (filename.Contains(Photo.PhotoId))
                    scale = filename.Replace(Photo.PhotoId + "-", "");

                if (scale != "")
                    scalesList.Add(Convert.ToInt16(scale));
            }

            // determine if imagefile is a photo or a profileimage
            PhotoCategory = (scalesList.Count(s => s > 3) > 0) ? PhotoCategories.FriendicaProfileImage : PhotoCategories.FriendicaPhoto;

            return scalesList.OrderByDescending(scale => scale).ToList();
        }


        public async Task<ImageSource> GetImageSourceFromPhotoIdAndScaleAsync(string scale)
        {
            // called when initial loading photolist, re-load photolist, but also when user flips to another photo
            // or when user open a photo in fullview (later two when local saving is disabled or limited)

            ImageSource imgSource = null;
            var filename = BuildFilename();
            if (filename == null)
                return imgSource;

            if (App.Settings.SaveLocalAllowed == "false" ||     // don't save if user has not allowed it
                (scale == "0" && !App.Settings.SaveFullsizePhotosAllowed)) // don't save when user only allowed small images to be saved
            {
                var folder = ApplicationData.Current.TemporaryFolder;
                var photoData = await LoadImageFromServerAsync(scale);

                if (photoData == null || photoData.PhotoData == null)
                    return imgSource;

                var file = await SavePhotoDataToFileAsync(photoData, folder, filename);

                if (file != null)
                {
                    //read date when photo was taken
                    await GetDateTakenFromImageAsync(file);
                    await file.DeleteAsync();
                }

                // create ImageSource from loaded photo data
                imgSource = await ConvertPhotoDataAsync(photoData);
            } 
            else
            {
                var folder = await GetScaleFolderAsync(scale);

                // check if we can get an existing file
                var file = await folder.TryGetItemAsync(filename) as StorageFile;

                // no file existing, so load data from server and save it into LocalCache folder (we don't want the photos in Onecloud backup)                
                if (file == null)
                {
                    var photoData = await LoadImageFromServerAsync(scale);

                    if (photoData == null || photoData.PhotoData == null)
                        return imgSource;


                    file = await SavePhotoDataToFileAsync(photoData, folder, filename);
                }

                // set ImageSource to the file (the existing one or the newly saved one)
                var bitmapSource = new BitmapImage(new Uri(file.Path, UriKind.RelativeOrAbsolute));
                bitmapSource.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                imgSource = bitmapSource;

                // read date when photo was taken
                await GetDateTakenFromImageAsync(file);
            }

            return imgSource;
        }

        public async Task<WriteableBitmap> GetWriteableBitmapFromStorageFileAsync(StorageFile file)
        {
            if (file != null)
            {
                WriteableBitmap wb = new WriteableBitmap(1, 1);
                using (IRandomAccessStream inputStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    wb = await wb.FromStream(inputStream);
                }
                return wb;
            }
            return null;
        }

        public async Task<WriteableBitmap> GetWriteableBitmapFromPhotoIdAndScaleAsync(string scale)
        {
            // called when initial loading photolist, re-load photolist, but also when user flips to another photo
            // or when user open a photo in fullview (later two when local saving is disabled or limited)

            WriteableBitmap wb = new WriteableBitmap(1, 1);
            var filename = BuildFilename();
            if (filename == null)
                return null;

            if (App.Settings.SaveLocalAllowed == "false" ||     // don't save if user has not allowed it
                (scale == "0" && !App.Settings.SaveFullsizePhotosAllowed)) // don't save when user only allowed small images to be saved
            {
                var folder = ApplicationData.Current.TemporaryFolder;
                var photoData = await LoadImageFromServerAsync(scale);

                if (photoData == null || photoData.PhotoData == null)
                    return wb;

                var file = await SavePhotoDataToFileAsync(photoData, folder, filename);

                if (file != null)
                {
                    using (IRandomAccessStream inputStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        wb = await wb.FromStream(inputStream);
                    }
                    await file.DeleteAsync();
                }
            }
            else
            {
                var folder = await GetScaleFolderAsync(scale);

                // check if we can get an existing file
                var file = await folder.TryGetItemAsync(filename) as StorageFile;

                // no file existing, so load data from server and save it into LocalCache folder (we don't want the photos in Onecloud backup)                
                if (file == null)
                {
                    var photoData = await LoadImageFromServerAsync(scale);

                    if (photoData == null || photoData.PhotoData == null)
                        return wb;

                    file = await SavePhotoDataToFileAsync(photoData, folder, filename);
                }

                // set ImageSource to the file (the existing one or the newly saved one)
                using (IRandomAccessStream inputStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    wb = await wb.FromStream(inputStream);
                }
            }

            return wb;
        }

        public async Task DeleteAllScalesFromDevice()
        {
            await DeleteScaleFromDevice("0");
            await DeleteScaleFromDevice("1");
            await DeleteScaleFromDevice("2");
            await DeleteScaleFromDevice("4");
            await DeleteScaleFromDevice("5");
            await DeleteScaleFromDevice("6");
        }

        private async Task DeleteScaleFromDevice(string scale)
        {
            var filename = BuildFilename();

            if (scale == "false")
                scale = "0";

            var folder = await GetScaleFolderAsync(scale);

            // check if we can get an existing file
            var file = await folder.TryGetItemAsync(filename) as StorageFile;

            if (file != null)
            {
                await file.DeleteAsync();
            }
        }


        public async void LoadMediumSize()
        {
            if (!MediumSizeLoaded && Photo != null)
            {
                IsLoadingMediumSize = true;
                MediumSizeData = await GetImageSourceFromPhotoIdAndScaleAsync("1");
                MediumSizeLoaded = true;
                IsLoadingMediumSize = false;
            }
        }


        public async void LoadFullSize()
        {
            if (!FullSizeLoaded && Photo != null)
            {
                IsLoadingFullSize = true;
                FullSizeData = await GetImageSourceFromPhotoIdAndScaleAsync("0");
                FullSizeLoaded = true;
                IsLoadingFullSize = false;
            }
        }


        private async Task GetDateTakenFromImageAsync(StorageFile file)
        {
            var prop = await file.Properties.GetImagePropertiesAsync();
            var proptest = await file.Properties.GetDocumentPropertiesAsync();
            var comment = proptest.Comment;
            _photoDateShotDateTime = prop.DateTaken.DateTime;

            // check on year as the "empty DateTaken" is interpreted as 1st January 1601
            if (_photoDateShotDateTime.Year < 1900)
                PhotoDateShotLocalized = "---";
            else
                PhotoDateShotLocalized = _photoDateShotDateTime.ToString("d") + " " + _photoDateShotDateTime.ToString("t");
        }


        private async Task GetCommentFromImageAsync(StorageFile file)
        {
            var prop = await file.Properties.GetDocumentPropertiesAsync();
            Photo.PhotoDesc = prop.Comment;
            NewPhotoDesc = prop.Comment;
        }

        private async Task<StorageFolder> GetScaleFolderAsync(string scale)
        {
            scale = (scale == "false") ? "0" : scale;

            StorageFolder cacheBaseFolder = ApplicationData.Current.LocalCacheFolder;
            var folder = await cacheBaseFolder.CreateFolderAsync(scale, CreationCollisionOption.OpenIfExists);
            return folder;
        }

        private string BuildFilename()
        {
            switch (Photo.PhotoType)
            {
                case "image/jpeg":
                    return Photo.PhotoId + ".jpg";
                case "image/png":
                    return Photo.PhotoId + ".png";
                default:
                    return null;
            }
        }


        private async Task<StorageFile> SavePhotoDataToFileAsync(FriendicaPhoto photo, StorageFolder folder, string filename)
        {
            var file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            byte[] bytes = Convert.FromBase64String(photo.PhotoData);

            using (FileStream fileStream = new FileStream(file.Path, FileMode.Create, FileAccess.Write))
            {
                // Write the data to the file, byte by byte.
                for (int i = 0; i < bytes.Length; i++)
                {
                    fileStream.WriteByte(bytes[i]);
                }
            }
            return file;
        }


        private async Task<ImageSource> ConvertPhotoDataAsync(FriendicaPhoto data)
        {
            if (data == null || data.PhotoData == null)
                return null;

            byte[] bytes = Convert.FromBase64String(data.PhotoData);
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                // Übertrage base64-Stream im ms in ein BitmapImage übergebe dies an das Image im RichTextBox-Control
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                IRandomAccessStream a1 = await ConvertToRandomAccessStreamAsync(ms);
                bitmapImage.SetSource(a1);
                return bitmapImage;
            }
        }

        public async Task<byte[]> GetPhotoDataForNewUpload()
        {
            // if user has only changed the acl settings, we do not have new photo data, so load from PhotoData
            if (_newPhotoData != null)
                return _newPhotoData;
            else if (Photo != null && Photo.PhotoData != null)
                return Convert.FromBase64String(Photo.PhotoData);
            else if (FullSizeData != null && FullSizeData.GetType() == typeof(BitmapImage))
            {
                var wb = await GetWriteableBitmapFromPhotoIdAndScaleAsync("0");
                Stream pixelStream = wb.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                var folder = ApplicationData.Current.TemporaryFolder;
                var file = await folder.CreateFileAsync("temp.jpg", CreationCollisionOption.ReplaceExisting);
                byte[] bytes;
                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)wb.PixelWidth, (uint)wb.PixelHeight, 96.0, 96.0, pixels);
                    await encoder.FlushAsync();
                    stream.Seek(0);

                    var reader = new DataReader(stream);
                    bytes = new byte[stream.Size];
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(bytes);
                }
                await file.DeleteAsync();
                return bytes;
            }
            else
                return null;
        }

        public async Task<IRandomAccessStream> ConvertToRandomAccessStreamAsync(MemoryStream memoryStream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();
            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = Task.Factory.StartNew(() => dw.WriteBytes(memoryStream.ToArray()));
            await task;
            await dw.StoreAsync();
            await outputStream.FlushAsync();
            return randomAccessStream;
        }

    }
}
