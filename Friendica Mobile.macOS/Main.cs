using AppKit;

namespace Friendica_Mobile.macOS
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.SharedApplication.Delegate = new AppDelegate();
            // to avoid errors when trying to load an image where no image is available
            ObjCRuntime.Class.ThrowOnInitFailure = false;
            NSApplication.Main(args);
        }
    }
}
