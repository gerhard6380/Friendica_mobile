using System.Globalization;

namespace Friendica_Mobile
{
    public interface ILocalize
    {
        // method to get the current CultureInfo from platform-specific code
        CultureInfo GetCurrentCultureInfo();

        // method to set the CultureInfo with platform-specific code (on Android necessary)
        void SetLocale(CultureInfo ci);
    }
}
