using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Audit
{
    public enum AuditActionType
    {
        Create,
        Update,
        Delete,
        Login,
        Logout,
        PasswordChange,
        RoleChange,
        PermissionChange,
        UserRegistration,
        DataExport,
        DataImport,
        FailedLogin,
        ProfileUpdate,
        UserLogin
    }

}
