using Newtonsoft.Json;
using ReactNative.Bridge;
using ReactNative.Modules.Core;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;

namespace RNNotifications
{
    public sealed class RNPushNotificationModule : ReactContextNativeModuleBase
    {
        static EventHandler PushNotificationReceivedInBackground;

        public RNPushNotificationModule(ReactContext reactContext)
            : base(reactContext)
        {
            PushNotificationReceivedInBackground += OnPushNotificationReceivedInBackground;
        }

        public override string Name => "RNPushNotification";

        public override IReadOnlyDictionary<string, object> Constants
        {
            get
            {
                return new Dictionary<string, object>
                { };
            }
        }

        public static async void RegisterBackgroundTask()
        {
            var result = await BackgroundExecutionManager.RequestAccessAsync();

            var pushNotificationName = "PushNotificationTask";
            var taskRegistered = false;

            foreach (var bgItem in BackgroundTaskRegistration.AllTasks)
            {
                if (bgItem.Value.Name.Equals(pushNotificationName))
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = pushNotificationName;
                var trigger = new PushNotificationTrigger();
                builder.SetTrigger(trigger);
                BackgroundTaskRegistration task = builder.Register();
            }
        }

        public override async void Initialize()
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            channel.PushNotificationReceived += OnPushNotification;

            Context
                .GetJavaScriptModule<RCTDeviceEventEmitter>()
                .emit("remoteNotificationsRegistered", new { deviceToken = channel.Uri });
        }

        private void OnPushNotification(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            String notificationContent = string.Empty;

            switch (args.NotificationType)
            {
                case PushNotificationType.Toast:
                    notificationContent = args.ToastNotification.Content.GetXml();
                    break;
                case PushNotificationType.Tile:
                    notificationContent = args.TileNotification.Content.GetXml();
                    break;
                case PushNotificationType.Badge:
                    notificationContent = args.BadgeNotification.Content.GetXml();
                    break;
                case PushNotificationType.Raw:
                    notificationContent = args.RawNotification.Content;
                    Context
                        .GetJavaScriptModule<RCTDeviceEventEmitter>()
                        .emit("remoteNotificationReceived", new { content = notificationContent });
                    break;
                default:
                    break;
            }

            args.Cancel = true;
        }

        public static void HandlePushNotification(RawNotification notification)
        {
            PushNotificationReceivedInBackground?.Invoke(notification, new EventArgs());
        }

        static void OnPushNotificationReceivedInBackground(object sender, EventArgs args)
        {
            var notification = (RawNotification)sender;

            if (notification != null)
            {
                var jsonData = JsonConvert.DeserializeObject<PushNotification>(notification.Content);
                RNPushNotificationHelper.SendToast(jsonData.payload.title);
                RNPushNotificationHelper.SetBadgeNumber(jsonData.badge);
            }
        }

        [ReactMethod]
        public void requestPermissions(string senderId)
        {
            // Not required on Windows
        }

        [ReactMethod]
        public void presentLocalNotification(string message)
        {
            RNPushNotificationHelper.SendToast(message);
        }

        [ReactMethod]
        public void scheduleLocalNotification(ScheduledNotification notification)
        {
            RNPushNotificationHelper.ScheduleNotification(notification);
        }

        [ReactMethod]
        public void getInitialNotification(IPromise promise)
        {
            promise.Resolve(new { });
        }

        [ReactMethod]
        public void setApplicationIconBadgeNumber(int badgeCount)
        {
            RNPushNotificationHelper.SetBadgeNumber(badgeCount);
        }

        [ReactMethod]
        public void cancelLocalNotifications(object details)
        {
            RNPushNotificationHelper.ClearAll();
        }

        [ReactMethod]
        public void cancelAllLocalNotifications()
        {
            RNPushNotificationHelper.ClearAll();
            RNPushNotificationHelper.ClearScheduled();
        }

        [ReactMethod]
        public void registerNotificationActions(object details)
        {
            // Not implemented yet.
        }
    }

    internal class PushNotification
    {
        public Payload payload { get; set; }

        public int badge { get; set; }
    }

    internal class Payload
    {
        public string title { get; set; }

        public string body { get; set; }
    }

    public class Notification
    {
        public string title { get; set; }

        public string message { get; set; }
    }

    public class ScheduledNotification : Notification
    {
        public DateTime date { get; set; }
    }
}