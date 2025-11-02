using App.Core.Domain.Registrations;
using App.Core.Domain.Users;
using System;

namespace App.Core.Domain.Organization
{
    public class Sector : BaseEntity
    {
        public string Name { get; set; }
        public string? SectorDescription { get; set; }
        public int? ParentDepartmentId { get; set; }
		public int ContactId { get; set; }
        public int ContactTypeId { get; set; }
        public ContactType ContactTypes { get; set; }
		public ICollection<Department> Departments { get; set; }
	}
}
