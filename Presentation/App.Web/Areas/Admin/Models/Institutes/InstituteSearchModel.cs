namespace App.Web.Areas.Admin.Models.Institutes
{
    public class InstituteSearchModel
    {
        public string SearchName { get; set; }
        public string LicenseNumber { get; set; }
        public int? CountryId { get; set; }

        public IList<CountryModel> AvailableCountries { get; set; } = new List<CountryModel>();

        public IList<InstituteListModel> Results { get; set; } = new List<InstituteListModel>();
    }
}
