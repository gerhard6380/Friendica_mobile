using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

namespace Friendica_Mobile.UWP
{
    public static class NavigationPaneAcrylic
    {
        public static void ChangeBackgroundToAcrylic(Xamarin.Forms.Grid grid)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5, 0))
            {
                LayoutRenderer renderer = null;

                if (grid != null)
                    renderer = Platform.GetRenderer(grid) as LayoutRenderer;

                var color = grid.BackgroundColor;
                var accentAlpha = System.Math.Round(color.A * 255, 0);
                var accentRed = System.Math.Round(color.R * 255, 0);
                var accentGreen = System.Math.Round(color.G * 255, 0);
                var accentBlue = System.Math.Round(color.B * 255, 0);
                var acrylicBrush = new Windows.UI.Xaml.Media.AcrylicBrush
                {
                    BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                    TintColor = Windows.UI.Color.FromArgb((byte)accentAlpha, (byte)accentRed, (byte)accentGreen, (byte)accentBlue),
                    FallbackColor = Windows.UI.Color.FromArgb(255, 0, 159, 227),
                    TintOpacity = 0.7
                };
                if (renderer != null)
                    renderer.Background = acrylicBrush;
            }
        }
    }
}
