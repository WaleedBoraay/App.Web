using System.Collections.Generic;
using App.Core.Domain.Users;

namespace App.Web.Areas.Admin.Models
{
    public class EditUserRolesModel
    {
        // --- For Users/EditRoles.cshtml (by user) ---
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<RoleEntry> Roles { get; set; } = new();

        public class RoleEntry
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            /// <summary>
            /// Canonical selection flag.
            /// </summary>
            public bool Selected { get; set; }

            /// <summary>
            /// Backward-compat alias used by some views (binds to the same flag).
            /// </summary>
            public bool IsAssigned
            {
                get => Selected;
                set => Selected = value;
            }
        }

        // --- For Roles/ManageUsers.cshtml (by role) ---
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<int> UserIds { get; set; } = new();
        public List<AppUser> AllUsers { get; set; } = new();
    }
}
