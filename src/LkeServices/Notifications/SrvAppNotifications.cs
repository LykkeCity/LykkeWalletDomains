using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Core.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LkeServices.Notifications
{
    public enum Device
    {
        Android, Ios
    }

    public interface IIosNotification { }

    public interface IAndroidNotification { }

    public class IosFields
    {
        [JsonProperty("alert")]
        public string Alert { get; set; }
        [JsonProperty("type")]
        public NotificationType Type { get; set; }
    }

    public class AssetsCreditedFields : IosFields
    {
        [JsonProperty("amount")]
        public double Amount { get; set; }
        [JsonProperty("assetId")]
        public string AssetId { get; set; }
    }

    public class IosNotification : IIosNotification
    {
        [JsonProperty("aps")]
        public IosFields Aps { get; set; }
    }

    public class AssetsCreditedNotification : IIosNotification
    {
        [JsonProperty("aps")]
        public AssetsCreditedFields Aps { get; set; }
    }

    public class AndroidPayloadFields
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("event")]
        public NotificationType Event { get; set; }

        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class AndoridPayloadNotification : IAndroidNotification
    {
        [JsonProperty("data")]
        public AndroidPayloadFields Data { get; set; }
    }


    public class SrvAppNotifications : IAppNotifications
    {
        private readonly string _connectionString;
        private readonly string _hubName;

        public SrvAppNotifications(string connectionString, string hubName)
        {
            _connectionString = connectionString;
            _hubName = hubName;
        }

        public async Task SendDataNotificationToAllDevicesAsync(string[] notificationIds, NotificationType type, string entity, string id = "")
        {
            var apnsMessage = new IosNotification
            {
                Aps = new IosFields
                {
                    Type = type
                }
            };

            var gcmMessage = new AndoridPayloadNotification
            {
                Data = new AndroidPayloadFields
                {
                    Entity = entity,
                    Event = type,
                    Id = id
                }
            };

            await SendIosNotificationAsync(notificationIds, apnsMessage);
            await SendAndroidNotificationAsync(notificationIds, gcmMessage);
        }

        public async Task SendNotificationAsync(string[] notificationIds, NotificationType type, string message)
        {
            var apnsMessage = new IosNotification
            {
                Aps = new IosFields
                {
                    Alert = message,
                    Type = type
                }
            };

            await SendIosNotificationAsync(notificationIds, apnsMessage);
        }

        public async Task SendAssetsCreditedNotification(string[] notificationsIds, double amount, string assetId, string message)
        {
            var apnsMessage = new AssetsCreditedNotification
            {
                Aps = new AssetsCreditedFields
                {
                    Alert = message,
                    Amount = amount,
                    AssetId = assetId,
                    Type = NotificationType.AssetsCredited
                }
            };

            await SendIosNotificationAsync(notificationsIds, apnsMessage);
        }

        public async Task SendRawIosNotification(string notificationId, string payload)
        {
            await SendRawNotificationAsync(Device.Ios, new[] { notificationId }, payload);
        }

        public async Task SendRawAndroidNotification(string notificationId, string payload)
        {
            await SendRawNotificationAsync(Device.Android, new[] { notificationId }, payload);
        }

        private async Task SendIosNotificationAsync(string[] notificationIds, IIosNotification notification)
        {
            await SendRawNotificationAsync(Device.Ios, notificationIds, notification.ToJson(ignoreNulls: true));
        }

        private async Task SendAndroidNotificationAsync(string[] notificationIds, IAndroidNotification notification)
        {
            await SendRawNotificationAsync(Device.Android, notificationIds, notification.ToJson(ignoreNulls: true));
        }

        private async Task SendRawNotificationAsync(Device device, string[] notificationIds, string payload)
        {
            try
            {
                notificationIds = notificationIds?.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                if (notificationIds != null && notificationIds.Any())
                {
                    var hub = CustomNotificationHubClient.CreateClientFromConnectionString(_connectionString, _hubName);

                    if (device == Device.Ios)
                        await hub.SendAppleNativeNotificationAsync(payload, notificationIds);
                    else
                        await hub.SendGcmNativeNotificationAsync(payload, notificationIds);
                }
            }
            catch (Exception)
            {
                //TODO: process exception
            }
        }
    }
}
