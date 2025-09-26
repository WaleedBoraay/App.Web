using App.Web.Areas.Admin.Models.Registrations;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Institutes
{
    public class InstituteListModel
    {
        public int InstituteId { get; set; }
        public string InstituteName { get; set; }
        [Phone]
        public string BusinessPhoneNumber { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public string CountryName { get; set; }
        public bool IsActive { get; set; }

        public RegistrationModel Registration { get; set; }
        public List<BranchModel> Branches { get; set; } = new();
        public IList<DocumentModel> Documents { get; set; } = new List<DocumentModel>();
    }
}
