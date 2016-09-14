using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.UI.Popups;

namespace Friendica_Mobile
{
    public class clsDetectTrial
    {
        ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
        private LicenseInformation _licenseInformation;

        public clsDetectTrial()
        {
            GetLicense();
        }

        private void GetLicense()
        {
            try
            {
#if DEBUG
                _licenseInformation = CurrentAppSimulator.LicenseInformation;
#else
                _licenseInformation = CurrentApp.LicenseInformation;
#endif
                //App.Settings.IsTrial = licenseInformation.IsTrial;
                App.Settings.PaidForRemovingAds = _licenseInformation.ProductLicenses["RemoveAdvertising"].IsActive;
            }
            catch
            {
                // folgender Eintrag muss bei Publizierung im Store deaktiviert sein
                //App.Settings.IsTrial = false;
            }
        }

        public async Task BuyFeature()
        {
            GetLicense();
            if (!_licenseInformation.ProductLicenses["RemoveAdvertising"].IsActive)
            {
                try
                {
#if DEBUG
                    var result = await CurrentAppSimulator.RequestProductPurchaseAsync("RemoveAdvertising");
#else
                    var result = await CurrentApp.RequestProductPurchaseAsync("RemoveAdvertising");
#endif
                    switch (result.Status)
                    {
                        case ProductPurchaseStatus.Succeeded:
                        case ProductPurchaseStatus.AlreadyPurchased:
                            App.Settings.PaidForRemovingAds = true;
                            break;
                        default:
                            App.Settings.PaidForRemovingAds = false;
                            break;
                    }
                }
                catch 
                {
                    App.Settings.PaidForRemovingAds = false;
                }
            }
        }

    }  // END OF public class clsDetectTrial

}  // END OF namespace Friendica_Mobile
