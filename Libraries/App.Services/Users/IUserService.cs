using App.Core.Domain.Audit;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using App.Services.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Users
{
    /// <summary>
    /// User management and identity service.
    /// Handles CRUD, authentication helpers, roles, preferences, and audit trail.
    /// </summary>
    public interface IUserService
    {
        #region CRUD

        Task<AppUser> GetByIdAsync(int id);

        Task<IList<AppUser>> GetAllAsync(bool onlyActive = true);

        Task<AppUser> InsertAsync(AppUser user, string password, int createdByUserId);
        Task<AppUser> InsertAsync(AppUser user, string password);

        Task<AppUser> UpdateAsync(AppUser user);

        Task DeleteAsync(int id);

        #endregion

        #region Identity

        Task<AppUser> GetByEmailAsync(string email);

        Task<AppUser> GetByUsernameAsync(string username);

        #endregion

        #region Security

        Task ActivateAsync(int userId);

        Task DeactivateAsync(int userId);

        /// <summary>
        /// Resets the user password. Password should be processed via IEncryptionService.
        /// </summary>
        Task ResetPasswordAsync(int userId, string newPassword, int performedByUserId);

        Task LockUserAsync(int userId, int? minutes = null);

        Task UnlockUserAsync(int userId);

        Task<bool> CheckPasswordAsync(AppUser user, string password);

        Task<bool> IsPasswordExpiredAsync(AppUser user);

        #endregion

        #region Roles

        Task<IList<string>> GetRolesAsync(int userId);

        Task<IList<Role>> GetRoleEntitiesAsync(int userId);

        Task<bool> IsInRoleAsync(int userId, string roleSystemName);

        Task AddToRoleAsync(int userId, int roleId);

        Task RemoveFromRoleAsync(int userId, int roleId);

        Task<AppUser> GetRegulatorUserAsync();

        #endregion

        #region Status Queries

        Task<IList<AppUser>> GetLockedUsersAsync();

        Task<IList<AppUser>> GetInactiveUsersAsync();

        #endregion

        #region Preferences

        Task<UserPreference> GetPreferencesAsync(int userId);

        Task UpdatePreferencesAsync(UserPreference preference);

        #endregion

        #region Audit

        Task<IList<AuditTrail>> GetUserAuditTrailAsync(int userId, int pageIndex = 0, int pageSize = 50);

        #endregion

        Task RecordLoginFailureAsync(AppUser user);
        Task ResetFailedAttemptsAsync(AppUser user);
        bool IsUserLockedOut(AppUser user);

        Task AddUserLogAsync(int userId, string action, string clientIp);

        Task<bool> ValidatePasswordAsync(AppUser user, string password);
        Task SetPasswordAsync(AppUser user, string newPassword);
    }
}
