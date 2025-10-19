using App.Core;
using App.Core.Domain.Notifications;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using App.Services.Audit;
using App.Services.Common;
using App.Services.Localization;
using App.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Security
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly ILocalizationService _localization;
        private readonly IAuditTrailService _auditTrailService;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;

        public RoleService(
            IRepository<Role> roleRepo,
            IRepository<UserRole> userRoleRepo,
            ILocalizationService localization,
            IAuditTrailService auditTrailService,
            IWorkContext workContext,
            INotificationService notificationService
            )
        {
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _localization = localization;
            _auditTrailService = auditTrailService;
            _workContext = workContext;
            _notificationService = notificationService;
        }

        #region CRUD

        public async Task<Role> GetByIdAsync(int roleId)
        {
            return await _roleRepo.GetByIdAsync(roleId);
        }

        public async Task<IList<Role>> GetAllAsync(bool onlyActive = true)
        {
            return await _roleRepo.GetAllAsync(q =>
                onlyActive ? q.Where(r => r.IsActive) : q);
        }

        public async Task<Role> InsertAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var exists = _roleRepo.Table.Any(r => r.SystemName == role.SystemName);
            if (exists)
                throw new InvalidOperationException("Role with the same SystemName already exists");

            role.CreatedOnUtc = DateTime.UtcNow;
            await _roleRepo.InsertAsync(role);

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(Role), role.Id, "*", null, $"Created Role {role.Name}", userId);

            // Notification
            await _notificationService.SendAsync(
                registrationId: null,
                eventType: NotificationEvent.RoleAssigned,
                triggeredByUserId: userId,
                recipientUserId: userId,
                channel: NotificationChannel.InApp,
                tokens: new Dictionary<string, string> { { "RoleName", role.Name } });

            return role;
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var oldRole = await _roleRepo.GetByIdAsync(role.Id);
            var oldName = oldRole?.Name;

            role.UpdatedOnUtc = DateTime.UtcNow;
            await _roleRepo.UpdateAsync(role);

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(Role), role.Id, "Name", oldName, role.Name, userId);

            return role;
        }

        public async Task DeleteAsync(int roleId)
        {
            var entity = await _roleRepo.GetByIdAsync(roleId);
            if (entity == null)
                throw new InvalidOperationException("Role not found");

            await _roleRepo.DeleteAsync(entity);

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(Role), entity.Id, "*", entity.Name, null, userId);
        }

        #endregion

        #region User ? Role

        public async Task<IList<Role>> GetRolesByUserIdAsync(int userId)
        {
            var query = from ur in _userRoleRepo.Table
                        join r in _roleRepo.Table on ur.RoleId equals r.Id
                        where ur.UserId == userId && r.IsActive
                        select r;

            return query.ToList();
        }

        public async Task AddUserToRoleAsync(int userId, int roleId)
        {
            var exists = _userRoleRepo.Table.Any(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (exists)
                throw new InvalidOperationException("User already assigned to role");

            await _userRoleRepo.InsertAsync(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });

            // ?? Audit
            var actorId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(UserRole), roleId, "UserId", null, userId.ToString(), actorId);
        }

        public async Task RemoveUserFromRoleAsync(int userId, int roleId)
        {
            var entity = _userRoleRepo.Table
                .FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (entity == null)
                throw new InvalidOperationException("User not assigned to role");

            await _userRoleRepo.DeleteAsync(entity);

            // ?? Audit
            var actorId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(UserRole), roleId, "UserId", userId.ToString(), null, actorId);
        }

        public async Task ClearRolesAsync(int userId)
        {
            var roles = await _userRoleRepo.GetAllAsync(q => q.Where(ur => ur.UserId == userId));
            foreach (var role in roles)
            {
                await _userRoleRepo.DeleteAsync(role);
            }
        }

        #endregion
    }
}
