using System.Threading.Tasks;

namespace Friendica_Mobile
{
    public static class StaticMessageDialog
    {
        public static IMessageDialog Dialog;

        public static async Task<int> ShowDialogAsync(string Title, string Message, string PositiveButtonText,
                            string NegativeButtonText = null, string NeutralButtonText = null,
                            uint DefaultIndex = 0, uint CancelIndex = 0)
        {
            await Dialog.ShowDialogAsync(Title, Message, PositiveButtonText, 
                NegativeButtonText, NeutralButtonText, 
                DefaultIndex, CancelIndex);
            return Dialog.Result;
        }
    }
}
