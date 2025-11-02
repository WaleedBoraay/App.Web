using System;
using App.Core.Domain.Common;

namespace App.Core.Domain.Directory
{
    public partial class EventSchedule : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public bool IsAllDay { get; set; }
        public string Location { get; set; } = string.Empty;
        public EventType EventType { get; set; } // PublicHoliday, Deadline, Meeting     
        public string Color { get; set; } = "#3788d8";
        public bool NotifyUsers { get; set; }
        public DateTime? NotificationSentAt { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public enum EventType
    {
        PublicHoliday = 1,
        Appointment = 2,
        Meeting = 3,
        Task = 4,
        Deadline = 5,
        Other = 6
    }
}
