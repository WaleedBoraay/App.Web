using System;

namespace App.Core.Domain.Organization
{
    public class Department : BaseEntity
    {
        public string Name { get; set; }
        public int? ParentDepartmentId { get; set; }
        public Department ParentDepartment { get; set; }
        public ICollection<Department> SubDepartments { get; set; }
        public ICollection<Unit> Units { get; set; }
    }
}
