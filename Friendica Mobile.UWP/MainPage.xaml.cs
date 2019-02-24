using Friendica_Mobile.Views;
using Xamarin.Forms;

namespace Friendica_Mobile.UWP
{
    public sealed partial class MainPage
    {
        private Friendica_Mobile.App _app;

        public MainPage()
        {
            this.InitializeComponent();
            _app = new Friendica_Mobile.App();
            LoadApplication(_app);

            // change the navigation background from solid color to acrylic design if supported on the OS version
            var grid = GetNavigationPane();
            NavigationPaneAcrylic.ChangeBackgroundToAcrylic(grid);
        }

        private Xamarin.Forms.Grid GetNavigationPane()
        {
            // retrieve the Grid "GridNavigationPane" from MasterDetailControl
            var shell = (_app.MainPage as NavigationPage).RootPage as CustomShell;
            // number 2 used, because 0 = Detail and 1 = GridTapRecognizer
            var pane = ((Grid)shell?.Content)?.Children[2] as Grid;
            return pane;
        }
    }
}
