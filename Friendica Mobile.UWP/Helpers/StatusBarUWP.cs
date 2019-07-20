using System;
using Friendica_Mobile.UWP;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBarUWP))]
namespace Friendica_Mobile.UWP
{
    public class StatusBarUWP : IStatusBar
    {
        public void HideStatusBar()
        {
        }

        public void ShowStatusBar()
        {
        }
    }
}
