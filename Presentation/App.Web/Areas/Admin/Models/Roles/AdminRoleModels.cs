using System;
using System.Collections.Generic;

namespace App.Core.Domain.Admin.Models.Roles
{
    public class RoleListModel
    {
        public IList<RoleSummaryModel> Items { get; set; } = new List<RoleSummaryModel>();
    }

    public class RoleSummaryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class PermissionSummaryModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Category { get; set; }
    }

    public class RoleMatrixModel
    {
        public IList<RoleSummaryModel> Roles { get; set; } = new List<RoleSummaryModel>();
        public IList<PermissionSummaryModel> Permissions { get; set; } = new List<PermissionSummaryModel>();
        public IDictionary<int, IList<int>> RolePermissions { get; set; } = new Dictionary<int, IList<int>>();
    }

    public class AssignRolePrivilegeModel
    {
        public int RoleId { get; set; }
        public int PrivilegeId { get; set; }
    }
}
