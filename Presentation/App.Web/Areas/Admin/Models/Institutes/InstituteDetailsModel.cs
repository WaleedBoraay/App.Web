using App.Web.Areas.Admin.Models.Registrations;

namespace App.Web.Areas.Admin.Models.Institutes
{
    public class InstituteDetailsModel
    {
        public int InstituteId { get; set; }
        public string InstituteName { get; set; }
        public string LicenseNumber { get; set; }
        public string CountryName { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; set; }
        public string BusinessPhoneNumber { get; set; }

        public RegistrationModel Registration { get; set; }

        public List<BranchModel> Branches { get; set; } = new();

        public IList<DocumentModel> Documents { get; set; } = new List<DocumentModel>();

        public IList<ContactModel> Contacts { get; set; } = new List<ContactModel>();
    }
}
