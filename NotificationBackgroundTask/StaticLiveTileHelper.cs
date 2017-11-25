using Friendica_Mobile.PCL.Models;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System;

namespace BackgroundTasks
{
    public static class StaticLiveTileHelper
    {
        public static void EnableNotificationQueue()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }


        public static void CreateTileNotification(string xmlString)
        {
            var xml = new XmlDocument();
            xml.LoadXml(xmlString);
            var notification = new TileNotification(xml);
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(60);
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            }
            catch { }
        }


        public static void ClearNotifications()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }

    }
}
