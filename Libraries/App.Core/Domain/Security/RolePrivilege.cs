using System;

namespace App.Core.Domain.Security
{
    /// <summary>
    /// Many-to-many mapping between roles and privileges.
    /// </summary>
    public class RolePrivilege : BaseEntity
    {
        public int RoleId { get; set; }
        public int PrivilegeId { get; set; }
    }
}
