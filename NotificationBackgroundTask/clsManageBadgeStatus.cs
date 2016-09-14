using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class clsManageBadgeStatus
    {
        //private int _currentBadgeValue;
        public int CurrentBadgeValue
        {
            get { return Convert.ToInt16(ApplicationData.Current.LocalSettings.Values["CurrentBadgeValue"]); }
            set { ApplicationData.Current.LocalSettings.Values["CurrentBadgeValue"] = value.ToString();
                //_currentBadgeValue = value;
            }
        }


        public clsManageBadgeStatus()
        {
            var current = ReadCurrentBadgeValue();
        }


        private int ReadCurrentBadgeValue()
        {
            // we cannot read from the badge directly, so we save the value to localsettings
            CurrentBadgeValue = Convert.ToInt16(ApplicationData.Current.LocalSettings.Values["CurrentBadgeValue"]);
            return CurrentBadgeValue;
        }


        private void SetBadgeNumber(int newNumber)
        {
            // save new badge number to local settings and then set badge on the new value
            CurrentBadgeValue = newNumber;
            var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            var badgeAttributes = badgeXml.GetElementsByTagName("badge");
            badgeAttributes[0].Attributes[0].InnerText = newNumber.ToString();
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
            ReadCurrentBadgeValue();
        }


        private async Task<bool> ClearBadgeNumberHelper()
        {
            SetBadgeNumber(0);

            // get all remaining notifications of app and remove them
            var history = ToastNotificationManager.History.GetHistory();
            foreach (var notification in history)
            {
                ToastNotificationManager.History.Remove(notification.Tag);
            }

            // get folder Toastimages and remove all containing files
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFolder toastimages = await localFolder.GetFolderAsync("Toastimages");
                var files = await toastimages.GetFilesAsync();
                foreach (var file in files)
                    await file.DeleteAsync();
                return true;
            }
            catch
            { return false; }
        }


        // set badge to zero, async task because we delete all toast images after setting badge to zero (= no notifications anymore)
        public IAsyncOperation<bool> ClearBadgeNumber()
        {
            return this.ClearBadgeNumberHelper().AsAsyncOperation();
        }

        public IAsyncOperation<bool> SetNewBadgeNumberAsync(int newNumber)
        {
            return this.SetNewBadgeNumber(newNumber).AsAsyncOperation();
        }

        private async Task<bool> SetNewBadgeNumber(int newNumber)
        {
            if (newNumber != 0 && newNumber > 0)
            {
                SetBadgeNumber(newNumber);
            }
            else
                await ClearBadgeNumber();
            return true;
        }


        public bool ReduceBadgeByOne()
        {
            int currentValue = ReadCurrentBadgeValue();
            if (currentValue > 0)
            {
                var newNumber = currentValue - 1;
                SetBadgeNumber(newNumber);
                return true;
            }
            else
                return false;
        }

    }
}
