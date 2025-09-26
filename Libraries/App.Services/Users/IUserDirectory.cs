using System.Threading.Tasks;

namespace App.Services.Users
{
    /// <summary>
    /// Facade used by Admin/UserManagementController to manage user lifecycle and role assignments.
    /// Bridges to IUserService for activate/deactivate and handles role mapping.
    /// </summary>
    public interface IUserDirectory
    {
        Task<bool> ActivateAsync(int userId);
        Task<bool> DeactivateAsync(int userId);
        /// <summary>
        /// Assign roles to a user by role SystemName or Name. Replaces existing mappings.
        /// </summary>
        Task<bool> AssignRolesAsync(int userId, string[] roles);
    }
}
