using System;

namespace App.Core.Domain.Security
{
    /// <summary>
    /// System role (e.g., Maker, Checker, Regulator, Auditor, Admin).
    /// </summary>
    public class Role : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SystemName { get; set; }


        /// <summary>
        /// Indicates whether the role is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether the role is a system role (cannot be deleted)
        /// </summary>
        public bool IsSystemRole { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
