using App.Core.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Organization
{
    public class Unit : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
