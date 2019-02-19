using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Imaging;

namespace Friendica_Mobile.UWP
{
    class clsConvertDroppedContent
    {
        // input parameter - contains dropped content from DropArea
        private DataPackageView _dataPackage;

        // output parameter - contains converted data in FriendicaNewPost (which will be transferred to A0_NewPost.xaml)
        private FriendicaNewPost _post;
        public FriendicaNewPost Post
        {
            get { return _post; }
            set { _post = value; }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        private string _imageFilename;
        public string ImageFilename
        {
            get { return _imageFilename; }
            set { _imageFilename = value; }
        }

        private double _imageLatitude;
        public double ImageLatitude
        {
            get { return _imageLatitude; }
            set { _imageLatitude = value; }
        }

        private double _imageLongitude;
        public double ImageLongitude
        {
            get { return _imageLongitude; }
            set { _imageLongitude = value; }
        }

        private Uri _weblink;
        public Uri Weblink
        {
            get { return _weblink; }
            set { _weblink = value; }
        }


        // events of the class
        public event EventHandler MoreThanOneImageDropped;
        public event EventHandler ConversionErrorCreated;


        public clsConvertDroppedContent()
        {
            Post = new FriendicaNewPost();
        }

        public async Task<FriendicaNewPost> ConvertDroppedContent(DataPackageView content)
        {
            bool bitmapConverted = false;
            _dataPackage = content;
            var storageItemsConverted = await ConvertStorageItemsAsync();
            if (!storageItemsConverted)
                bitmapConverted = await ConvertBitmapAsync();
            // currently not implemented to retrieve copied RTF or HTML into preformatted text in rebNewPost
            //var RtfConverted = await ConvertRtfAsync();
            bool RtfConverted = false;
            //var HtmlConverted = await ConvertHtmlAsync();
            bool HtmlConverted = false;
            var TextConverted = await ConvertTextAsync();
            var LinkConverted = await ConvertLinkAsync();

            if (!storageItemsConverted && !bitmapConverted && !RtfConverted && !HtmlConverted && !TextConverted && !LinkConverted)
            {
                if (ConversionErrorCreated != null)
                    ConversionErrorCreated(this, EventArgs.Empty);
                return null;
            }
            else
                return Post;
        }

        private async Task<bool> ConvertStorageItemsAsync()
        {
            bool isSuccess = false;
            bool isMoreThanOneImage = false;
            bool hasStorageItems = _dataPackage.Contains(StandardDataFormats.StorageItems);
            if (hasStorageItems)
            {
                var files = await _dataPackage.GetStorageItemsAsync();
                foreach (StorageFile file in files)
                {
                    try
                    {
                        if (file.ContentType == "image/png" || file.ContentType == "image/jpeg" || file.ContentType == "image/bmp")
                        {
                            if (Image == null)
                            {
                                // load image for showing Image in A0_NewPost.xaml and the coordinates if available
                                var imageProperties = await file.Properties.GetImagePropertiesAsync();
                                if (imageProperties.Latitude != null)
                                    ImageLatitude = (double)imageProperties.Latitude;
                                else
                                    ImageLatitude = 0.0;
                                if (imageProperties.Longitude != null)
                                    ImageLongitude = (double)imageProperties.Longitude;
                                else
                                    ImageLongitude = 0.0;
                                BitmapImage bi = new BitmapImage();
                                bi.SetSource(await file.OpenAsync(FileAccessMode.Read));
                                ImageFilename = file.Name;
                                Image = bi;

                                // load image into post
                                using (IRandomAccessStream inputStream = await file.OpenAsync(FileAccessMode.Read))
                                {
                                    var reader = new DataReader(inputStream);
                                    var bytes = new byte[inputStream.Size];
                                    await reader.LoadAsync((uint)inputStream.Size);
                                    reader.ReadBytes(bytes);
                                    Post.NewPostMedia = bytes;
                                }
                                isSuccess = true;
                            }
                            else
                            {
                                isMoreThanOneImage = true;
                            }
                        }
                    }
                    catch
                    {
                        isSuccess = false;
                    }
                }

                // raise message to user if more than one image was pasted and give info which filename has been used
                if (isMoreThanOneImage)
                {
                    // we had more than one image in the drop content
                    if (MoreThanOneImageDropped != null)
                        MoreThanOneImageDropped(this, EventArgs.Empty);
                }

                return isSuccess;
            }
            else
                return false;
        }

        private async Task<bool> ConvertBitmapAsync()
        {
            bool hasBitmap = _dataPackage.Contains(StandardDataFormats.Bitmap);
            if (hasBitmap)
            {
                try
                {
                    var bitmap = await _dataPackage.GetBitmapAsync();
                    
                    // if the dropped bitmap is a png we need to convert it to jpg as at least my Friendica instance was not able
                    // to handle png (returned "Unable to process image." as php5-Imagick is not installed)
                    WriteableBitmap wb = new WriteableBitmap(50, 50);
                    using (IRandomAccessStream inputStream = await bitmap.OpenReadAsync())
                    {
                        await wb.SetSourceAsync(inputStream);
                    }

                    // create a temporary folder and a temporary jpg file to store pixels there
                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    await localFolder.CreateFolderAsync("TempImages", CreationCollisionOption.OpenIfExists);
                    var file = await localFolder.CreateFileAsync("TempImages\\temp.jpg", CreationCollisionOption.ReplaceExisting);

                    using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        // create jpg bitmap from the provided bitmap data (we do this in any case even if provided bitmap is already jpg)
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                        Stream pixelStream = wb.PixelBuffer.AsStream();
                        byte[] pixels = new byte[pixelStream.Length];
                        await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)wb.PixelWidth, (uint)wb.PixelHeight, 96.0, 96.0, pixels);
                        await encoder.FlushAsync();
                    }

                    // load image for showing Image in A0_NewPost.xaml and the coordinates if available
                    var imageProperties = await file.Properties.GetImagePropertiesAsync();
                    if (imageProperties.Latitude != null)
                        ImageLatitude = (double)imageProperties.Latitude;
                    else
                        ImageLatitude = 0.0;
                    if (imageProperties.Longitude != null)
                        ImageLongitude = (double)imageProperties.Longitude;
                    else
                        ImageLongitude = 0.0;
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(await file.OpenAsync(FileAccessMode.Read));
                    ImageFilename = file.Name;
                    Image = bi;

                    // load image into post
                    using (IRandomAccessStream inputStream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        var reader = new DataReader(inputStream);
                        var bytes = new byte[inputStream.Size];
                        await reader.LoadAsync((uint)inputStream.Size);
                        reader.ReadBytes(bytes);
                        Post.NewPostMedia = bytes;
                    }


                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }


        private async Task<bool> ConvertRtfAsync()
        {
            bool hasRtf = _dataPackage.Contains(StandardDataFormats.Rtf);
            if (hasRtf)
            {
                var rtf = await _dataPackage.GetRtfAsync();

                //RtfText.SetText(TextSetOptions.FormatRtf, rtf);
                return true;
            }
            else
                return false;
        }

        private async Task<bool> ConvertHtmlAsync()
        {
            bool hasHtml = _dataPackage.Contains(StandardDataFormats.Html);
            if (hasHtml)
            {
                var Html = await _dataPackage.GetHtmlFormatAsync();
                var htmlConvert = new clsHtmlToRichTextBlock(Html);

                Post.NewPostStatus = Html;
                return true;
            }
            else
                return false;
        }

        private async Task<bool> ConvertTextAsync()
        {
            bool hasText = _dataPackage.Contains(StandardDataFormats.Text);
            if (hasText)
            {
                var text = await _dataPackage.GetTextAsync();
                Post.NewPostStatus = text;
                return true;
            }
            else
                return false;
        }

        private async Task<bool> ConvertLinkAsync()
        {
            bool hasWeblink = _dataPackage.Contains(StandardDataFormats.WebLink);
            if (hasWeblink)
            {
                Weblink = await _dataPackage.GetWebLinkAsync();
                return true;
            }
            return false;
        }

    }
}