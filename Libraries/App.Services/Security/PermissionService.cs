using App.Core;
using App.Core.Domain.Notifications;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using App.Services.Audit;
using App.Services.Common;
using App.Services.Localization;
using App.Services.Notifications;
using Microsoft.Graph.Models.TermStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Security
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly IRepository<UserPermissionOverride> _userOverrideRepo;
        private readonly ILocalizationService _localization;
        private readonly IAuditTrailService _auditTrailService;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;

        public PermissionService(
            IRepository<Permission> permissionRepo,
            IRepository<RolePermission> rolePermissionRepo,
            IRepository<UserRole> userRoleRepo,
            IRepository<UserPermissionOverride> userOverrideRepo,
            ILocalizationService localization,
            IAuditTrailService auditTrailService,
            IWorkContext workContext,
            INotificationService notificationService
            )
        {
            _permissionRepo = permissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _userRoleRepo = userRoleRepo;
            _userOverrideRepo = userOverrideRepo;
            _localization = localization;
            _auditTrailService = auditTrailService;
            _workContext = workContext;
            _notificationService = notificationService;
        }

        #region CRUD

        public async Task<Permission> GetBySystemNameAsync(string systemName)
        {
            var list = await _permissionRepo.GetAllAsync(q => q.Where(p => p.SystemName == systemName));
            return list.FirstOrDefault();
        }

        public async Task<IList<Permission>> GetAllAsync(bool onlyActive = true)
        {
            return await _permissionRepo.GetAllAsync(q =>
                onlyActive ? q.Where(p => p.IsActive) : q);
        }

        public async Task<Permission> InsertAsync(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            var existing = await GetBySystemNameAsync(permission.SystemName);
            if (existing != null)
                throw new InvalidOperationException("Permission already exists");

            await _permissionRepo.InsertAsync(permission);

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(Permission), permission.Id, "*", null, $"Created {permission.SystemName}", userId);
            return permission;
        }

        public async Task<Permission> UpdateAsync(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            var oldPerm = await _permissionRepo.GetByIdAsync(permission.Id);
            var oldName = oldPerm?.Name;

            await _permissionRepo.UpdateAsync(permission);

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(Permission), permission.Id, "Name", oldName, permission.Name, userId);

            return permission;
        }

        public async Task DeleteAsync(int permissionId)
        {
            var entity = await _permissionRepo.GetByIdAsync(permissionId);
            if (entity == null)
                throw new InvalidOperationException("Permission not found");

            await _permissionRepo.DeleteAsync(entity);

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(Permission), entity.Id, "*", entity.Name, null, userId);
        }

        #endregion

        #region Role ? Permission

        public async Task<IList<string>> GetPermissionsForRoleAsync(int roleId)
        {
            var perms = from rp in _rolePermissionRepo.Table
                        join p in _permissionRepo.Table on rp.PermissionId equals p.Id
                        where rp.RoleId == roleId && p.IsActive
                        select p.SystemName;

            return perms.ToList();
        }

        public async Task GrantPermissionToRoleAsync(int roleId, string permissionSystemName)
        {
            var perm = await GetBySystemNameAsync(permissionSystemName);
            if (perm == null)
                throw new InvalidOperationException("Permission not found");

            var exists = _rolePermissionRepo.Table.Any(rp => rp.RoleId == roleId && rp.PermissionId == perm.Id);
            if (exists)
                await _localization.GetResourceAsync("Admin.Security.Permissions.PermissionAlreadyGranted");

            await _rolePermissionRepo.InsertAsync(new RolePermission
            {
                RoleId = roleId,
                PermissionId = perm.Id
            });

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(RolePermission), roleId, perm.SystemName, null, "Granted", userId);

            // 🔹 Notification
            await _notificationService.SendAsync(
                registrationId: null,
                eventType: NotificationEvent.PermissionGranted,
                triggeredByUserId: userId,
                recipientUserId: userId, // أو Admins
                channel: NotificationChannel.InApp,
                tokens: new Dictionary<string, string>
                {
            { "RoleId", roleId.ToString() },
            { "Permission", perm.SystemName }
                }
            );
        }

        public async Task RevokePermissionFromRoleAsync(int roleId, string permissionSystemName)
        {
            var perm = await GetBySystemNameAsync(permissionSystemName);
            if (perm == null)
                throw new InvalidOperationException("Permission not found");

            var entity = _rolePermissionRepo.Table
                .FirstOrDefault(rp => rp.RoleId == roleId && rp.PermissionId == perm.Id);

            if (entity == null)
                throw new InvalidOperationException("Permission not granted");

            await _rolePermissionRepo.DeleteAsync(entity);

            // ?? Audit
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(RolePermission), roleId, perm.SystemName, "Granted", "Revoked", userId);
        }

        #endregion

        #region User overrides

        public async Task<IList<string>> GetUserOverridesAsync(int userId, bool onlyGranted = false)
        {
            var query = from o in _userOverrideRepo.Table
                        join p in _permissionRepo.Table on o.PermissionId equals p.Id
                        where o.UserId == userId
                        select new { o.IsGranted, p.SystemName };

            if (onlyGranted)
                return query.Where(x => x.IsGranted).Select(x => x.SystemName).ToList();

            return query.Select(x => $"{x.SystemName}:{(x.IsGranted ? "Allow" : "Deny")}").ToList();
        }

        public async Task SetUserOverrideAsync(int userId, string permissionSystemName, bool isGranted)
        {
            var perm = await GetBySystemNameAsync(permissionSystemName);
            if (perm == null)
                throw new InvalidOperationException("Permission not found");

            var entity = _userOverrideRepo.Table
                .FirstOrDefault(o => o.UserId == userId && o.PermissionId == perm.Id);

            if (entity != null)
            {
                entity.IsGranted = isGranted;
                await _userOverrideRepo.UpdateAsync(entity);
            }
            else
            {
                await _userOverrideRepo.InsertAsync(new UserPermissionOverride
                {
                    UserId = userId,
                    PermissionId = perm.Id,
                    IsGranted = isGranted
                });
            }

            // ?? Audit
            var actorId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(UserPermissionOverride), userId, perm.SystemName, null, isGranted ? "Granted" : "Denied", actorId);
        }

        public async Task RemoveUserOverrideAsync(int userId, string permissionSystemName)
        {
            var perm = await GetBySystemNameAsync(permissionSystemName);
            if (perm == null)
                throw new InvalidOperationException("Permission not found");

            var entity = _userOverrideRepo.Table
                .FirstOrDefault(o => o.UserId == userId && o.PermissionId == perm.Id);

            if (entity == null)
                throw new InvalidOperationException("Override not found");

            await _userOverrideRepo.DeleteAsync(entity);

            // ?? Audit
            var actorId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;
            await _auditTrailService.LogChangeAsync(
                nameof(UserPermissionOverride), userId, perm.SystemName, "Overridden", "Removed", actorId);
        }

        #endregion

        #region Authorization

        public async Task<bool> AuthorizeAsync(int userId, string permissionSystemName)
        {
            var perm = await GetBySystemNameAsync(permissionSystemName);
            if (perm == null || !perm.IsActive)
                return false;

            // 1) User override
            var overrideEntity = _userOverrideRepo.Table
                .FirstOrDefault(o => o.UserId == userId && o.PermissionId == perm.Id);
            if (overrideEntity != null)
                return overrideEntity.IsGranted;

            // 2) Roles
            var roleIds = _userRoleRepo.Table
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToList();

            if (!roleIds.Any())
                return false;

            var granted = _rolePermissionRepo.Table
                .Any(rp => roleIds.Contains(rp.RoleId) && rp.PermissionId == perm.Id);

            return granted;
        }

        #endregion
    }
}
