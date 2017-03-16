using BackgroundTasks;
using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
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
                    if (value.PhotoEdited != null && value.PhotoEdited != "")
                    {
                        _photoEditedDateTime = DateTime.ParseExact(value.PhotoEdited, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        PhotoEditedLocalized = _photoEditedDateTime.ToString("d") + " " + _photoEditedDateTime.ToString("t");
                    }
            }
        }


        // change Photo.PhotoEdited to a localized version of the text
        private DateTime _photoEditedDateTime;
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
            }
        }

        // define the color in which the ACL button is shown for the photo (red = visible for all, standard = closed audience)
        private Brush _publiclyVisibleColor;
        public Brush PubliclyVisibleColor
        {
            get { return _publiclyVisibleColor; }
            set { _publiclyVisibleColor = value; }
        }

        // indicator if there is an existing conversation to this photo (aka someone has commented)
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

        // new description text if user is changing the description
        private string _photoDescNew;
        public string PhotoDescNew
        {
            get { return _photoDescNew; }
            set
            {
                _photoDescNew = value;
                OnPropertyChanged("PhotoDescNew");
            }
        }

        // save type of image 
        private PhotoCategories _photoCategory;
        public PhotoCategories PhotoCategory
        {
            get { return _photoCategory; }
            set { _photoCategory = value; }
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

        // indicator to avoid that user can change description or move image before upload has been finished
        private bool _isServerOperationPending;
        public bool IsServerOperationPending
        {
            get { return _isServerOperationPending; }
            set { _isServerOperationPending = value;
                EditPhotoDescriptionCommand.RaiseCanExecuteChanged();
            }
        }


        // property for holding data of a new image
        private byte[] _newPhotoData;


        // local indicators if different processes are already running
        private bool _isSavingChangedPhotoDescription;
        
        // eventhandlers
        public event EventHandler PrintButtonClicked;
        public event EventHandler NewPhotoUploaded;

        private string _sourcePath;
        public string SourcePath
        {
            get { return _sourcePath; }
            set { _sourcePath = value;
            }
        }


        #region commands


        // show access rights for the image in a separate window
        Mvvm.Command _editPhotoDescriptionCommand;
        public Mvvm.Command EditPhotoDescriptionCommand { get { return _editPhotoDescriptionCommand ?? (_editPhotoDescriptionCommand = new Mvvm.Command(ExecuteEditPhotoDescription, CanEditPhotoDescription)); } }
        private bool CanEditPhotoDescription()
        {
            if (_isServerOperationPending)
                return false;
            else 
                return true;
        }

        private void ExecuteEditPhotoDescription()
        {
            PhotoDescNew = Photo.PhotoDesc;
            PhotoEditingEnabled = true;
        }


        // save changed albumname to server
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

            // check if text has not changed, then cancel and go back to editing
            if (Photo.PhotoDesc == PhotoDescNew)
            {
                string errorMsg = loader.GetString("messageDialogPhotosNewPhotoDescriptionUnchanged");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                _isSavingChangedPhotoDescription = false;
                return;
            }

            // TODO: Änderung durchführen
            // now we are in the loop to change the description on server

            var errorMsg2 = String.Format("Bildbeschreibung von '{0}' auf '{1}' ändern.", Photo.PhotoDesc, PhotoDescNew);
            var dialog2 = new MessageDialogMessage(errorMsg2, "", "OK", null);
            await dialog2.ShowDialog(0, 0);
            PhotoEditingEnabled = false;
            _isSavingChangedPhotoDescription = false;
            // TODO: nicht vergessen, die echte Beschreibung nach dem Ändern auf dem Server hier in der App nachziehen
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
            // TODO implement correct function for navigating to page showing the access rights and accepting changes

            string errorMsg = "Zeige separate Seite mit den bestehenden Zugriffsberechtigungen auf dieses Foto.";
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
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
            // TODO implement correct function for navigating to page with the ink canvas and the photo
            // create inkCanvasViewmodel 
            //var inkCanvasVm = new PhotosInkCanvasViewmodel();
            //inkCanvasVm.IsNewImage = false;

            // load the picture info into this vm and transfer it to A5_InkCanvas
            var bitmapImage = MediumSizeData;

            // on Navigating away store PhotoViewmodel into App.PhotosVm
            var frame = App.GetFrameForNavigation();
            frame.Navigate(typeof(A5_InkCanvas), bitmapImage);
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
            // TODO implement correct function for deleting the photo from server

            string errorMsg = "Löscht Foto vom Server, vorher erfolgt noch eine Sicherheitsabfrage";
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }


        // command for creating profile image from current photo
        Mvvm.Command _createProfileimageCommand;
        public Mvvm.Command CreateProfileimageCommand { get { return _createProfileimageCommand ?? (_createProfileimageCommand = new Mvvm.Command(ExecuteCreateProfileimage, CanCreateProfileimage)); } }
        private bool CanCreateProfileimage()
        {
            return true;
        }

        private async void ExecuteCreateProfileimage()
        {
            // TODO implement correct function for cropping photo for a profileimage

            string errorMsg = "Wechselt auf neue Seite, wo User das Rechteck für das Erstellen eines Profilbildes einstellen kann. ";
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
        }


        // command for showing the conversation to a photo (if there is one)
        Mvvm.Command _showPhotoConversationCommand;
        public Mvvm.Command ShowPhotoConversationCommand { get { return _showPhotoConversationCommand ?? (_showPhotoConversationCommand = new Mvvm.Command(ExecuteShowPhotoConversation, CanShowPhotoConversation)); } }
        private bool CanShowPhotoConversation()
        {
            return true;
        }

        private async void ExecuteShowPhotoConversation()
        {
            // TODO implement correct function for displaying the conversation related to a photo

            string errorMsg = "Wechselt auf neue Seite, wo die Kommentare zu einem Foto angezeigt werden. ";

            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
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
        }


        // TODO: load data from server
        // OK Photo.PhotoId, .PhotoAlbum, .PhotoFilename, .PhotoType, .PhotolistThumb kommen bereits vom Photolist-Call
        // OK allerdings hätten wir auch gerne die übrigen Daten: created, edited, [title], desc, height, width, profile, link
        // OK die bekommen wir mit der api/friendica/photo.json?photo_id=xyz
        // OK mit parameter Scale könnte man ein spezielles Format inkl. Base64 daten bekommen
        // OK link enthält dann die Liste der scale url's, daraus muss man die Scales dann extrahieren
        // OK für jeden Scale kann man dann die Daten extra laden (base64-Daten) um diese dann zu speichern (wenn erlaubt) oder als ImageSource zu verwenden
        // OK jetzt haben wir Bilddaten, da kann man ja das originale Aufnahmedatum extrahieren (PhotoDateShotLocalized)
        // TODO: momentan bekommen wir keine Conversations und ACL data mitgeschickt, muss erst in api.php angepasst werden, dann hier weiterverarbeiten

        public async Task GetPhotoFromServer()
        {
            // load basis photo details (without data for scales)
            var getHttpLoadPhoto = new GetFriendicaPhotos();
            await getHttpLoadPhoto.LoadPhotoFromServerAsync(Photo.PhotoId);
            Photo = getHttpLoadPhoto.PhotoReturned;

            WorkOnPhotoScales();
        }

        private async void WorkOnPhotoScales()
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
                PhotoCategory = FriendicaPhotoExtended.PhotoCategories.FriendicaPhoto;
                Photo.PhotoAlbum = album;
            }
            await GetDateTakenFromImageAsync(NewPhotoStorageFile);

            // set the bitmapdata for fullsize/medium/thumb with the local image until upload to server has been finished
            var bitmapData = new BitmapImage(new Uri(NewPhotoStorageFile.Path, UriKind.RelativeOrAbsolute));
            ThumbSizeData = bitmapData;
            MediumSizeData = bitmapData;
            FullSizeData = bitmapData;
            IsLoadingFullSize = true;
            IsLoadingMediumSize = true;
            IsPubliclyVisible = false;
        }

        public async Task UploadNewPhotoToServerAsync()
        {
            // set indicator to avoid the user working on photo description etc.
            IsServerOperationPending = true;

            // upload file to server
            var postHttpUploadNewPhoto = new GetFriendicaPhotos();
            await postHttpUploadNewPhoto.PostNewPhotoAsync(PrepareNewPhotoData(), NewPhotoStorageFile.ContentType, NewPhotoStorageFile.Name);

            switch (postHttpUploadNewPhoto.ErrorPhotoFriendica)
            {
                case GetFriendicaPhotos.PhotoErrors.OK:
                    // react on positive upload (returned json with photo details
                    Photo = postHttpUploadNewPhoto.PhotoReturned;

                    // load scales of photo to device if allowed
                    WorkOnPhotoScales();

                    // set view to this actual image
                    NewPhotoUploaded?.Invoke(this, EventArgs.Empty);
                    break;
                case GetFriendicaPhotos.PhotoErrors.ServerNotAnswered:
                    // ask for retry
                    var errorMsgRetry = String.Format(loader.GetString("messageDialogPhotoUploadingNoReaction"), NewPhotoStorageFile.Name);
                    var dialogRetry = new MessageDialogMessage(errorMsgRetry, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                    await dialogRetry.ShowDialog(0, 1);

                    if (dialogRetry.Result == 0)
                        await postHttpUploadNewPhoto.PostNewPhotoAsync(PrepareNewPhotoData(), NewPhotoStorageFile.ContentType, NewPhotoStorageFile.Name);
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
                case GetFriendicaPhotos.PhotoErrors.NoAlbumnameSpecified:
                case GetFriendicaPhotos.PhotoErrors.NoPhotoFound:
                    // message to user that we had an unexpected error
                    var errorMsgProblem = String.Format(loader.GetString("messageDialogPhotosUploadingUnknownError"), NewPhotoStorageFile.Name);
                    dialogError = new MessageDialogMessage(errorMsgProblem, "", "OK", null);
                    await dialogError.ShowDialog(0, 0);
                    break;
            }
            IsServerOperationPending = false;
            // TODO: react if there was an error - do we keep the photo in the album, delete it from Photo collection?
        }


        private Dictionary<string, object> PrepareNewPhotoData()
        {
            var Parameters = new Dictionary<string, object>();
            Parameters.Add("media", _newPhotoData);
            Parameters.Add("album", Photo.PhotoAlbum);
            // TODO: get correct self-cid for this dictionary
            Parameters.Add("allow_cid", "<1>");
            if (Photo.PhotoDesc != null)
                Parameters.Add("desc", Photo.PhotoDesc);
            // TODO: deny_cid, allow_gid, deny_gid optional

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
                imgSource = new BitmapImage(new Uri(file.Path, UriKind.RelativeOrAbsolute));

                // read date when photo was taken
                await GetDateTakenFromImageAsync(file);
            }

            return imgSource;
        }

        public void DeleteAllScalesFromDevice()
        {
            DeleteScaleFromDevice("0");
            DeleteScaleFromDevice("1");
            DeleteScaleFromDevice("2");
            DeleteScaleFromDevice("4");
            DeleteScaleFromDevice("5");
            DeleteScaleFromDevice("6");
        }

        private async void DeleteScaleFromDevice(string scale)
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
            if (!MediumSizeLoaded)
            {
                IsLoadingMediumSize = true;
                MediumSizeData = await GetImageSourceFromPhotoIdAndScaleAsync("1");
                MediumSizeLoaded = true;
                IsLoadingMediumSize = false;
            }
        }


        public async void LoadFullSize()
        {
            if (!FullSizeLoaded)
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
            _photoDateShotDateTime = prop.DateTaken.DateTime;

            // check on year as the "empty DateTaken" is interpreted as 1st January 1601
            if (_photoDateShotDateTime.Year < 1900)
                PhotoDateShotLocalized = "---";
            else
                PhotoDateShotLocalized = _photoDateShotDateTime.ToString("d") + " " + _photoDateShotDateTime.ToString("t");
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
                IRandomAccessStream a1 = await ConvertToRandomAccessStreamAsync(ms);
                bitmapImage.SetSource(a1);
                return bitmapImage;
            }
        }


        public static async Task<IRandomAccessStream> ConvertToRandomAccessStreamAsync(MemoryStream memoryStream)
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
