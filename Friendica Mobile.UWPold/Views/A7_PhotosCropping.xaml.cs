using System;
using Friendica_Mobile.UWP.Triggers;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Web.Http;
using Windows.UI.Xaml.Media;

namespace Friendica_Mobile.UWP.Views
{
    public sealed partial class A7_PhotosCropping : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public A7_PhotosCropping()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);
            bottomAppBar.DataContext = App.Settings;
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.SourcePageType.Name == "A7_PhotosCropping")
            {
                panelIndicatorLoading.Visibility = Visibility.Visible;

                var photo = App.PhotosVm.SelectedPhotoalbum.SelectedPhoto;
                WriteableBitmap wb;
                // when coming from a new photo for profileimage we do have a Storagefile
                if (photo.NewPhotoStorageFile != null)
                {
                    var file = photo.NewPhotoStorageFile;
                    wb = new WriteableBitmap(1, 1);
                    await wb.LoadAsync(file);
                }
                else
                {
                    // when sample images, we need to load the writablebitmap stream from source again
                    if (photo.PhotolistThumb != null && photo.PhotolistThumb.Contains("images.metmuseum.org"))
                    {
                        var httpClient = new HttpClient();
                        var buffer = await httpClient.GetBufferAsync(new Uri(photo.PhotolistThumb));
                        var writeableBitmap = new WriteableBitmap(1, 1);

                        using (var stream = buffer.AsStream())
                        {
                            await writeableBitmap.SetSourceAsync(stream.AsRandomAccessStream());
                        }
                        wb = writeableBitmap;
                    }
                    else
                    {
                        // when coming from an existing photo we need to get the WritableBitmap for this activity
                        wb = await photo.GetWriteableBitmapFromPhotoIdAndScaleAsync("0");
                    }
                }
                this.PhotoCropper.SourceImage = wb;
                this.PhotoCropper.UpdateLayout();
                App.Settings.HideNavigationElements = true;
                panelIndicatorLoading.Visibility = Visibility.Collapsed;            
            }
            base.OnNavigatedTo(e);
        }


        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // ask user for confirmation when navigating away
            if (PhotoCropper.HasUnsavedChanges)
            {
                e.Cancel = true;
                // "cropped image will now be saved, sure?"
                string errorMsg = loader.GetString("messageDialogPhotosAskForConfirmationCroppingOk");
                var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                await dialog.ShowDialog(0, 1);

                var frame = App.GetFrameForNavigation();
                // save strokes if user has confirmed with yes
                if (dialog.Result == 0)
                {
                    await SaveCroppedPhotoAsync();
                    frame.GoBack();
                }
            }

            base.OnNavigatingFrom(e);
        }


        private void gridTotalCroppingPage_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            // set limits for image size according to the size of the available space, otherwise pictures will zoom to fill the
            // whole width, this can cause vanishing parts of the image as height is then higher than available space (missing logic in Image.Stretch = Uniform)
            if (gridTotalCroppingPage.ActualHeight > 400)
                PhotoCropper.MaxHeight = 400;
            else
                PhotoCropper.MaxHeight = gridTotalCroppingPage.ActualHeight - 84;
        }

        private async Task SaveCroppedPhotoAsync()
        {
            // retrieve pixels of the cropped picture
            Stream pixelStream = this.PhotoCropper.CroppedImage.PixelBuffer.AsStream();
            byte[] pixels = new byte[pixelStream.Length];
            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

            // save data back to PhotosViewmodel
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.SaveChangedPhotoData(pixels);

            var folder = ApplicationData.Current.TemporaryFolder;
            var file = await folder.CreateFileAsync("cropped.jpg", CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)PhotoCropper.CroppedImage.PixelWidth, (uint)PhotoCropper.CroppedImage.PixelHeight, 96.0, 96.0, pixels);
                await encoder.FlushAsync();
                stream.Seek(0);

                var reader = new DataReader(stream);
                var bytes = new byte[stream.Size];
                await reader.LoadAsync((uint)stream.Size);
                reader.ReadBytes(bytes);
                App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.SaveChangedPhotoData(bytes);
            }

            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.ThumbSizeData = PhotoCropper.CroppedImage;
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.MediumSizeData = PhotoCropper.CroppedImage;
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.FullSizeData = PhotoCropper.CroppedImage;
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.ThumbSizeLoaded = true;
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.MediumSizeLoaded = true;
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.FullSizeLoaded = true;

            this.PhotoCropper.HasUnsavedChanges = false;
        }


        private async void topAppBarPhotosCroppingSave_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await SaveCroppedPhotoAsync();
        }


        private void bottomAppBarPhotosShowPreviews_Click(object sender, RoutedEventArgs e)
        {
            // load cropped images as binding is not working on this elements
            imageFullsizePreviewMobile.Source = this.PhotoCropper.CroppedImage;
            imageMediumPreviewMobile.Source = this.PhotoCropper.CroppedImage;
            imageThumbPreviewMobile.Source = this.PhotoCropper.CroppedImage;

            // show the flyout
            var button = sender as AppBarButton;
            button.Flyout.ShowAt(button);
        }
    }
}
