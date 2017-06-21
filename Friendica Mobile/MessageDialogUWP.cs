using Friendica_Mobile.PCL;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Friendica_Mobile
{
    internal class MessageDialogUWP : IMessageDialog
    {
        public int Result { get; set; }


        public MessageDialogUWP()
        {
        }


        public async Task<int> ShowDialogAsync(string Title, string Message, string PositiveButtonText,
                        string NegativeButtonText = null, string NeutralButtonText = null,
                        uint DefaultIndex = 0, uint CancelIndex = 0)
        {
            // create dialog and add buttons as specified
            MessageDialog dialog;
            if (Message == null)
            {
                Result = 9;
                return Result;
            }
            else
            {
                dialog = (Title == null) ? new MessageDialog(Message) : new MessageDialog(Message, Title);
            }


            if (!string.IsNullOrWhiteSpace(PositiveButtonText))
                dialog.Commands.Add(new UICommand(PositiveButtonText, cmd => Result = 0));
            else
                dialog.Commands.Add(new UICommand("OK", cmd => Result = 0));

            if (!string.IsNullOrWhiteSpace(NegativeButtonText))
                dialog.Commands.Add(new UICommand(NegativeButtonText, cmd => Result = 1));

            // set default action for Enter or Esc key, ensure that no higher value than 1 can be specified
            dialog.DefaultCommandIndex = (NegativeButtonText == null && DefaultIndex > 0) ? 0 : ((DefaultIndex > 1) ? 1 : DefaultIndex);
            dialog.CancelCommandIndex = (NegativeButtonText == null && CancelIndex > 0) ? 0 : ((CancelIndex > 1) ? 1 : CancelIndex);

            try
            {
                // show dialog and return user reaction
                await dialog.ShowAsync();
                return Result;
            }
            catch
            {
                return 0;
            }
        }

    }
}
