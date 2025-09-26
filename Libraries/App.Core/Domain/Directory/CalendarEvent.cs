using System;
using App.Core.Domain.Common;

namespace App.Core.Domain.Directory
{
    public partial class CalendarEvent : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public bool IsHoliday { get; set; }
        public string EventType { get; set; } // PublicHoliday, Deadline, Meeting
        public int CreatedByUserId { get; set; }
    }
}