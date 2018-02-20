using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace RNNotifications
{
    class RNPushNotificationHelper
    {
        internal static void SendToast(Notification notification)
        {
            var doc = MakeDoc(notification).Result;
            var toastNotification = new ToastNotification(doc)
            {
                Tag = notification.payload.id.ToString(),
                Group = "RNNotifications"
            };

            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        static async Task<XmlDocument> MakeDoc(Notification notification)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                await BuildNotificationXml(stream, notification);
                stream.Position = 0;
                var xml = await reader.ReadToEndAsync();
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return doc;
            }
        }

        static async Task BuildNotificationXml(Stream stream, Notification notification)
        {
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Async = true;

            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stream, settings))
            {
                await writer.WriteStartElementAsync(null, "toast", null);
                await writer.WriteAttributeStringAsync(null, "activationType", null, "foreground");
                await writer.WriteAttributeStringAsync(null, "launch", null, JsonConvert.SerializeObject(notification));

                await writer.WriteStartElementAsync(null, "visual", null);
                await writer.WriteStartElementAsync(null, "binding", null);
                await writer.WriteAttributeStringAsync(null, "template", null, "ToastText01");

                await writer.WriteStartElementAsync(null, "text", null);
                await writer.WriteAttributeStringAsync(null, "id", null, notification.payload.id.ToString());
                await writer.WriteStringAsync(notification.payload.body);
                await writer.WriteEndElementAsync();

                await writer.WriteEndElementAsync();
                await writer.FlushAsync();
            }
        }

        internal static void SetBadgeNumber(int badgeCount)
        {
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            // Set the value of the badge in the XML to our number
            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", badgeCount.ToString());

            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(badgeXml);

            // Create the badge updater for the application
            BadgeUpdater badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();

            // And update the badge
            badgeUpdater.Update(badge);
        }

        private static void clearBadge()
        {
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }

        internal static void ClearAll()
        {
            ToastNotificationManager.History.Clear();
            clearBadge();
        }

        internal static void ScheduleNotification(ScheduledNotification notification)
        {
            var toast = new ScheduledToastNotification(MakeDoc(notification).Result, notification.date);
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
        }

        internal static void ClearScheduled()
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();

            foreach (var notification in scheduled)
            {
                notifier.RemoveFromSchedule(notification);
            }
        }
    }
}