using Friendica_Mobile.PCL.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL.Viewmodels
{
    public class TestViewmodel
    {
        private IMessageDialog _messageDialog;

        public TestViewmodel(IMessageDialog dialog, ILocalize localizeHelper)
        {
            AppResources.Culture = localizeHelper.GetCurrentCultureInfo();
            _messageDialog = dialog;
        }

        public void Testen()
        {
            var test = AppResources.buttonCancel;
        }

        public async void ShowDialog()
        {
            string errorStr = AppResources.messageDialogAppContentConversionError;
            await _messageDialog.ShowDialogAsync(null, errorStr, AppResources.buttonOK, AppResources.buttonCancel, null, 0, 1);

            var result = _messageDialog.Result;

            await _messageDialog.ShowDialogAsync(null, String.Format("Ergebnis der Abfrage: {0}", result.ToString()), AppResources.buttonOK);
        }
    }
}
