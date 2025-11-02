using App.Web.Areas.Admin.Models.Registrations;

namespace App.Web.Api.Models
{
	public class RegistrationModel
	{
		public int Id { get; set; }
		public string InstitutionName { get; set; }
		public string LicenseNumber { get; set; }
		public int LicenseSectorId { get; set; }
		public int FinancialDomainId { get; set; }
		public DateTime? IssueDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public int CountryId { get; set; }
		public string Address { get; set; }
		public IList<ContactModel> Contacts { get; set; }
		public IList<DocumentModel> Documents { get; set; }
	}
}
