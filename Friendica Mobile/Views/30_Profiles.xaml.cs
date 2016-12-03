using Friendica_Mobile.Models;
using Friendica_Mobile.Mvvm;
using Friendica_Mobile.Triggers;
using System;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Friendica_Mobile.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Profiles : Page
    {
        public Profiles()
        {
            this.InitializeComponent();
            // wählt den entsprechenden VisualState für die aktuelle Orientation und das Device aus (sollte eigentlich
            // über OrientationDeviceFamilyTrigger funktioniert, wird aber beim Wechsel zwischen den Views nicht richtig angestoßen)
            VisualStateSelector selector = new VisualStateSelector(this);

            // the bottomappbar might cover important things on Phone devices if keyboard is shown - collapse the appbar thanl
            var inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;

            var mvvm = this.DataContext as ProfilesViewmodel;
            mvvm.LoadProfiles();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var mvvm = this.DataContext as ProfilesViewmodel;
            mvvm.SaveProfiles();
            // TODO: issue with switching pages - saving mvvm leads to "is already child of another element" crashes
            //App.ProfilesVm = mvvm;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var mvvm = this.DataContext as ProfilesViewmodel;
            // navigating to this page without parameter means real navigation or backnav
            if (App.ProfilesVm != null)
            {
                this.DataContext = App.ProfilesVm;
                mvvm = this.DataContext as ProfilesViewmodel;
                mvvm.RestoreProfiles();
            }
            else
                mvvm.LoadProfiles();

            base.OnNavigatedTo(e);
        }

        private void UserControl_SizeChanged(object sender, System.EventArgs e)
        {
            var gridView = sender as clsVariableGridView;
            if (!gridView.IsUpdating)
                gridView.Update();
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Collapsed;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            bottomAppBar.Visibility = Visibility.Visible;
        }


        private void buttonShowHelp_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.Flyout.ShowAt(button);
        }

        private void buttonGotoSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Views.Settings));
        }

        private void imgProfilePhoto_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // grap central setting for displaying content in RichTextBlock from MainStyles.xaml
            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("ms-appx:///Styles/MainStyles.xaml", UriKind.RelativeOrAbsolute);
            var flyoutStyle = (Style)myResourceDictionary["FlyoutPresenterImageFullscreen"];

            // load full data from tapping sender
            var imgSender = sender as Image;
            Image img = new Image();
            img.Stretch = Stretch.Uniform;
            img.Source = imgSender.Source;

            ScrollViewer scrlViewer = new ScrollViewer();
            scrlViewer.ZoomMode = ZoomMode.Enabled;
            scrlViewer.MinZoomFactor = (float)0.7;
            scrlViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrlViewer.Margin = new Thickness(-8, -8, 0, 0);
            scrlViewer.Height = Window.Current.Bounds.Height;
            scrlViewer.Width = Window.Current.Bounds.Width;

            Canvas canvas = new Canvas();

            // Fonticon für Schließen 
            FontIcon fIcon = new FontIcon();
            fIcon.Glyph = System.Net.WebUtility.HtmlDecode("&#x1F5D9;"); ;
            fIcon.FontFamily = new FontFamily("Segoe UI Emoji");
            fIcon.FontSize = 16;

            Button buttonClose = new Button();
            buttonClose.Tapped += ButtonClose_Tapped;
            buttonClose.Content = fIcon;

            scrlViewer.Content = img;
            canvas.Children.Add(scrlViewer);
            canvas.Children.Add(buttonClose);

            // display border as flyout
            Flyout flyout = new Flyout();
            flyout.Content = canvas;
            buttonClose.Tag = flyout;
            flyout.FlyoutPresenterStyle = flyoutStyle;
            flyout.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full;
            flyout.ShowAt(imgSender);
        }

        private void ButtonClose_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var button = sender as Button;
            var flyout = button.Tag as Flyout;
            flyout.Hide();
        }

        private void imgProfilePhoto_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var img = sender as Image;

            if (e.ErrorMessage == "E_NETWORK_ERROR")
            {
                // if error is thrown it might be an issue with the IP address not recognized, so try again with the server name from settings
                var uri = ((BitmapImage)img.Source).UriSource;
                var server = new Uri(App.Settings.FriendicaServer, UriKind.RelativeOrAbsolute);
                if (uri.Host != server.Host)
                    img.Source = new BitmapImage(new Uri(uri.AbsoluteUri.Replace(uri.Host, server.Host)));
                else
                {
                    img.Source = new BitmapImage(new Uri("ms-appx:///Assets/NoImage.jpg", UriKind.RelativeOrAbsolute));
                    img.Width = 100;
                    img.Height = 100;
                }
            }
        }

    }
}
