using Friendica_Mobile.HttpRequests;
using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;


namespace Friendica_Mobile.Mvvm
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
            set { _isNewImage = value; }
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
        private BitmapImage _originalImage;
        public BitmapImage OriginalImage
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
            set { _isRenderingImage = value; }
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
        
        #endregion


        #region commands

        // save strokes button
        Mvvm.Command _saveStrokesCommand;
        public Mvvm.Command SaveStrokesCommand { get { return _saveStrokesCommand ?? (_saveStrokesCommand = new Mvvm.Command(ExecuteSaveStrokes, CanSaveStrokes)); } }
        private bool CanSaveStrokes()
        {
            return true;
        }

        private async void ExecuteSaveStrokes()
        {
            // TODO implement correct function for saving strokes into image or into a new white background image
            // TODO: convert strokes into writeablebitmap together with background image
            // TODO: save this new image into the App.PhotosVm and start the upload to the server
            // TODO: don't forget to set an indicator to show the user that we are loading an image
            // TODO: avoid exiting app without finishing the upload
            // TODO: test if navigating to another page might be possible while App is uploading data in background in App.PhotosVm

            string errorMsg = "Damit werden die Strokes in das Bild eingefügt, Upload zum Server erfolgt erst im nächsten Bild.";
            var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
            await dialog.ShowDialog(0, 0);
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