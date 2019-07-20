using Friendica_Mobile.Styles;
using Friendica_Mobile.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Friendica_Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Network : BaseContentPage
    {
        public Network()
        {
            InitializeComponent();       

        }

        // needed to avoid rendering errors in ios designer
        void Handle_OnCommandBarPositionChanged(object sender, System.EventArgs e)
        {
        }
        void Handle_MoreOptionsButtonClicked(object sender, System.EventArgs e)
        {
        }

        void ButtonIconButton_Clicked(object sender, System.EventArgs e)
        {
            // macOS is not scrolling to top if we use 0, we need to set it to the value of the view height
            if (Device.RuntimePlatform == Device.macOS)
                ScrollViewNetworkThreads.ScrollToAsync(0, ScrollViewNetworkThreads.Height, true);
            else
                ScrollViewNetworkThreads.ScrollToAsync(0, 0, true);
        }

        async void ScrollViewNetworkThreads_Scrolled(object sender, Xamarin.Forms.ScrolledEventArgs e)
        {
            // make the button for scrolling back to the top of the page to visible after leaving the first third of the visible area
            var scroll = sender as ScrollView;
            GridScrollToTop.IsVisible = (scroll.ScrollY > scroll.Height / 3);

            // check if we are reaching the last part of the contetn --> trigger to start loading new entries
            var atBottom = scroll.ScrollY > (scroll.ContentSize.Height - 2 * scroll.Height);
            if (atBottom)
            {
                var mvvm = this.BindingContext as NetworkViewModel;
                if (mvvm.PostsModel.NetworkThreads.Count > 0 && !mvvm.PostsModel.IsLoadingNext)
                    await mvvm.LoadNextAsync();
            }
        }
    }
}
