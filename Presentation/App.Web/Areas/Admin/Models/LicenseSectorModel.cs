namespace App.Web.Areas.Admin.Models
{
    public partial class LicenseSectorModel
    {
        // Keep existing property names used by the views
        public int LicenseSectorId { get; set; } // maps to Domain.Id
        public string Code { get; set; } // optional, not in domain; keep to avoid breaking views
        public string DisplayName { get; set; } // maps to Domain.Name
    }
}
