using System;
using Friendica_Mobile.iOS;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBarIOS))]
namespace Friendica_Mobile.iOS
{
    public class StatusBarIOS : IStatusBar
    {
        public void HideStatusBar()
        {
            UIApplication.SharedApplication.StatusBarHidden = true;
        }

        public void ShowStatusBar()
        {
            UIApplication.SharedApplication.StatusBarHidden = false;
        }
    }
}
