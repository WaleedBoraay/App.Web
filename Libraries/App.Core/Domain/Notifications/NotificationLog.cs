using System;

namespace App.Core.Domain.Notifications
{
    /// <summary>
    /// Detailed notification delivery log (email, sms, etc.)
    /// </summary>
    public class NotificationLog : BaseEntity
    {
        public int NotificationId { get; set; }
        public string Channel { get; set; }
        public bool Success { get; set; }
        public string Response { get; set; }
        public DateTime SentOnUtc { get; set; }
    }
}
