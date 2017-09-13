using Android.App;
using Friendica_Mobile.PCL;
using System.Threading.Tasks;

namespace Friendica_Mobile
{
    internal class MessageDialogDroid : IMessageDialog
    {
        TaskCompletionSource<bool> tcs;
        public Activity Act { get; private set; }

        public int Result { get; set; }


        public MessageDialogDroid(Activity activity)
        {
            // get Android activity to show the dialog on
            Act = activity;
        }


        public async Task<int> ShowDialogAsync(string Title, string Message, string PositiveButtonText, 
                        string NegativeButtonText = null, string NeutralButtonText = null, 
                        uint DefaultIndex = 0, uint CancelIndex = 0)
        {
            // initialize from last dialog run
            Result = 0;
            tcs = new TaskCompletionSource<bool>();

            // Show the AlertDialog (no await possible so we use TaskCompletionSource to wait for user reaction)
            ShowDialog(Title, Message, PositiveButtonText, NegativeButtonText, NeutralButtonText, DefaultIndex, CancelIndex);
            await tcs.Task;

            // User has clicked now so we can return the selected option
            return Result;
        }


        private void ShowDialog(string Title, string Message, string PositiveButtonText,
                        string NegativeButtonText = null, string NeutralButtonText = null, 
                        uint DefaultIndex = 0, uint CancelIndex = 0)
        {
            // 1. Instantiate an AlertDialog.Builder with its constructor
            AlertDialog.Builder builder = new AlertDialog.Builder(Act);

            // 2. Chain together various setter methods to set the dialog characteristics
            builder.SetTitle(Title);
            builder.SetMessage(Message);
            builder.SetCancelable(false);
            builder.SetPositiveButton(PositiveButtonText, (sender, args) => { OkAction(); });
            builder.SetNegativeButton(NegativeButtonText, (sender, args) => { CancelAction(); });
            builder.SetNeutralButton(NeutralButtonText, (sender, args) => { NeutralAction(); });
            
            // 3. Get the AlertDialog from create()
            builder.Show();
        }


        private void OkAction()
        {
            Result = 0;
            tcs.SetResult(true);
        }


        private void CancelAction()
        {
            Result = 1;
            tcs.SetResult(true);
        }


        private void NeutralAction()
        {
            Result = 2;
            tcs.SetResult(true);
        }

    }
}
