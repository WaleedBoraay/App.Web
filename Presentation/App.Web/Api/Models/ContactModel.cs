using App.Core.Domain.Registrations;
using App.Web.Areas.Admin.Models;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Api.Models
{
	public class ContactModel
	{
		public ContactModel()
		{
			countries = new List<CountryModel>();
		}
		public int? Id { get; set; }

		public int RegistrationId { get; set; }

		[Display(Name = "Contact Type")]
		public int ContactTypeId { get; set; }

		[Display(Name = "Job Title")]
		public string? JobTitle { get; set; }

		[Required]
		[Display(Name = "First Name")]
		public string? FirstName { get; set; }

		[Display(Name = "Middle Name")]
		public string? MiddleName { get; set; }

		[Display(Name = "Last Name")]
		public string? LastName { get; set; }

		[Display(Name = "Contact Phone")]
		public string? ContactPhone { get; set; }

		[Display(Name = "Business Phone")]
		public string? BusinessPhone { get; set; }

		[EmailAddress]
		[Display(Name = "Email")]
		public string? Email { get; set; }

		public DateTime? CreatedOnUtc { get; set; }
		public DateTime? UpdatedOnUtc { get; set; }

		[Display(Name = "Nationality Country")]
		public int NationalityCountryId { get; set; }
		public IList<CountryModel> countries { get; set; }
		public ContactType ContactTypes { get; set; }
    }
}
