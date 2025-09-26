using App.Core.Domain.Security;
using System.Threading.Tasks;

namespace App.Services.Security
{
    public interface IRoleTemplateSyncService
    {
        Task SyncRoleTemplatesAsync();
    }

    public class RoleTemplateSyncService : IRoleTemplateSyncService
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public RoleTemplateSyncService(IRoleService roleService, IPermissionService permissionService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
        }

        public async Task SyncRoleTemplatesAsync()
        {
            foreach (var kvp in RoleTemplates.RolePermissions)
            {
                var roleName = kvp.Key;
                var permissions = kvp.Value;

                // check if role exists
                var existingRoles = await _roleService.GetAllAsync(false);
                var role = existingRoles.FirstOrDefault(r => r.SystemName == roleName);

                if (role == null)
                {
                    role = new Role
                    {
                        Name = roleName,
                        SystemName = roleName,
                        Description = $"Default {roleName} role",
                        IsActive = true,
                        IsSystemRole = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    await _roleService.InsertAsync(role);
                }

                // assign permissions
                foreach (var perm in permissions)
                {
                    await _permissionService.GrantPermissionToRoleAsync(role.Id, perm);
                }
            }
        }
    }
}
