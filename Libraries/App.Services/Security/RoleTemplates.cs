using System.Collections.Generic;

namespace App.Services.Security
{
    public static class RoleTemplates
    {
        public const string Admin = "Administrator";
        public const string Maker = "Maker";
        public const string Checker = "Checker";
        public const string Regulator = "Regulator";
        public const string Inspector = "Inspector";

        // mapping Role → Permissions
        public static readonly Dictionary<string, string[]> RolePermissions = new()
        {
            {
                Admin, new[]
                {
                    App.Core.Security.AppPermissions.System_Admin,
                    App.Core.Security.AppPermissions.User_Create,
                    App.Core.Security.AppPermissions.User_Read,
                    App.Core.Security.AppPermissions.User_Update,
                    App.Core.Security.AppPermissions.User_Delete,
                    App.Core.Security.AppPermissions.Role_Manage,
                    App.Core.Security.AppPermissions.Permission_Manage,
                    App.Core.Security.AppPermissions.Registration_Create,
                    App.Core.Security.AppPermissions.Registration_Update,
                    App.Core.Security.AppPermissions.Registration_Approve,
                    App.Core.Security.AppPermissions.Registration_Audit,
                }
            },
            {
                Maker, new[]
                {
                    App.Core.Security.AppPermissions.Registration_Create,
                    App.Core.Security.AppPermissions.Registration_Update,
                    App.Core.Security.AppPermissions.Registration_Submit,
                    App.Core.Security.AppPermissions.Registration_Read
                }
            },
            {
                Checker, new[]
                {
                    App.Core.Security.AppPermissions.Registration_Validate,
                    App.Core.Security.AppPermissions.Registration_ReturnForEdit,
                    App.Core.Security.AppPermissions.Registration_Read
                }
            },
            {
                Regulator, new[]
                {
                    App.Core.Security.AppPermissions.Registration_Approve,
                    App.Core.Security.AppPermissions.Registration_Reject,
                    App.Core.Security.AppPermissions.Registration_Audit,
                    App.Core.Security.AppPermissions.Registration_Read
                }
            },
            {
                Inspector, new[]
                {
                    App.Core.Security.AppPermissions.Registration_Read,
                    App.Core.Security.AppPermissions.Registration_Audit
                }
            }
        };
    }
}
