using System.Threading.Tasks;

namespace Friendica_Mobile
{
    public interface IMessageDialog
    {
        // value to hold the result (0 = positive/OK, 1 = negative/Cancel, 2 = neutrag [only Android])
        int Result { get; set; }

        // method to display the dialog, Android has a NeutralButton, but is deprecated
        Task<int> ShowDialogAsync(string Title, string Message, string PositiveButtonText, 
                            string NegativeButtonText = null, string NeutralButtonText = null, 
                            uint DefaultIndex = 0, uint CancelIndex = 0);
    }
}
