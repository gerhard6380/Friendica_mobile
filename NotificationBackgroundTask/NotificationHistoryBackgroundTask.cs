using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace BackgroundTasks
{
    public sealed class NotificationHistoryBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        clsManageBadgeStatus Badge = new clsManageBadgeStatus();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // get deferral for async operations
            _deferral = taskInstance.GetDeferral();

            var history = taskInstance.TriggerDetails as ToastNotificationHistoryChangedTriggerDetail;
            if (history != null)
            {
                switch (history.ChangeType)
                {
                    case ToastHistoryChangedType.Added:
                        await Badge.SetNewBadgeNumberAsync(ToastNotificationManager.History.GetHistory().Count);
                        break;
                    case ToastHistoryChangedType.Removed:
                        await Badge.SetNewBadgeNumberAsync(ToastNotificationManager.History.GetHistory().Count);
                        break;
                    default:
                        break;
                }
                _deferral.Complete();
            }
            else
                _deferral.Complete();
        }

    }

}
