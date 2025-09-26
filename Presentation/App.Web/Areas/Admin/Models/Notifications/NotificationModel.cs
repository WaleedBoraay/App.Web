namespace App.Web.Areas.Admin.Models.Notifications
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool IsRead { get; set; }
        public string TargetUrl { get; set; }
    }
}
