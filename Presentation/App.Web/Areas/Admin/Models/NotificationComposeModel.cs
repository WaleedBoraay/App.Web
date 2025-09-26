using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models
{
    public class NotificationComposeModel
    {
        [Display(Name="Event Type")] public string EventType { get; set; }
        public int RecipientUserId { get; set; } // optional if RegistrationId provided
        public int? RegistrationId { get; set; }
        public string Channel { get; set; } = "InApp";
        public string Message { get; set; }
    }
}
