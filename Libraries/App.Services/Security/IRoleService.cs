using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Security;
using App.Services.Common;

namespace App.Services.Security
{
    public interface IRoleService
    {
        // CRUD
        Task<Role> GetByIdAsync(int roleId);
        Task<IList<Role>> GetAllAsync(bool onlyActive = true);
        Task<Role> InsertAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task DeleteAsync(int roleId);

        Task<IList<Role>> GetRolesByUserIdAsync(int userId);
        Task AddUserToRoleAsync(int userId, int roleId);
        Task RemoveUserFromRoleAsync(int userId, int roleId);
        Task ClearRolesAsync(int userId);


    }
}
