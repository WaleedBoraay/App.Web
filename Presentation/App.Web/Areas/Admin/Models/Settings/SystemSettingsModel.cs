namespace App.Web.Areas.Admin.Models.Settings
{
    public class SystemSettingsModel
    {
        public string ApplicationName { get; set; }
        public string DefaultTimeZone { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableAuditTrail { get; set; }
    }
}
