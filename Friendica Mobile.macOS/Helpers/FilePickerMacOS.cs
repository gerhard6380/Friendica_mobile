using System.IO;
using System.Threading.Tasks;
using AppKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(Friendica_Mobile.macOS.FilePickerMacOS))]
namespace Friendica_Mobile.macOS
{
    public class FilePickerMacOS : IImagePicker
    {
        public Task<FileData> PickImage()
        {
            var openPanel = new NSOpenPanel();
            // currently we do not need a multiple selection of files
            openPanel.CanChooseFiles = true;
            openPanel.AllowsMultipleSelection = false;
            openPanel.CanChooseDirectories = false;
            // only allow selection of png and jpg, maybe later we could add gif's (not yet sure if working on Friendica at the moment)
            openPanel.AllowedFileTypes = new string[] { "png", "jpg", "jpeg" };

            FileData data = null;

            // open dialog to user for selecting file from mac filesystem
            var result = openPanel.RunModal();

            // do nothing on cancellation = return null
            if (result == 1)
            {
                var url = openPanel.Urls[0];
                var fileName = openPanel.Filenames[0];

                if (url != null)
                {
                    var path = url.Path;
                    data = new FileData(path, fileName, () => File.OpenRead(path));
                }
            }

            return Task.FromResult(data);
        }
    }
}
