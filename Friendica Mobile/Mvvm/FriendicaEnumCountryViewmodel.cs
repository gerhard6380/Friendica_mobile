using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.Mvvm
{
    public class FriendicaEnumCountryViewmodel : FriendicaEnumBaseViewmodel
    {

        public FriendicaEnumCountryViewmodel()
        {
            Type = typeof(FriendicaCountry);
            EmptyType = "";
            UnknownType = "Friendicaland";
            PrepareDictionaries();           
        }
    }
}
