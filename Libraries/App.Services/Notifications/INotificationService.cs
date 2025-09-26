using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Notifications;

namespace App.Services.Notifications
{
    public interface INotificationService
    {
        Task SendAsync(
            int? registrationId,
            NotificationEvent eventType,
            int triggeredByUserId,
            int recipientUserId,
            NotificationChannel channel = NotificationChannel.InApp,
            IDictionary<string, string> tokens = null);

        Task<IList<NotificationLog>> GetLogsByUserAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAsReadAsync(int notificationId);
        Task RetryFailedAsync(int maxAttempts = 3);
        Task<IList<(Notification Notification, bool IsRead)>> GetInboxAsync(int userId, bool onlyUnread = false);

        //Show notifications to user
        Task NotifySuccessAsync(string resourceKey);
        Task NotifyErrorAsync(string resourceKey);
        Task NotifyWarningAsync(string resourceKey);
        Task NotifyInfoAsync(string resourceKey);

    }
}
