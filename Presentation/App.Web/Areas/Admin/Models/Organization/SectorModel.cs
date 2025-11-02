using App.Core.Domain.Registrations;
using App.Core.Domain.Users;

namespace App.Web.Areas.Admin.Models.Organization
{
    public class SectorModel
    {
        public int Id { get; set; }
        public string? SectorName { get; set; }
        public string? SectorDescription { get; set; }
        public int ContactId { get; set; }
		public IList<Contact>? Contacts { get; set; }

        public int ContactTypeId { get; set; }
        public ContactType ContactTypes
        {
            get => (ContactType)ContactTypeId;
            set => ContactTypeId = (int)value;
		}



	}
}
