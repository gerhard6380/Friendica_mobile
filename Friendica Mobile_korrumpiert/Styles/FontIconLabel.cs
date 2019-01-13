using Xamarin.Forms;

namespace Friendica_Mobile.Styles
{
    public class FontIconLabel : Label
    {
        public static readonly string FontIconName = "Segoe MDL2 Assets";

        public FontIconLabel()
        {
            FontFamily = FontIconName;
        }

       public FontIconLabel(string fontIconLabel = null)
        {
            FontFamily = FontIconName;
            Text = fontIconLabel;
        }
    }

    public static class Icon
    {
        public static readonly string FIHamburger = "\uE700"; // symbol showing 3 horizontal lines indicating the menu
        public static readonly string FIMore = "\uE712"; // symbol showing 3 dots to indicate more elements
        public static readonly string FIBackArrow = "\uE72B"; // arrow to left indicating back navigation
        public static readonly string FINavArrowUp = "\uE010"; // simple arrow up indicating that user can scroll up
        public static readonly string FINavArrowDown = "\uE011"; // simple arrow down indicating that user can scroll down
        public static readonly string FIHome = "\uEC25"; // symbol showing an open map with a user symbol, indicating the user's own posts
        public static readonly string FINetwork = "\uE8A1"; // symbol like a website indicating the network's posts
        public static readonly string FINewsfeed = "\uEC05"; // symbol like a radio antenna indicating the newsfeed posts
        public static readonly string FIContactGroup = "\uE716"; // symbol showing two generic user icons indicating contacts
        public static readonly string FIMessage = "\uE715"; // symbol of an envelope indicating messages
        public static readonly string FIContact = "\uE77B"; // symbol showing a generic user icon
        public static readonly string FICamera = "\uE722"; // symbol of a camera indicating photo module
        public static readonly string FIVideoCamera = "\uE714"; // symbol of a video camera indicating videos
        public static readonly string FICalendar = "\uE787"; // symbol of a calendar sheet to indicate event calendars
        public static readonly string FISheet = "\uE7C3"; // symbol of a blank sheet to indicate personal notes
        public static readonly string FISettings = "\uE713"; // symbol with a gearwheel
        public static readonly string FIPlane = "\uE709"; // symbol of a plane indicating the flight mode to disable notifications
        public static readonly string FIHelp = "\uE9CE"; // question mark in a circle to indicate the help section
        public static readonly string FIInfo = "\uE946"; // "i" in a circle to indicate further information
        public static readonly string FIExit = "\uE805"; // symbol of a moving man to indicate the exit button from the app

        public static readonly string FICancelCircle = "\uEA39"; // "x" in a circle 
        public static readonly string FIUserAdmin = "\uEF58"; // symbol showing a user icon with a small gearwheel bottom right
        public static readonly string FIAddUser = "\uE8FA"; // symbol showing a user icon with a small plus sign bottom right
        public static readonly string FIAdd = "\uE710"; // symbol showing a big plus sign
        public static readonly string FIEdit = "\uE70F"; // symbol showing a pencil indicating that user can edit something
        public static readonly string FIDelete = "\uE74D"; // symbol showing a trash can indicating that user can delete the item
        public static readonly string FIKey = "\uE8D7"; // symbol showing a key for the registry token
        public static readonly string FISave = "\uE74E"; // symbol of a disk to save changes
        public static readonly string FICancel = "\uE711"; // symbol of an X for cancelling current activity 
        public static readonly string FIQRCode = "\uE8B3"; // symbol of a 2x2 table surrounded by dots simulating a QR code to scan
        public static readonly string FIRolesAdmin = "\uE7EF"; // symbol of a square with a shield indicating defending ACL
    }
}
