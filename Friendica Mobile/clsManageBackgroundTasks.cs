using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Friendica_Mobile
{
    public class clsManageBackgroundTasks
    {
        public string notificationTaskName = "FriendicaNotificationBackgroundTask";
        public string notificationReactionTaskName = "NotificationReactionBackgroundTask";
        public string notificationHistoryTaskname = "NotificationHistoryBackgroundTask";

        public void ActivateNotifications()
        {
            RegisterBackgroundTasks();
        }


        public void DeactivateNotifications()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == notificationTaskName || task.Value.Name == notificationReactionTaskName || task.Value.Name == notificationHistoryTaskname)
                    task.Value.Unregister(true); 
            }
        }


        private async void RegisterBackgroundTasks()
        {
            var result = await BackgroundExecutionManager.RequestAccessAsync();

            if (result == BackgroundAccessStatus.Denied)
                DeactivateNotifications();
            else
            {
                var notificationTaskRegistered = false;
                var notificationReactionTaskRegistered = false;
                var notificationHistoryTaskRegistered = false;

                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == notificationTaskName)
                    {
                        notificationTaskRegistered = true;
                        break;
                    }
                    if (task.Value.Name == notificationReactionTaskName)
                    {
                        notificationReactionTaskRegistered = true;
                        break;
                    }
                    if (task.Value.Name == notificationHistoryTaskname)
                    {
                        notificationHistoryTaskRegistered = true;
                        break;
                    }
                }

                if (!notificationTaskRegistered)
                {
                    var builder = new BackgroundTaskBuilder();
                    builder.Name = notificationTaskName;
                    builder.TaskEntryPoint = "BackgroundTasks.NotificationBackgroundTask";
                    builder.SetTrigger(new TimeTrigger((uint)App.Settings.NotificationFreshnessTime, false));
                    builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                    BackgroundTaskRegistration task = builder.Register();
                }

                if (!notificationReactionTaskRegistered)
                {
                    var builder = new BackgroundTaskBuilder();
                    builder.Name = notificationReactionTaskName;
                    builder.TaskEntryPoint = "BackgroundTasks.NotificationReactionBackgroundTask";
                    builder.SetTrigger(new ToastNotificationActionTrigger());
                    BackgroundTaskRegistration task = builder.Register();
                }

                if (!notificationHistoryTaskRegistered)
                {
                    var builder = new BackgroundTaskBuilder();
                    builder.Name = notificationHistoryTaskname;
                    builder.TaskEntryPoint = "BackgroundTasks.NotificationHistoryBackgroundTask";
                    builder.SetTrigger(new ToastNotificationHistoryChangedTrigger());
                    BackgroundTaskRegistration task = builder.Register();
                }

            }
        }

    }
}
