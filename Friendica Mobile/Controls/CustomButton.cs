using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    /// <summary>
    /// encapsulate Xamarin.Forms.Button without any changes into a different class which enables 
    /// changing the requested theme on UWP (see CustomButtonRenderer)
    /// </summary>
    public class CustomButton : Button
    {
        public CustomButton()
        {
            this.Clicked += CustomButton_Clicked;
        }

        private async void CustomButton_Clicked(object sender, System.EventArgs e)
        {
            if (Device.RuntimePlatform != Device.WinPhone)
            {
                var button = sender as Button;
                await button.ScaleTo(1.05, 150);
                await button.ScaleTo(1, 150);
            }
        }
    }
}
