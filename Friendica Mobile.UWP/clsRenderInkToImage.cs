using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Media.Imaging;

namespace Friendica_Mobile.UWP
{
    class clsRenderInkToImage
    {
        // property for original image
        private WriteableBitmap _originalPhoto;
        private StorageFile _file;
        private bool _isEmptyCanvas;
        private SoftwareBitmap _softwareBitmap;
        // property for ink strokes
        private InkStrokeContainer _inkStrokeContainer;
        // properties for canvas used to draw inks
        private CanvasDevice _device;
        private CanvasRenderTarget _renderStrokes;
        private CanvasRenderTarget _renderTarget;
        private int _originalWidth;
        private int _originalHeight;
        private int _canvasWidth;
        private int _canvasHeight;
        private int _shownWidth;
        private int _shownHeight;

        // provide rendered image as a byte array for uploading
        private byte[] _outputByteArray;
        public byte[] OutputByteArray
        {
            get { return _outputByteArray; }
            set { _outputByteArray = value; }
        }

        // provide rendered image as a writeable bitmap for displaying results before uploading
        private WriteableBitmap _outputWriteableBitmap;
        public WriteableBitmap OutputWriteableBitmap
        {
            get { return _outputWriteableBitmap; }
            set { _outputWriteableBitmap = value; }
        }


        // constructor method reacting on different input types
        public clsRenderInkToImage(object original)
        {
            _device = CanvasDevice.GetSharedDevice();

            // null if we are working in an empty inking area
            if (original == null)
            {
                _isEmptyCanvas = true;
            }
            // a writeablebitmap if user is inking on an existing image (then we can get the WritableBitmap from the server)
            else if (original.GetType() == typeof(WriteableBitmap))
            {
                _originalPhoto = original as WriteableBitmap;
            }
            // a StorageFile if user is inking on a new image from device or from camera (then we only have the storagefile from local device)
            else if (original.GetType() == typeof(StorageFile))
            {
                _file = original as StorageFile;
            }
        }


        public async Task RenderInkToImageAsync(InkStrokeContainer container, int shownWidth, int shownHeight)
        {
            // get input parameters
            _inkStrokeContainer = container;
            _shownWidth = shownWidth;
            _shownHeight = shownHeight;

            // set original size to shown size if we have an empty canvas for inking
            if (_isEmptyCanvas)
            {
                _originalWidth = _shownWidth;
                _originalHeight = _shownHeight;
            }
            else
            {
                // load writablebitmap from file if existing
                if (_file != null)
                {
                    using (var fileStream = await _file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        _originalPhoto = await _originalPhoto.FromStream(fileStream);
                    }
                }

                // get data from transferred original photo
                _originalWidth = _originalPhoto.PixelWidth;
                _originalHeight = _originalPhoto.PixelHeight;
                _softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(_originalPhoto.PixelBuffer, BitmapPixelFormat.Bgra8, _originalWidth, _originalHeight, BitmapAlphaMode.Premultiplied);
            }

            // calculate canvas size
            _canvasWidth = (int)_inkStrokeContainer.BoundingRect.Width + (int)Math.Abs(_inkStrokeContainer.BoundingRect.Left);
            _canvasHeight = (int)_inkStrokeContainer.BoundingRect.Height + (int)Math.Abs(_inkStrokeContainer.BoundingRect.Top);

            // calculate new sizes for ink canvas (resized to original image)
            var xScale = (float)_originalWidth / _shownWidth;
            var yScale = (float)_originalHeight / _shownHeight;
            int newCanvasWidth = (int)(_canvasWidth * xScale);
            int newCanvasHeight = (int)(_canvasHeight * yScale);
            
            // calculate new start points for image in canvas
            uint startPointX = (_inkStrokeContainer.BoundingRect.Left < 0) ? (uint)(Math.Abs(_inkStrokeContainer.BoundingRect.Left) * xScale) : 0;
            uint startPointY = (_inkStrokeContainer.BoundingRect.Top < 0) ? (uint)(Math.Abs(_inkStrokeContainer.BoundingRect.Top) * yScale) : 0;
            
            // resize strokes
            var strokes = _inkStrokeContainer.GetStrokes();
            foreach (var stroke in strokes)
            {
                // change size of the pen tip 
                var drawingAttributes = stroke.DrawingAttributes;
                if (drawingAttributes.PenTip == PenTipShape.Rectangle)
                    drawingAttributes.PenTipTransform = Matrix3x2.CreateScale(xScale, yScale);
                // we cannot adjust PenTipTransform when we have a circle, throws an exception
                else if (drawingAttributes.PenTip == PenTipShape.Circle)
                    drawingAttributes.Size = new Size(drawingAttributes.Size.Width * xScale, drawingAttributes.Size.Height * yScale);
                stroke.DrawingAttributes = drawingAttributes;
                // change size of the stroke
                stroke.PointTransform = Matrix3x2.CreateScale(xScale, yScale);
            }

            // creates the resized area for inserting the image and the resized strokes
            _renderStrokes = new CanvasRenderTarget(_device, newCanvasWidth, newCanvasHeight, 96, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized, CanvasAlphaMode.Premultiplied);
            using (var ds = _renderStrokes.CreateDrawingSession())
            {
                ds.Clear(Colors.White);
                if (!_isEmptyCanvas)
                {
                    var image = CanvasBitmap.CreateFromSoftwareBitmap(_device, _softwareBitmap);
                    ds.DrawImage(image, 0, 0);
                }
                ds.DrawInk(strokes);
            }

            // insert the resized image into the target image with the correct original size = cropping out-of-border strokes
            _renderTarget = new CanvasRenderTarget(_device, _originalWidth, _originalHeight, 96);
            using (var dsTarget = _renderTarget.CreateDrawingSession())
            {
                dsTarget.DrawImage(_renderStrokes, new Rect(0, 0, _originalWidth, _originalHeight), new Rect(0, 0, _originalWidth, _originalHeight));
            }

            await CreateOutputDataAsync();
        }


        private async Task CreateOutputDataAsync()
        {
            // prepare a temporary file for storing the image data, using this filestream 
            StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
            var file = await storageFolder.CreateFileAsync("outputRenderTarget.jpg", CreationCollisionOption.ReplaceExisting);

            // save rendering output
            OutputWriteableBitmap = new WriteableBitmap(1, 1);
            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await _renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);

                // create writeable bitmap from stream
                fileStream.Seek(0);
                OutputWriteableBitmap = await OutputWriteableBitmap.FromStream(fileStream);

                // create byte array from stream
                fileStream.Seek(0);
                var reader = new DataReader(fileStream);
                OutputByteArray = new byte[fileStream.Size];
                await reader.LoadAsync((uint)fileStream.Size);
                reader.ReadBytes(OutputByteArray);
            }
            
            // delete temporary file after saving output
            await file.DeleteAsync();
        }
    }    
}