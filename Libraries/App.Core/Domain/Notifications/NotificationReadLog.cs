using System;

namespace App.Core.Domain.Notifications
{
    /// <summary>
    /// Tracks when a user reads/dismisses a notification.
    /// </summary>
    public class NotificationReadLog : BaseEntity
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public DateTime ReadOnUtc { get; set; }
    }
}
