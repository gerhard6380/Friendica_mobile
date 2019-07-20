using System;
using Android.App;
using Android.Views;
using Friendica_Mobile.Droid;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBarDroid))]
namespace Friendica_Mobile.Droid
{
    public class StatusBarDroid : IStatusBar
    {
        WindowManagerFlags _originalFlags;

        public void HideStatusBar()
        {
            var activity = (Activity)Forms.Context;
            var attrs = activity.Window.Attributes;
            _originalFlags = attrs.Flags;
            attrs.Flags |= Android.Views.WindowManagerFlags.Fullscreen;
            activity.Window.Attributes = attrs;
        }

        public void ShowStatusBar()
        {
            var activity = (Activity)Forms.Context;
            var attrs = activity.Window.Attributes;
            attrs.Flags = _originalFlags;
            activity.Window.Attributes = attrs;
        }
    }
}
