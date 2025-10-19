using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Institutes
{
    public class InstituteEditModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string BusinessPhoneNumber { get; set; }
        public string LicenseNumber { get; set; }
        public int LicenseSectorId { get; set; }
        public int FinancialDomainId { get; set; }
        public DateTime? LicenseIssueDate { get; set; } = DateTime.UtcNow;
        public DateTime? LicenseExpiryDate { get; set; } = DateTime.UtcNow.AddYears(1);
        public int CountryId { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AvailableLicenseSectors { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AvailableFinancialDomains { get; set; } = new List<SelectListItem>();
    }
}
