using System.Threading.Tasks;

namespace App.Services.Security
{
    public enum CrudAction { Create, Read, Update, Delete }

    public interface IAccessControlService
    {
        Task<bool> CanAsync(int userId, string entity, CrudAction action);

        Task<bool> CanDoAsync(int userId, string permissionSystemName);
    }

}
