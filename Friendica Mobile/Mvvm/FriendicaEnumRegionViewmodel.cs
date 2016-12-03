using Friendica_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.Mvvm
{
    public class FriendicaEnumRegionViewmodel : FriendicaEnumBaseViewmodel
    {

        public FriendicaEnumRegionViewmodel(string country)
        {
            Type = typeof(FriendicaEnumRegionViewmodel);
            EmptyType = "";
            UnknownType = "";
            Parameter = country;
            PrepareDictionaries();           
        }
    }
}
