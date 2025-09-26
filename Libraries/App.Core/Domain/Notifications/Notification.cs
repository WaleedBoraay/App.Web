using System;
using App.Core.Domain.Common;
using App.Core.Domain.Registrations;
using App.Core.Domain.Users;

namespace App.Core.Domain.Notifications
{
    public partial class Notification : BaseEntity
    {
        /// <summary>
        /// Related registration (if applicable)
        /// </summary>
        public int? RegistrationId { get; set; }
        public virtual Registration Registration { get; set; }

        /// <summary>
        /// The type of event that triggered this notification
        /// </summary>
        public int EventTypeId { get; set; }
        public NotificationEvent EventType { get; set; }

        /// <summary>
        /// The recipient user
        /// </summary>
        public int RecipientUserId { get; set; }
        public virtual AppUser RecipientUser { get; set; }

        /// <summary>
        /// The user who triggered the notification
        /// </summary>
        public int TriggeredByUserId { get; set; }
        public virtual AppUser TriggeredByUser { get; set; }

        /// <summary>
        /// The notification message body
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Delivery channel (Email, SMS, InApp, Push)
        /// </summary>
        public int ChannelId { get; set; }
        public NotificationChannel Channel { get; set; }

        /// <summary>
        /// Delivery status (Pending, Sent, Failed)
        /// </summary>
        public int StatusId { get; set; }
        public NotificationDeliveryStatus Status { get; set; }

        /// <summary>
        /// UTC timestamp when the notification was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        public string EntityName { get; set; }
        public int EntityId { get; set; }

        public NotificationType Type { get; set; }
    }
}
