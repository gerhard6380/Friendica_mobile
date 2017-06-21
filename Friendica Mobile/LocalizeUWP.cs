using Friendica_Mobile.PCL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace Friendica_Mobile
{
    class LocalizeUWP : ILocalize
    {
        public void SetLocale(CultureInfo ci)
        {

        }

        public CultureInfo GetCurrentCultureInfo()
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (language == "de" || language == "en" || language == "es"
                    || language == "fr" || language == "it" || language == "pt")
            {
                return new CultureInfo(CultureInfo.CurrentUICulture.Name);
            }
            else
            {
                return new CultureInfo("en");
            }
        }
    }
}
