namespace App.Web.Areas.Admin.Models
{
    public class NotificationSettingsModel
    {
        public bool EnableEmail { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableInApp { get; set; }
    }
}
