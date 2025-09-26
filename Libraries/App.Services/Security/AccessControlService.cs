using System.Threading.Tasks;
using App.Core.Security;

namespace App.Services.Security
{
    public class AccessControlService : IAccessControlService
    {
        private readonly IPermissionService _permissionService;

        public AccessControlService(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<bool> CanAsync(int userId, string entity, CrudAction action)
        {
            string permission = action switch
            {
                CrudAction.Create => $"{entity}.Create",
                CrudAction.Read => $"{entity}.Read",
                CrudAction.Update => $"{entity}.Update",
                CrudAction.Delete => $"{entity}.Delete",
                _ => null
            };

            if (string.IsNullOrEmpty(permission))
                return false;

            return await _permissionService.AuthorizeAsync(userId, permission);
        }

        public async Task<bool> CanDoAsync(int userId, string permissionSystemName)
        {
            return await _permissionService.AuthorizeAsync(userId, permissionSystemName);
        }
    }
}
