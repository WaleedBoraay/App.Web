using App.Core.Domain.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Security
{
    public interface IPermissionService
    {
        #region CRUD

        Task<Permission> GetBySystemNameAsync(string systemName);

        Task<IList<Permission>> GetAllAsync(bool onlyActive = true);

        Task<Permission> InsertAsync(Permission permission);

        Task<Permission> UpdateAsync(Permission permission);

        Task DeleteAsync(int permissionId);

        #endregion

        #region Role ↔ Permission

        Task<IList<string>> GetPermissionsForRoleAsync(int roleId);

        Task GrantPermissionToRoleAsync(int roleId, string permissionSystemName);

        Task RevokePermissionFromRoleAsync(int roleId, string permissionSystemName);

        #endregion

        #region User overrides

        Task<IList<string>> GetUserOverridesAsync(int userId, bool onlyGranted = false);

        Task SetUserOverrideAsync(int userId, string permissionSystemName, bool isGranted);

        Task RemoveUserOverrideAsync(int userId, string permissionSystemName);

        #endregion

        #region Authorization

        Task<bool> AuthorizeAsync(int userId, string permissionSystemName);

        #endregion
    }
}
