using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Web.Areas.Admin.Models.Institutes
{
    public class BranchEditModel
    {
        public int Id { get; set; }
        public int InstitutionId { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int CountryId { get; set; }

        public IEnumerable<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
    }
}
