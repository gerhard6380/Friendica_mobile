using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using Friendica_Mobile.Views;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Friendica_Mobile.Styles
{
    public sealed partial class UserControlPhotosFlipView : UserControl
    {
        public UserControlPhotosFlipView()
        {
            this.InitializeComponent();
            VisualStateSelector selector = new VisualStateSelector(this);
            // separate caller for respecting different states defined in UserControlPhotosFlipView
            selector.VisualStateSelectorPhotosFlipView(this);
        }

        private void gridImageInFlipView_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            // set sizes for full view image
            scrollViewerFullImage.Width = Window.Current.Bounds.Width - 24;
            scrollViewerFullImage.Height = Window.Current.Bounds.Height - 24;
            imageFullView.Width = Window.Current.Bounds.Width - 24;
            imageFullView.Height = Window.Current.Bounds.Height - 24;

            // change orientation of stackpanels showing dates from horizontal to vertical 
            if (gridImageInFlipView.ActualWidth < 300)
            {
                stackpanelPhotoDateEdited.Orientation = Orientation.Vertical;
                stackpanelPhotoDateShot.Orientation = Orientation.Vertical;
            }
            else
            {
                stackpanelPhotoDateEdited.Orientation = Orientation.Horizontal;
                stackpanelPhotoDateShot.Orientation = Orientation.Horizontal;
            }

            ResizeImage();
        }

        private void ResizeImage()
        {
            var mvvm = this.DataContext as FriendicaPhotoExtended;

            // we need in editing mode at least 72 for texbox/buttons + 32; in viewmode MaxHeight = 48 -> minimum = 64 + 32
            // if textblock in viewmode is lower than 48 maxheight, we can reduce the height by this difference 
            // both are concerning the height of the date line
            double correction;
            double textblockHeight = textblockPhotoDescription.ActualHeight;
            if (mvvm.PhotoEditingEnabled)
                correction = 104 + stackpanelPhotoDateEdited.ActualHeight;
            else
                correction = 96 + stackpanelPhotoDateEdited.ActualHeight - (48 - textblockHeight);

            // safe guard if correction is higher than the ActualHeight to avoid an exception
            if (gridImageInFlipView.ActualHeight > correction)
                imageInFlipView.Height = gridImageInFlipView.ActualHeight - correction;
            else
                imageInFlipView.Height = gridImageInFlipView.ActualHeight;

            if (!Double.IsNaN(imageInFlipView.ActualWidth))
                gridPhotoDescriptionShow.MaxWidth = imageInFlipView.ActualWidth;
        }

        private void textBoxPhotoDescriptionEdit_IsEnabledChanged(object sender, Windows.UI.Xaml.DependencyPropertyChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.Focus(Windows.UI.Xaml.FocusState.Keyboard);
            ResizeImage();
        }

        private void textBoxPhotoDescriptionEdit_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            var mvvm = this.DataContext as FriendicaPhotoExtended;

            // if user hits escape we want to end the edit mode
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                mvvm.PhotoEditingEnabled = false;
                ResizeImage();
            }
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // check if we are allowed to execute to avoid multiple calling
                mvvm.NewPhotoDesc = textbox.Text;
                if (mvvm.SaveChangedPhotoDescriptionCommand.CanExecute(null))
                    mvvm.SaveChangedPhotoDescriptionCommand.Execute(null);
                e.Handled = true;
                mvvm.PhotoEditingEnabled = false;
            }
        }


        private void buttonPhotoCancelDescriptionChange_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var mvvm = this.DataContext as FriendicaPhotoExtended;
            mvvm.PhotoEditingEnabled = false;
            ResizeImage();
        }

        private void buttonPhotoDescriptionSaveChangedName_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var mvvm = this.DataContext as FriendicaPhotoExtended;
            mvvm.NewPhotoDesc = textBoxPhotoDescriptionEdit.Text;
            // check if we are allowed to execute to avoid multiple calling
            if (mvvm.SaveChangedPhotoDescriptionCommand.CanExecute(null))
                mvvm.SaveChangedPhotoDescriptionCommand.Execute(null);
            mvvm.PhotoEditingEnabled = false;
        }

        private void buttonClosePhotoFullImage_Click(object sender, RoutedEventArgs e)
        {
            flyoutFullImage.Hide();
        }

        private void flyoutFullImage_Opened(object sender, object e)
        {
            // load fullsize image if user wants to display in fullview (data not yet loaded if user has disabled local 
            // saving at all, or limited local saving to only small images)
            var mvvm = this.DataContext as FriendicaPhotoExtended;

            if (mvvm != null && !mvvm.FullSizeLoaded && !mvvm.NewUploadPlanned)
            {
                mvvm.LoadFullSize();
            }
        }

        private void imageFullView_Loaded(object sender, RoutedEventArgs e)
        {
            var photo = sender as Image;
            var mvvm = photo.DataContext as FriendicaPhotoExtended;
            if (mvvm == null)
                return;

            if (mvvm.IsLoadingFullSize)
                mvvm.IsLoadingFullSize = false;
        }

    }
}
