using System.Threading.Tasks;

namespace Core.Notifications
{
    public enum NotificationType
    {
        Info = 0,
        KycSucceess = 1,
        KycRestrictedArea = 2,
        KycNeedToFillDocuments = 3,

        TransctionFailed = 4,
        TransactionConfirmed = 5,

        AssetsCredited = 6,

        BackupWarning = 7,

        NeedTransactionSign = 8
    }

    public interface IAppNotifications
    {
        Task SendDataNotificationToAllDevicesAsync(string[] notificationIds, NotificationType type, string entity, string id = "");

        Task SendNotificationAsync(string[] clientIds, NotificationType type, string message);

        Task SendAssetsCreditedNotification(string[] notificationsIds, double amount, string assetId, string message);

        Task SendRawIosNotification(string notificationId, string payload);

        Task SendRawAndroidNotification(string notificationId, string payload);
    }
}