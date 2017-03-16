using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft/fwlink/?LinkId=234238

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Photos : Page
    {
        private PrintHelper printHelper;

        public Photos()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // react on back button pressed on phones to navigate back to onlyAlbums
            App.BackToAlbumsRequested += App_BackToAlbumsRequested;

            // the bottomappbar might cover important things on Phone devices if keyboard is shown - collapse the appbar thanl
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;

            pageMvvm.PrintButtonClicked += SelectedPhoto_PrintButtonClicked;
            pageMvvm.NewPhotoalbumAdded += PageMvvm_NewPhotoalbumAdded;
            pageMvvm.PhotoalbumUpdated += PageMvvm_PhotoalbumUpdated;
        }

        private void PageMvvm_PhotoalbumUpdated(object sender, EventArgs e)
        {
            var album = sender as FriendicaPhotoalbum;
            listviewPhotoalbums.SelectedItem = album;
            listviewPhotoalbums.ScrollIntoView(album);
        }

        private async void App_BackToAlbumsRequested(object sender, EventArgs e)
        {
            // var answer = await AskUserForConfirmingDiscard(listviewConversations);  --> derzeit keine Abfrage notwendig
            var answer = true;

            if (answer)
            {
                if (pageMvvm.PhotosView == PhotosViewmodel.PhotosViewStates.OnlyPhotos)
                {
                    pageMvvm.PhotosView = PhotosViewmodel.PhotosViewStates.OnlyAlbums;
                    pageMvvm.SelectedPhotoalbum = null;
                    App.PhotosNavigatedIntoAlbum = false;
                    App.NavStatus = NavigationStatus.OK;
                }
            }
        }

        private void PageMvvm_NewPhotoalbumAdded(object sender, EventArgs e)
        {
            // scroll up to the newly added photoalbum 
            var album = sender as FriendicaPhotoalbum;
            listviewPhotoalbums.ScrollIntoView(album);
        }

        private async void SelectedPhoto_PrintButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // show print ui
                printHelper.PhotoData = (BitmapImage)pageMvvm.SelectedPhotoalbum.SelectedPhoto.FullSizeData;
                await Windows.Graphics.Printing.PrintManager.ShowPrintUIAsync();
            }
            catch
            {
                // printing canot proceed at this time
                string errorMsg = "Drucken ist derzeit nicht möglich.";
                var dialog = new MessageDialogMessage(errorMsg, "", "OK", null);
                await dialog.ShowDialog(0, 0);
            }
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Collapsed;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Visible;
        }

        private void buttonGotoSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.Settings));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.PhotosVm = pageMvvm;
            base.OnNavigatedFrom(e);

            // disconnect printing service
            if (printHelper != null)
                printHelper.UnregisterForPrinting();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.PhotosVm != null)
                this.DataContext = App.PhotosVm;

            //// user could navigate from settings, so we might need to reload the settings indicator
            //if (e.NavigationMode != NavigationMode.Back)
            //pageMvvm.CheckServerSettings();

            pageMvvm.LoadContentFromServer();
            base.OnNavigatedTo(e);

            // initialize common helper class and register for printing
            printHelper = new PrintHelper(this);
            printHelper.RegisterForPrinting();
        }








        private void listviewPhotoalbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mvvm = this.DataContext as PhotosViewmodel;

            if (mvvm.SelectedPhotoalbum == null)
                return;

            if (mvvm.SelectedPhotoalbum.SelectedPhoto == null && mvvm.SelectedPhotoalbum.PhotosInAlbum.Count != 0)
                mvvm.SelectedPhotoalbum.SelectedPhoto = mvvm.SelectedPhotoalbum.PhotosInAlbum[0];

            if (mvvm.PhotosView == PhotosViewmodel.PhotosViewStates.OnlyAlbums)
            {
                App.PhotosNavigatedIntoAlbum = true;
                mvvm.PhotosView = PhotosViewmodel.PhotosViewStates.OnlyPhotos;
            }
        }


        private void listViewSelectedAlbum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            var selectedItem = listview.SelectedItem as FriendicaPhotoExtended;
            listViewSelectedAlbum.ScrollIntoView(selectedItem);
        }


        private void flipviewSelectedPhoto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var flipview = sender as FlipView;
            var selectedItem = flipview.SelectedItem as FriendicaPhotoExtended;
            listViewSelectedAlbum.ScrollIntoView(selectedItem);
            // load mediumsize image if user moves to the picture (data not yet loaded if user has disabled local saving at all)
            if (selectedItem != null && !selectedItem.MediumSizeLoaded)
            {
                selectedItem.LoadMediumSize();
            }
        }


        private void textBoxAlbumnameEdit_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            var mvvm = this.DataContext as PhotosViewmodel;

            // if user hits escape we want to end the edit mode
            if (e.Key == Windows.System.VirtualKey.Escape)
                mvvm.AlbumEditingEnabled = false;
            else if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // execute only if we can do so, avoids twice start of the function
                mvvm.SelectedPhotoalbum.AlbumnameNew = textbox.Text;
                if (mvvm.SaveChangedAlbumnameCommand.CanExecute(null))
                    mvvm.SaveChangedAlbumnameCommand.Execute(null);
            }
        }

        private void textBoxAlbumnameEdit_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // set focus on the textbox when user has hit the edit button to enable the editor mode
            var textbox = sender as TextBox;
            textbox.Focus(FocusState.Keyboard);
        }

        private void buttonAlbumnameCancelNameChange_Click(object sender, RoutedEventArgs e)
        {
            // go back to view mode when user hit the cancel button (esc key is handled above in the same way)
            var mvvm = this.DataContext as PhotosViewmodel;
            mvvm.AlbumEditingEnabled = false;
        }
    }
}
