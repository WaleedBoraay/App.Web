using App.Core.Domain.Common;

namespace App.Core.Domain.Security
{
    public partial class Permission : BaseEntity
    {
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Category { get; set; } // Example: Security, Configuration, Registration
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}