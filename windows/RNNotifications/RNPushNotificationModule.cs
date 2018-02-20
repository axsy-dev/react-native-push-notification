using Newtonsoft.Json;
using ReactNative.Bridge;
using ReactNative.Modules.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;

namespace RNNotifications
{
    public sealed class RNPushNotificationModule : ReactContextNativeModuleBase
    {
        static EventHandler PushNotificationReceivedInBackground;

        static int currentid = 1;

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

        public static async void RegisterBackgroundNotificationTask()
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
            Debug.WriteLine(channel.Uri);

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
            var rawNotification = (RawNotification)sender;

            if (rawNotification != null)
            {
                var notification = JsonConvert.DeserializeObject<Notification>(rawNotification.Content);
                notification.payload.id = currentid;
                RNPushNotificationHelper.SendToast(notification);
                RNPushNotificationHelper.SetBadgeNumber(notification.badge);
                currentid++;
            }
        }

        [ReactMethod]
        public void requestPermissions(string senderId)
        {
            Debug.WriteLine(senderId);
        }

        [ReactMethod]
        public void presentLocalNotification(Notification notification)
        {
            RNPushNotificationHelper.SendToast(notification);
        }

        [ReactMethod]
        public void scheduleLocalNotification(ScheduledNotification notification)
        {
            RNPushNotificationHelper.ScheduleNotification(notification);
        }

        [ReactMethod]
        public void getInitialNotification(IPromise promise)
        {
            // TODO: Get this done correctly, needs to send back a notificaton if there is one to send back...
            promise.Resolve(new { });
        }

        [ReactMethod]
        public void setApplicationIconBadgeNumber(int badgeCount)
        {
            RNPushNotificationHelper.SetBadgeNumber(badgeCount);
        }

        [ReactMethod]
        public void cancelLocalNotifications()
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
        public void registerNotificationActions()
        {
            throw new NotImplementedException();
        }
    }

    public class Notification
    {
        public Payload payload { get; set; }

        public int badge { get; set; }
    }

    public class Payload
    {
        public string title { get; set; }

        public string body { get; set; }

        public int id { get; set; }
    }

    public class ScheduledNotification : Notification
    {
        public DateTime date { get; set; }
    }
}