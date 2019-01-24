using Xamarin.Forms;

namespace SeeberXamarin.Controls
{
    /// <summary>
    /// encapsulate Xamarin.Forms.Button without any changes into a different class which enables 
    /// removing the border on macos, see FramelessButtonRenderer in macOS plattform project
    /// </summary>
    public class FramelessButton : Button
    {
        public FramelessButton()
        {
        }
    }
}
