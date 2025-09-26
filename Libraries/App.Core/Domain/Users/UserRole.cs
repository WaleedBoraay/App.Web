using App.Core.Domain.Organization;
using App.Core.Domain.Security;
using System;

namespace App.Core.Domain.Users
{
    /// <summary>
    /// Many-to-many mapping between users and roles.
    /// </summary>
    public class UserRole : BaseEntity
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public virtual Role Role { get; set; }
        public virtual AppUser User { get; set; }

        // Optional Scope
        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public int? UnitId { get; set; }
        public virtual Unit Unit { get; set; }

        public int? SubUnitId { get; set; }
        public virtual SubUnit SubUnit { get; set; }
    }
}
