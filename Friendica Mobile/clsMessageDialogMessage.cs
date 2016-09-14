using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Friendica_Mobile
{
    public class MessageDialogMessage
    {
        public MessageDialogCallBack OkCallback { get; private set; }
        public MessageDialogCallBack CancelCallback { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string OkText { get; private set; }
        public string CancelText { get; private set; }
        public int Result { get; private set; }

        public MessageDialogMessage(string message, string title,
                                string okText, string cancelText,
                                MessageDialogCallBack okCallback = null,
                                MessageDialogCallBack cancelCallback = null,
                                object sender = null, object target = null)
        {
            Message = message;
            Title = title;
            OkText = okText;
            CancelText = cancelText;
            OkCallback = okCallback;
            CancelCallback = cancelCallback;
        }

        public async Task ShowDialog(uint defaultIndex, uint cancelIndex)
        {
            bool result = false;
            var dialog = new MessageDialog(Message, Title);
            if (!string.IsNullOrWhiteSpace(OkText))
                dialog.Commands.Add(new UICommand(OkText, cmd => result = true));
            if (!string.IsNullOrWhiteSpace(CancelText))
                dialog.Commands.Add(new UICommand(CancelText, cmd => result = false));
            dialog.DefaultCommandIndex = defaultIndex;
            dialog.CancelCommandIndex = cancelIndex;

            try
            {
                await dialog.ShowAsync();
                if (result && OkCallback != null)
                    await OkCallback();
                if (result && OkCallback == null)
                    Result = 0;
                if (!result && CancelCallback != null)
                    await CancelCallback();
                if (!result && CancelCallback == null)
                    Result = 1;
            }
            catch
            {
                ///
            }
        }

    }

        public delegate Task MessageDialogCallBack();

}
