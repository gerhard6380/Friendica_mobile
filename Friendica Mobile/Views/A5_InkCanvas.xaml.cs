using Friendica_Mobile.Models;
using Friendica_Mobile.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace Friendica_Mobile.Views
{
    public sealed partial class A5_InkCanvas : Page
    {
        ResourceLoader loader = ResourceLoader.GetForCurrentView();

        public A5_InkCanvas()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            pageMvvm.PencilSettingsChanged += PageMvvm_PencilSettingsChanged;
        }

        // take over photo data from flipview in Photos.xaml
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter != null && e.Parameter.GetType() == typeof(FriendicaPhotoExtended))
            {
                pageMvvm.Photo = e.Parameter as FriendicaPhotoExtended;
            }
            base.OnNavigatedTo(e);

            CheckUsedSpace();
        }


        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // ask user for confirmation when navigating away without saving
            if (CheckForUnsavedStrokes())
            {
                e.Cancel = true;
                // "strokes not yet saved, do you want to save them now?
                string errorMsg = loader.GetString("messageDialogPhotosAskForSavingStrokes");
                var dialog = new MessageDialogMessage(errorMsg, "", loader.GetString("buttonYes"), loader.GetString("buttonNo"));
                await dialog.ShowDialog(0, 1);

                var frame = App.GetFrameForNavigation();
                // save strokes if user has confirmed with yes
                if (dialog.Result == 0)
                {
                    await SaveStrokesAsync();
                    frame.GoBack();
                }
                else
                {
                    // empty container
                    inkCanvas.InkPresenter.StrokeContainer.Clear();
                    frame.GoBack();
                }

            }

            base.OnNavigatingFrom(e);
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

            // eventhandler for hiding navigation elements or not (doesn't raise when user calls "delete all" from toolbar)
            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }

        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            App.Settings.HideNavigationElements = CheckForUnsavedStrokes();
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            App.Settings.HideNavigationElements = CheckForUnsavedStrokes();
        }

        private bool CheckForUnsavedStrokes()
        {
            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            //TODO remove the following
            //if (strokes.Count > 0)
            //    App.NavStatus = NavigationStatus.PhotosChanged;
            //else
            //    App.NavStatus = NavigationStatus.OK;

            if (strokes.Count > 0)
                return true;
            else
                return false;
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
            App.Settings.HideNavigationElements = CheckForUnsavedStrokes();
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


        private void SaveSizesToViewmodel()
        {
            if (pageMvvm.IsNewImage)
            {
                pageMvvm.ShownWidth = rectNewImage.ActualWidth;
                pageMvvm.ShownHeight = rectNewImage.ActualHeight;
            }
            else
            {
                pageMvvm.ShownWidth = imageBackgroundForCanvas.ActualWidth;
                pageMvvm.ShownHeight = imageBackgroundForCanvas.ActualHeight;
            }
        }


        private async void buttonSavingStrokes_Click(object sender, RoutedEventArgs e)
        {
            await SaveStrokesAsync();
        }


        private async Task SaveStrokesAsync()
        {
            // activate an indicator showing the user that app is rendering the image
            pageMvvm.IsRenderingImage = true;

            SaveSizesToViewmodel();

            // add invisible ink to the edges to get the full size of ink area
            var container = inkCanvas.InkPresenter.StrokeContainer;
            var builder = new InkStrokeBuilder();
            var da = inkCanvas.InkPresenter.CopyDefaultDrawingAttributes();
            da.Size = new Size(0.1, 0.1);
            builder.SetDefaultDrawingAttributes(da);
            var topLeft = builder.CreateStroke(new List<Point>() { new Point(0, 0), new Point(0, 0) });
            container.AddStroke(topLeft);
            var bottomRight = builder.CreateStroke(new List<Point>() { new Point(pageMvvm.ShownWidth, pageMvvm.ShownHeight),
                                                                        new Point(pageMvvm.ShownWidth, pageMvvm.ShownHeight) });
            container.AddStroke(bottomRight);

            // prepare input data for the class to render strokes on the background
            object original;
            if (pageMvvm.ChangedImage != null)
                original = pageMvvm.ChangedImage; // if already saved by user, take this as base
            else if (pageMvvm.Photo.NewUploadPlanned)
                original = pageMvvm.Photo.NewPhotoStorageFile; // new photo upload from device or camera
            else if (pageMvvm.IsNewImage)
                original = null; // new empty ink canvas 
            else
            {
                // when sample images, we need to load the writablebitmap stream from source again
                if (pageMvvm.OriginalImageUrl.Contains("images.metmuseum.org"))
                {
                    var httpClient = new HttpClient();
                    var buffer = await httpClient.GetBufferAsync(new Uri(pageMvvm.OriginalImageUrl));
                    var writeableBitmap = new WriteableBitmap(1, 1);

                    using (var stream = buffer.AsStream())
                    {
                        await writeableBitmap.SetSourceAsync(stream.AsRandomAccessStream());
                    }
                    original = writeableBitmap;
                }
                else
                    original = await pageMvvm.Photo.GetWriteableBitmapFromPhotoIdAndScaleAsync("0"); // existing photo to be changed
            }

            // class for rendering strokes into the base image
            var clsRender = new clsRenderInkToImage(original);
            await clsRender.RenderInkToImageAsync(inkCanvas.InkPresenter.StrokeContainer, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight);

            // save rendered output to viewmodel for showing result to user and holding data for uploading
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.SaveChangedPhotoData(clsRender.OutputByteArray);
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.ThumbSizeData = clsRender.OutputWriteableBitmap;
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.MediumSizeData = clsRender.OutputWriteableBitmap;
            App.PhotosVm.SelectedPhotoalbum.SelectedPhoto.FullSizeData = clsRender.OutputWriteableBitmap;

            // set imagesource to newly combined picture data
            pageMvvm.OriginalImage = clsRender.OutputWriteableBitmap;
            pageMvvm.ChangedImage = clsRender.OutputWriteableBitmap;
            pageMvvm.IsNewImage = false;
            
            // empty container
            inkCanvas.InkPresenter.StrokeContainer.Clear();

            // remove indicator after successfull rendering image
            pageMvvm.IsRenderingImage = false;
        }

    }
}
