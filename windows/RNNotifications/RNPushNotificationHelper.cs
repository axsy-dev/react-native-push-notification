using System.IO;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace RNNotifications
{
    class RNPushNotificationHelper
    {
        internal static void SendToast(string message)
        {
            var doc = MakeDoc(message).Result;
            var notification = new ToastNotification(doc);

            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }

        static async Task<XmlDocument> MakeDoc(string content)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                await BuildNotificationXml(stream, content);
                stream.Position = 0;
                var xml = await reader.ReadToEndAsync();
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return doc;
            }
        }

        static async Task BuildNotificationXml(Stream stream, string content)
        {
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Async = true;

            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(stream, settings))
            {
                await writer.WriteStartElementAsync(null, "toast", null);
                await writer.WriteAttributeStringAsync(null, "activationType", null, "foreground");

                await writer.WriteStartElementAsync(null, "visual", null);
                await writer.WriteStartElementAsync(null, "binding", null);
                await writer.WriteAttributeStringAsync(null, "template", null, "ToastText01");

                await writer.WriteStartElementAsync(null, "text", null);
                await writer.WriteAttributeStringAsync(null, "id", null, "1");
                await writer.WriteStringAsync(content);
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
            var toast = new ScheduledToastNotification(MakeDoc(notification.message).Result, notification.date);
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
