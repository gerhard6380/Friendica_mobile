using SeeberXamarin.Controls;
using Friendica_Mobile.UWP.Helpers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Xamarin.Forms.Platform.UWP;


[assembly: ExportRenderer(typeof(CustomNavigationCounter), typeof(CustomNavigationCounterRenderer))]
namespace Friendica_Mobile.UWP.Helpers
{
    public class CustomNavigationCounterRenderer : ViewRenderer<CustomNavigationCounter, Border>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<CustomNavigationCounter> e)
        {
            base.OnElementChanged(e);
            if (Control != null || e.NewElement == null) { return; }

            // stick to PropertyChanged to react on changes of the counter value
            var oldcounter = e.OldElement as CustomNavigationCounter;
            if (oldcounter != null)
                oldcounter.PropertyChanged -= Counter_PropertyChanged;
            var counter = e.NewElement as CustomNavigationCounter;
            counter.PropertyChanged += Counter_PropertyChanged;
            SetCounterElement(counter);
        }

        private void Counter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Counter")
                SetCounterElement(sender as CustomNavigationCounter);
        }

        private void SetCounterElement(CustomNavigationCounter counter)
        {
            // create an ellipse if there is a number to display, standard renderer makes a sqaure
            if (counter.Counter > 0)
            {
                var ellipse = new Ellipse() { Fill = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]) };
                var text = new TextBlock
                {
                    Text = counter.Counter.ToString(),
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var horizontalSide = GetHorizontalSide();
                var margin = (horizontalSide == HorizontalAlignment.Left) ? new Thickness(-4, 0,0,0) : new Thickness(0);

                var grid = new Grid()
                {
                    Width = 24,
                    Height = 24,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = horizontalSide,
                    Margin = margin
                };
                grid.Children.Add(ellipse);
                grid.Children.Add(text);
                var border = new Border
                {
                    Child = grid
                };
                // catch errors here as missing controls would lead to exceptions
                try { SetNativeControl(border); } catch { }
                // vanish original counter
                ((Xamarin.Forms.Frame)counter.Content).BackgroundColor = Xamarin.Forms.Color.Transparent;
                ((Xamarin.Forms.Frame)counter.Content).OutlineColor = Xamarin.Forms.Color.Transparent;
                ((Xamarin.Forms.Frame)counter.Content).Content = null;
            }
            else
                try { SetNativeControl(null); } catch { }
        }

        private HorizontalAlignment GetHorizontalSide()
        {
            if (Friendica_Mobile.App.DeviceType == Plugin.DeviceInfo.Abstractions.Idiom.Phone)
            {
                // TODO: hochformat
                if (Settings.NavigationOnRightSide && Friendica_Mobile.App.ShellWidth < Friendica_Mobile.App.ShellHeight)
                {
                    return HorizontalAlignment.Left;
                }
                else
                {
                    return HorizontalAlignment.Right;
                }
            }
            else
                return HorizontalAlignment.Right;
        }
    }
}