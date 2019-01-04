using Friendica_Mobile.UWP.Models;
using System;
using System.Collections.ObjectModel;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


namespace Friendica_Mobile.UWP.Mvvm
{
    public class PhotosInkCanvasViewmodel : BindableClass
    {
        public event EventHandler PencilSettingsChanged;

        #region properties

        // indicator if app is used on a device running on W10 >= 10.0.14393 (having InkToolBar class)
        private bool _isInkToolBarPresent;
        public bool IsInkToolBarPresent
        {
            get { return _isInkToolBarPresent; }
            set { _isInkToolBarPresent = value; }
        }

        // indicator if button bar is fully visible or not
        private bool _isButtonBarNotVisible;
        public bool IsButtonBarNotVisible
        {
            get { return _isButtonBarNotVisible; }
            set { _isButtonBarNotVisible = value;
                OnPropertyChanged("IsButtonBarNotVisible"); }
        }

        // indicator if there is no original image provided (user starts a blank image)
        private bool _isNewImage;
        public bool IsNewImage
        {
            get { return _isNewImage; }
            set { _isNewImage = value;
                OnPropertyChanged("IsNewImage"); }
        }

        // object containing the photo
        private FriendicaPhotoExtended _photo;
        public FriendicaPhotoExtended Photo
        {
            get { return _photo; }
            set
            {
                _photo = value;
                if (value.FullSizeData != null)
                {
                    if (value.FullSizeData.GetType() == typeof(BitmapImage))
                        OriginalImage = (BitmapImage)value.FullSizeData;
                    else if (value.FullSizeData.GetType() == typeof(WriteableBitmap))
                    {
                        OriginalImage = (WriteableBitmap)value.FullSizeData;
                        if (ChangedImage == null)
                            ChangedImage = (WriteableBitmap)value.FullSizeData;
                    }
                    OriginalImageUrl = value.PhotolistThumb;
                    IsNewImage = false;
                }
                else
                    IsNewImage = true;
            }
        }


        // string with the url of the original image
        private string _originalImageUrl;
        public string OriginalImageUrl
        {
            get { return _originalImageUrl; }
            set { _originalImageUrl = value;
                OnPropertyChanged("OriginalImageUrl");
            }
        }

        // saving original image if provided to have a background for the strokes
        private ImageSource _originalImage;
        public ImageSource OriginalImage
        {
            get { return _originalImage; }
            set { _originalImage = value;
                OnPropertyChanged("OriginalImage");
            }
        }

        // indicator if app is currently combining image with the strokes
        private bool _isRenderingImage;
        public bool IsRenderingImage
        {
            get { return _isRenderingImage; }
            set { _isRenderingImage = value;
                OnPropertyChanged("IsRenderingImage");
            }
        }

        // contains image with the rendered content for giving it back to Photos.xaml
        private WriteableBitmap _changedImage;
        public WriteableBitmap ChangedImage
        {
            get { return _changedImage; }
            set { _changedImage = value; }
        }

        // contains size of the pencil
        private int _pencilSize;
        public int PencilSize
        {
            get { return _pencilSize; }
            set { _pencilSize = value;
            PencilSettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // list of all available colors
        private ObservableCollection<Brush> _availableColors;
        public ObservableCollection<Brush> AvailableColors
        {
            get { return _availableColors; }
            set { _availableColors = value; }
        }

        // selected color 
        private Brush _selectedColor;
        public Brush SelectedColor
        {
            get { return _selectedColor; }
            set { _selectedColor = value;
                OnPropertyChanged("SelectedColor");
                PencilSettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // indicator to show a warning sign if the ink canvas is not properly filling the space (> 75%)
        private bool _showUsedSpaceWarning;
        public bool ShowUsedSpaceWarning
        {
            get { return _showUsedSpaceWarning; }
            set { _showUsedSpaceWarning = value;
                OnPropertyChanged("ShowUsedSpaceWarning"); }
        }

        // define color of the warning sign (<50% = Red, <75% is Yellow)
        private Brush _usedSpaceWarningColor;
        public Brush UsedSpaceWarningColor
        {
            get { return _usedSpaceWarningColor; }
            set { _usedSpaceWarningColor = value;
                OnPropertyChanged("UsedSpaceWarningColor");
            }
        }

        // save current displayed widths/heights of image and inkcanvas - needed for placing invisible ink strokes in the corner
        private double _shownWidth;
        public double ShownWidth
        {
            get { return _shownWidth; }
            set { _shownWidth = value; }
        }

        private double _shownHeight;
        public double ShownHeight
        {
            get { return _shownHeight; }
            set { _shownHeight = value; }
        }

        #endregion


        public PhotosInkCanvasViewmodel()
        {
            // check if device is on W10 version >= 10.0.14393
            IsInkToolBarPresent = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 3, 0);
            
            IsButtonBarNotVisible = false;
            IsNewImage = false;

            // fill list of available colors for old mode
            AvailableColors = new ObservableCollection<Brush>();
            AvailableColors.Add(new SolidColorBrush(Colors.Black));
            AvailableColors.Add(new SolidColorBrush(Colors.White));
            AvailableColors.Add(new SolidColorBrush(Colors.Gainsboro));
            AvailableColors.Add(new SolidColorBrush(Colors.LightGray));
            AvailableColors.Add(new SolidColorBrush(Colors.DarkGray));
            AvailableColors.Add(new SolidColorBrush(Colors.Gray));

            AvailableColors.Add(new SolidColorBrush(Colors.DarkRed));
            AvailableColors.Add(new SolidColorBrush(Colors.Red));
            AvailableColors.Add(new SolidColorBrush(Colors.DarkOrange));
            AvailableColors.Add(new SolidColorBrush(Colors.Orange));
            AvailableColors.Add(new SolidColorBrush(Colors.Gold));
            AvailableColors.Add(new SolidColorBrush(Colors.Yellow));

            AvailableColors.Add(new SolidColorBrush(Colors.GreenYellow));
            AvailableColors.Add(new SolidColorBrush(Colors.LawnGreen));
            AvailableColors.Add(new SolidColorBrush(Colors.Green));
            AvailableColors.Add(new SolidColorBrush(Colors.Turquoise));
            AvailableColors.Add(new SolidColorBrush(Colors.Blue));
            AvailableColors.Add(new SolidColorBrush(Colors.DarkBlue));

            AvailableColors.Add(new SolidColorBrush(Colors.BlueViolet));
            AvailableColors.Add(new SolidColorBrush(Colors.Purple));
            AvailableColors.Add(new SolidColorBrush(Colors.Wheat));
            AvailableColors.Add(new SolidColorBrush(Colors.Tan));
            AvailableColors.Add(new SolidColorBrush(Colors.Chocolate));
            AvailableColors.Add(new SolidColorBrush(Colors.Brown));

            AvailableColors.Add(new SolidColorBrush(Colors.HotPink));
            AvailableColors.Add(new SolidColorBrush(Colors.LightSalmon));
            AvailableColors.Add(new SolidColorBrush(Colors.PaleGoldenrod));
            AvailableColors.Add(new SolidColorBrush(Colors.LightGreen));
            AvailableColors.Add(new SolidColorBrush(Colors.LightSkyBlue));
            AvailableColors.Add(new SolidColorBrush(Colors.Plum));

            // pre select red as color and pencil size as 4
            SelectedColor = AvailableColors[7];
            PencilSize = 4;
        }

    }
}