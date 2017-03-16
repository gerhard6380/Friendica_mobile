using Friendica_Mobile.Triggers;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Friendica_Mobile.Views
{
    public sealed partial class A5_InkCanvas : Page
    {
        public A5_InkCanvas()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            pageMvvm.PencilSettingsChanged += PageMvvm_PencilSettingsChanged;
        }

        // take over photo data from flipview in Photos.xaml
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.Parameter.GetType() == typeof(BitmapImage))
            {
                pageMvvm.OriginalImage = e.Parameter as BitmapImage;
                pageMvvm.IsNewImage = false;
            }
            else
            {
                pageMvvm.IsNewImage = true;
            }
            base.OnNavigatedTo(e);

            CheckUsedSpace();
        }


        // react on changes from user on the settings for the pencil
        private void PageMvvm_PencilSettingsChanged(object sender, System.EventArgs e)
        {
            var attr = new InkDrawingAttributes();
            attr.Color = (pageMvvm.SelectedColor as SolidColorBrush).Color;
            attr.PenTip = PenTipShape.Circle;
            attr.IgnorePressure = false;
            attr.Size = new Windows.Foundation.Size(pageMvvm.PencilSize, pageMvvm.PencilSize);
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attr);
        }


        // resize inkCanvas if white background has been resized
        private void borderNewImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckUsedSpace();

            inkCanvas.Width = rectNewImage.ActualWidth;
            inkCanvas.Height = rectNewImage.ActualHeight;
        }

        // resize InkCanvas if image has been resized
        private void borderExistingImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckUsedSpace();

            inkCanvas.Width = imageBackgroundForCanvas.ActualWidth;
            inkCanvas.Height = imageBackgroundForCanvas.ActualHeight;
        }


        private void gridInkCanvasTotalArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CheckUsedSpace();

            // set limits for image size according to the size of the available space, otherwise pictures will zoom to fill the
            // whole width, this can cause vanishing parts of the image as height is then higher than available space (missing logic in Image.Stretch = Uniform)
            imageBackgroundForCanvas.MaxHeight = gridInkCanvasTotalArea.ActualHeight - 48;
            imageBackgroundForCanvas.MaxWidth = gridInkCanvasTotalArea.ActualWidth - 48;

            // keep a standard aspect ratio of 3:2 for the white background
            if (gridInkCanvasTotalArea.ActualWidth > gridInkCanvasTotalArea.ActualHeight)
            {
                if (gridInkCanvasTotalArea.ActualHeight * 1.5 > borderNewImage.ActualWidth)
                {
                    borderNewImage.Width = gridInkCanvasTotalArea.ActualWidth;
                    borderNewImage.Height = gridInkCanvasTotalArea.ActualWidth * 0.67;
                }
                else
                {
                    borderNewImage.Width = gridInkCanvasTotalArea.ActualHeight * 1.5;
                    borderNewImage.Height = gridInkCanvasTotalArea.ActualHeight;
                }
            }
            else
            {
                if (gridInkCanvasTotalArea.ActualWidth * 1.5 > borderNewImage.ActualHeight)
                {
                    borderNewImage.Width = gridInkCanvasTotalArea.ActualHeight * 0.67;
                    borderNewImage.Height = gridInkCanvasTotalArea.ActualHeight;
                }
                else
                {
                    borderNewImage.Width = gridInkCanvasTotalArea.ActualWidth;
                    borderNewImage.Height = gridInkCanvasTotalArea.ActualWidth * 1.5;
                }
            }
        }


        private void inkCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            // enable all input devices
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen | 
                                                      Windows.UI.Core.CoreInputDeviceTypes.Touch | 
                                                      Windows.UI.Core.CoreInputDeviceTypes.Mouse;

            // load new InkToolBar if system version is >= 10.0.14393
            if (pageMvvm.IsInkToolBarPresent)
            {
                var toolbar = new InkToolbar();
                toolbar.Name = "inkToolBar";
                toolbar.Visibility = Visibility.Visible;
                toolbar.TargetInkCanvas = inkCanvas;
                stackPanelInkButtons.Children.Insert(0, toolbar);
            }
        }


        private void buttonEnableInking_Checked(object sender, RoutedEventArgs e)
        {
            if (buttonEnableInking.IsChecked == null)
                inkCanvas.InkPresenter.IsInputEnabled = false;
            else
                inkCanvas.InkPresenter.IsInputEnabled = (bool)buttonEnableInking.IsChecked;
        }


        private void buttonConfirmDeletion_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            buttonDeleteStrokes.Flyout.Hide();
        }


        private void CheckUsedSpace()
        {
            var totalArea = gridInkCanvasTotalArea.ActualWidth * gridInkCanvasTotalArea.ActualHeight;
            var imageArea = stackpanelBackgroundForCanvas.ActualWidth * stackpanelBackgroundForCanvas.ActualHeight;
            if (imageArea / totalArea < 0.5)
            {
                pageMvvm.UsedSpaceWarningColor = new SolidColorBrush(Colors.Red);
            }
            else if (imageArea / totalArea < 0.75)
            {
                pageMvvm.ShowUsedSpaceWarning = true;
                pageMvvm.UsedSpaceWarningColor = new SolidColorBrush(Colors.Yellow);
            }
            else
                pageMvvm.ShowUsedSpaceWarning = false;
        }


    }
}
