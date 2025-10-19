using App.Core.Domain.Users;
using System;

namespace App.Core.Domain.Organization
{
    public class Sector : BaseEntity
    {
        public string Name { get; set; }
        public string? SectorDescription { get; set; }
        public int? ParentDepartmentId { get; set; }
        public ICollection<Department> Departments { get; set; }
	}
}
