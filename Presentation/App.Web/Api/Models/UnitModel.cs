using App.Core.Domain.Registrations;

namespace App.Web.Api.Models
{
    public class UnitModel
    {
        public int Id { get; set; }
        public string UnitName { get; set; }
        public string UnitDescription { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        public string SectorName { get; set; }

        public int ContactId { get; set; }
        public string ContactName { get; set; }
        public int ContactTypeId { get; set; }
        public ContactType ContactTypes
        {
            get => (ContactType)ContactTypeId;
            set => ContactTypeId = (int)value;
		}
	}
}
