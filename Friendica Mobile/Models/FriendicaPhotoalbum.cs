using BackgroundTasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.UI.Xaml.Documents;

namespace Friendica_Mobile.Models
{
    public class FriendicaPhotoalbum : BindableClass
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public event EventHandler PrintButtonClicked;
        public event EventHandler LastPhotoDeleted;
        public event EventHandler NewProfileimageRequested;
        public event EventHandler MovePhotoToAlbumRequested;
        public event EventHandler CheckLoadingStatusRequested;

        // list of the photos in the album
        private ObservableCollection<FriendicaPhotoExtended> _photosInAlbum;
        public ObservableCollection<FriendicaPhotoExtended> PhotosInAlbum
        {
            get { return _photosInAlbum; }
            set { _photosInAlbum = value;
                OnPropertyChanged("PhotosInAlbum");
            }
        }

        // selected photo
        private FriendicaPhotoExtended _selectedPhoto;
        public FriendicaPhotoExtended SelectedPhoto
        {
            get { return _selectedPhoto; }
            set {
                // deactivate event handlers on old selected photo
                if (SelectedPhoto != null)
                {
                    SelectedPhoto.PrintButtonClicked -= SelectedPhoto_PrintButtonClicked;
                    SelectedPhoto.PhotoDeleted -= SelectedPhoto_PhotoDeleted;
                    SelectedPhoto.NewProfileimageRequested -= SelectedPhoto_NewProfileimageRequested;
                    SelectedPhoto.UpdateAsNewUploadRequested -= SelectedPhoto_UpdateAsNewUploadRequested;
                    SelectedPhoto.MovePhotoToAlbumRequested -= SelectedPhoto_MovePhotoToAlbumRequested;
                    SelectedPhoto.CheckLoadingStatusRequested -= SelectedPhoto_CheckLoadingStatusRequested;
                }
                _selectedPhoto = value;
                if (SelectedPhoto != null)
                {
                    SelectedPhoto.PrintButtonClicked += SelectedPhoto_PrintButtonClicked;
                    SelectedPhoto.PhotoDeleted += SelectedPhoto_PhotoDeleted;
                    SelectedPhoto.NewProfileimageRequested += SelectedPhoto_NewProfileimageRequested;
                    SelectedPhoto.UpdateAsNewUploadRequested += SelectedPhoto_UpdateAsNewUploadRequested;
                    SelectedPhoto.MovePhotoToAlbumRequested += SelectedPhoto_MovePhotoToAlbumRequested;
                    SelectedPhoto.CheckLoadingStatusRequested += SelectedPhoto_CheckLoadingStatusRequested;
                    SelectedPhoto.MovePhotoToAlbumCommand.RaiseCanExecuteChanged();
                }
                OnPropertyChanged("SelectedPhoto");
            }
        }

        private void SelectedPhoto_CheckLoadingStatusRequested(object sender, EventArgs e)
        {
            CheckLoadingStatusRequested?.Invoke(sender, EventArgs.Empty);
        }

        private void SelectedPhoto_MovePhotoToAlbumRequested(object sender, EventArgs e)
        {
            MovePhotoToAlbumRequested?.Invoke(sender, EventArgs.Empty);
        }

        private async void SelectedPhoto_UpdateAsNewUploadRequested(object sender, EventArgs e)
        {
            var photoOriginal = sender as FriendicaPhotoExtended;
            if (photoOriginal == null)
            {
                // "unexpected error on uploading, changing or deleting of a photo"
                string errorMsg = String.Format(loader.GetString("messageDialogPhotosUploadingUnknownError"), "");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                return;
            }
            photoOriginal.IsServerOperationRunning = true;

            // copy data from original photo to new photo
            var photoNew = new FriendicaPhotoExtended();
            photoNew.PhotoCategory = photoOriginal.PhotoCategory;
            photoNew.Photo.PhotoAlbum = photoOriginal.Photo.PhotoAlbum;
            photoNew.NewPhotoAllowCid = photoOriginal.NewPhotoAllowCid;
            photoNew.NewPhotoAllowGid = photoOriginal.NewPhotoAllowGid;
            photoNew.NewPhotoDenyCid = photoOriginal.NewPhotoDenyCid;
            photoNew.NewPhotoDenyGid = photoOriginal.NewPhotoDenyGid;
            photoNew.NewPhotoDesc = photoOriginal.NewPhotoDesc;
            photoNew.MediumSizeData = photoOriginal.MediumSizeData;
            photoNew.ThumbSizeData = photoOriginal.ThumbSizeData;
            photoNew.IsServerOperationRunning = true;

            // get pixel data for uploading the image - cancel if we cannot get the photo data
            var bytes = await photoOriginal.GetPhotoDataForNewUpload();
            if (bytes == null || bytes.Length == 0)
            {
                // "unexpected error on uploading, changing or deleting of a photo"
                string errorMsg = String.Format(loader.GetString("messageDialogPhotosUploadingUnknownError"), "");
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
                return;
            }
            photoNew.SaveChangedPhotoData(bytes);

            // start the upload of the new photo 
            var result = await photoNew.UploadNewPhotoToServerAsync();

            // insert new photo into album - user see for a short time both images
            var indexOld = PhotosInAlbum.IndexOf(photoOriginal);
            indexOld = (indexOld < 0) ? 0 : indexOld;
            PhotosInAlbum.Insert(indexOld, photoNew);

            // if upload was successful, delete original image from server
            switch (result)
            {
                case HttpRequests.GetFriendicaPhotos.PhotoErrors.OK:
                    await photoOriginal.DeletePhotoFromServerAsync();
                    break;
                default:
                    // "unexpected error on uploading, changing or deleting of a photo"
                    string errorMsg = String.Format(loader.GetString("messageDialogPhotosUploadingUnknownError"), "");
                    var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                    await dialog.ShowDialog(0, 0);
                    break;
            }
        }

        private void SelectedPhoto_NewProfileimageRequested(object sender, EventArgs e)
        {
            NewProfileimageRequested?.Invoke(sender, EventArgs.Empty);
        }

        // name of the album
        private string _albumname;
        public string Albumname
        {
            get { return _albumname; }
            set { _albumname = value;
                OnPropertyChanged("Albumname"); }
        }

        // has the changed albumname if user has edited the name in the view
        private string _albumnameNew;
        public string AlbumnameNew
        {
            get { return _albumnameNew; }
            set { _albumnameNew = value;
                OnPropertyChanged("AlbumnameNew"); }
        }

        // get the list of selectable albums for moving a photo into
        private List<string> _selectableAlbums;
        public List<string> SelectableAlbums
        {
            get { return _selectableAlbums; }
            set
            {
                _selectableAlbums = value;
                if (value != null && PhotosInAlbum != null)
                {
                    foreach (var photo in PhotosInAlbum)
                    {
                        photo.SelectableAlbums = new ObservableCollection<string>();
                        foreach (var album in value)
                            photo.SelectableAlbums.Add(album);
                    }
                }
            }
        }


        // trigger display of a new album for interaction of user
        private bool _newAlbumVisible;
        public bool NewAlbumVisible
        {
            get { return _newAlbumVisible; }
            set { _newAlbumVisible = value;
                OnPropertyChanged("NewAlbumVisible");
            }
        }

        // trigger display of the 1st image in the "up to three image stack" of an album
        private bool _photo1Visible;
        public bool Photo1Visible
        {
            get { return _photo1Visible; }
            set { _photo1Visible = value;
                OnPropertyChanged("Photo1Visible");
            }
        }

        // trigger display of the 2nd image in the "up to three image stack" of an album
        private bool _photo2Visible;
        public bool Photo2Visible
        {
            get { return _photo2Visible; }
            set { _photo2Visible = value;
                OnPropertyChanged("Photo2Visible");
            }
        }

        // trigger display of the 3rd image in the "up to three image stack" of an album
        private bool _photo3Visible;
        public bool Photo3Visible
        {
            get { return _photo3Visible; }
            set { _photo3Visible = value;
                OnPropertyChanged("Photo3Visible");
            }
        }

        // image source for the 1st image in the "up to three image stack" of an album
        private FriendicaPhotoExtended _stackPhoto1;
        public FriendicaPhotoExtended StackPhoto1
        {
            get { return _stackPhoto1; }
            set { _stackPhoto1 = value;
                OnPropertyChanged("StackPhoto1");
            }
        }

        // image source for the 2nd image in the "up to three image stack" of an album
        private FriendicaPhotoExtended _stackPhoto2;
        public FriendicaPhotoExtended StackPhoto2
        {
            get { return _stackPhoto2; }
            set { _stackPhoto2 = value;
                OnPropertyChanged("StackPhoto2");
            }
        }

        // image source for the 3rd image in the "up to three image stack" of an album
        private FriendicaPhotoExtended _stackPhoto3;
        public FriendicaPhotoExtended StackPhoto3
        {
            get { return _stackPhoto3; }
            set { _stackPhoto3 = value;
                OnPropertyChanged("StackPhoto3");
            }
        }




        public FriendicaPhotoalbum()
        {
            PhotosInAlbum = new ObservableCollection<FriendicaPhotoExtended>();
        }

        private void SelectedPhoto_PrintButtonClicked(object sender, EventArgs e)
        {
            if (PrintButtonClicked != null)
                PrintButtonClicked.Invoke(this, EventArgs.Empty);
        }

        private void SelectedPhoto_PhotoDeleted(object sender, EventArgs e)
        {
            var photo = sender as FriendicaPhotoExtended;

            // remove photo from album
            if (SelectedPhoto == photo)
            {
                var index = PhotosInAlbum.IndexOf(photo);
                if (index > 0)
                    SelectedPhoto = PhotosInAlbum[index - 1];
                else
                    SelectedPhoto = PhotosInAlbum[0];
            }
            PhotosInAlbum.Remove(photo);
            
            // trigger event when we delete the last photo of an album
            if (PhotosInAlbum.Count == 0)
                LastPhotoDeleted?.Invoke(this, EventArgs.Empty);

            // recreate the 3 images for the flip animation
            SetPhotosForAlbumView();
        }

        public void SetPhotosForAlbumView()
        {
            // fallback if collection is null, which should not occur
            if (PhotosInAlbum == null)
                return;

            Photo1Visible = false;
            Photo2Visible = false;
            Photo3Visible = false;

            if (PhotosInAlbum.Count > 0)
            {
                Photo1Visible = true;
                StackPhoto1 = PhotosInAlbum[0];
            }
            if (PhotosInAlbum.Count > 1)
            {
                Photo2Visible = true;
                StackPhoto2 = PhotosInAlbum[1];
            }
            if (PhotosInAlbum.Count > 2)
            {
                // if we have 3 or more sort list of photos randomly and take first three entries 
                // random does not make sense with only 3 but code can be reused
                Photo3Visible = true;
                var randomPhotos = RandomImageSelector(PhotosInAlbum);
                StackPhoto1 = randomPhotos[0];
                StackPhoto2 = randomPhotos[1];
                StackPhoto3 = randomPhotos[2];
            }
        }

        private List<FriendicaPhotoExtended> RandomImageSelector(ObservableCollection<FriendicaPhotoExtended> photosInAlbum)
        {
            var list = new List<FriendicaPhotoExtended>();

            return list = (from photo in photosInAlbum
                           orderby Guid.NewGuid()
                           select photo).ToList();
        }

    }
}
