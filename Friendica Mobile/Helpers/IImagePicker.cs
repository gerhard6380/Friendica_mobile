using System.Threading.Tasks;

namespace Friendica_Mobile
{
    /// <summary>
    /// interface to implement platform-specific code for returning an image from picture gallery
    /// currently only implemented for macOS, because other platforms are supported Xamarin.Plugin.Media
    /// </summary>
    public interface IImagePicker
    {
        /// <summary>
        /// open file selector to get a picture file from filesystem (only jpg, png to be selectable)
        /// </summary>
        /// <returns>FileData class to hold stream data</returns>
        Task<FileData> PickImage();
    }
}
