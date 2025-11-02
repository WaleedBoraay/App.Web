using App.Core.Domain.Registrations;

namespace App.Web.Api.Models
{
    public class DepartmentModel
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentDescription { get; set; }
		public int SectorId { get; set; }
        public string SectorName { get; set; }
        public string SectorDescription { get; set; }

        public int ContactId { get; set; }
        public string ContactName { get; set; }

        public int ContactTypeId { get; set; }
		public ContactType ContactTypes
        {
            get => (ContactType) ContactTypeId;
            set => ContactTypeId = (int) value;
		}
	}
}
