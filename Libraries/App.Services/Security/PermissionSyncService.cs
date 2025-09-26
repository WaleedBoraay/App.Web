using System.Reflection;
using App.Core.Domain.Security;
using App.Services.Security;
using System.Threading.Tasks;

namespace App.Services.Security
{
    public interface IPermissionSyncService
    {
        Task SyncPermissionsAsync();
    }

    public class PermissionSyncService : IPermissionSyncService
    {
        private readonly IPermissionService _permissionService;

        public PermissionSyncService(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task SyncPermissionsAsync()
        {
            var constants = typeof(App.Core.Security.AppPermissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetValue(null)?.ToString())
                .Where(val => !string.IsNullOrEmpty(val))
                .ToList();

            var existing = await _permissionService.GetAllAsync(false);

            foreach (var systemName in constants)
            {
                if (!existing.Any(p => p.SystemName == systemName))
                {
                    await _permissionService.InsertAsync(new Permission
                    {
                        Name = systemName,            // ممكن بعدين نربط بالـ Localization
                        SystemName = systemName,
                        Category = systemName.Split('.')[0], // الجزء الأول من الاسم كـ Category
                        Description = $"Auto-added from AppPermissions ({systemName})",
                        IsActive = true
                    });
                }
            }
        }
    }
}
