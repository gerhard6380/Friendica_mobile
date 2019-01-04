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
        public static readonly string FIContact = "\uE77B"; // symbol showing a generic user icon
        public static readonly string FISettings = "\uE713"; // symbol with a gearwheel
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
