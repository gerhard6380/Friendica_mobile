using System;
using Friendica_Mobile.macOS;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBarMacOS))]
namespace Friendica_Mobile.macOS
{
    public class StatusBarMacOS : IStatusBar
    {
        public void HideStatusBar()
        {
        }

        public void ShowStatusBar()
        {
        }
    }
}
