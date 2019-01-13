using AppKit;
using Foundation;
using Friendica_Mobile;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Friendica_Mobile.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow window;

        public override NSWindow MainWindow
        {
            get 
            {
                return window;
            }
        }

        public AppDelegate()
        {
            // Titled verursacht Fehler auf macos

            //var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable;
            //var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Miniaturizable | NSWindowStyle.FullSizeContentView;
            var rect = new CoreGraphics.CGRect(100, 100, 1024, 768);

            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);

            window.Title = "Friendica Mobile";
            window.TitleVisibility = NSWindowTitleVisibility.Hidden;
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            NSApplication.SharedApplication.MainMenu = new NSMenu();
            Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);

            window.StyleMask = window.StyleMask | NSWindowStyle.Titled;
            window.Title = "Test";
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
