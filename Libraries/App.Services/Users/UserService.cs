using App.Core.Domain.Audit;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using App.Services.Audit;
using App.Services.Common;
using App.Services.Localization;
using App.Services.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<AppUser> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<UserPreference> _preferenceRepository;
        private readonly IRepository<UserLog> _userLogRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuditTrailService _auditTrailService;
        private readonly IWorkContext _workContext;
        public UserService(
            IRepository<AppUser> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<UserPreference> preferenceRepository,
            IEncryptionService encryptionService,
            IAuditTrailService auditTrailService,
            ILocalizationService localizationService,
            IRepository<UserLog> userLogRepository,
            IWorkContext workContext)
            : base()
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _preferenceRepository = preferenceRepository;
            _encryptionService = encryptionService;
            _auditTrailService = auditTrailService;
            _userLogRepository = userLogRepository;
            _workContext = workContext;
        }

        #region CRUD
        public async Task<AppUser> GetByIdAsync(int id)
            => await _userRepository.GetByIdAsync(id);

        public async Task<IList<AppUser>> GetAllAsync(bool onlyActive = true)
        {
            return await _userRepository.GetAllAsync(q =>
            {
                if (onlyActive)
                    q = q.Where(u => u.IsActive);
                return q;
            });
        }

        public async Task<AppUser> InsertAsync(AppUser user, string password, int createdByUserId)
        {
            if (user == null)
                throw new System.ArgumentNullException(nameof(user));

            var saltKey = _encryptionService.CreateSaltKey();
            user.PasswordSalt = saltKey;
            user.PasswordHash = _encryptionService.CreatePasswordHash(password, saltKey, PasswordFormat.Hashed);

            await _userRepository.InsertAsync(user);

            await _auditTrailService.LogCreateAsync(nameof(AppUser), user.Id, createdByUserId);

            return user;
        }

        public async Task<AppUser> InsertAsync(AppUser user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var saltKey = _encryptionService.CreateSaltKey();
            user.PasswordSalt = saltKey;
            user.PasswordHash = _encryptionService.CreatePasswordHash(password, saltKey, PasswordFormat.Hashed);

            await _userRepository.InsertAsync(user);

            await _auditTrailService.LogCreateAsync(nameof(AppUser), user.Id);

            return user;
        }

        public async Task<AppUser> UpdateAsync(AppUser user)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await _userRepository.UpdateAsync(user);
            await _auditTrailService.LogUpdateAsync(nameof(AppUser), user.Id, currentUser.Id, comment: "User updated");
            return user;
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");
            await _userRepository.DeleteAsync(user);
            await _auditTrailService.LogDeleteAsync(nameof(AppUser), id, 0)
                .ConfigureAwait(false);
        }
        #endregion

        #region Identity
        public async Task<AppUser> GetByEmailAsync(string email)
            => (await _userRepository.GetAllAsync(q =>
                    q.Where(u => u.Email.ToLower() == email.ToLower())))
               .FirstOrDefault();

        public async Task<AppUser> GetByUsernameAsync(string username)
            => (await _userRepository.GetAllAsync(q => q.Where(u => u.Username == username))).FirstOrDefault();
        #endregion

        #region Security
        public async Task ActivateAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            user.IsActive = true;
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeactivateAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.IsActive = false;
            user.DeactivatedOnUtc = System.DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetPasswordAsync(int userId, string newPassword, int performedByUserId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");


            var saltKey = _encryptionService.CreateSaltKey();
            user.PasswordSalt = saltKey;
            user.PasswordHash = _encryptionService.CreatePasswordHash(newPassword, saltKey, PasswordFormat.Hashed);

            await _userRepository.UpdateAsync(user);
            await _auditTrailService.LogUpdateAsync(nameof(AppUser), user.Id, performedByUserId, field: "Password");
        }

        public async Task LockUserAsync(int userId, int? minutes = null)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.LockoutEndUtc = minutes.HasValue ? System.DateTime.UtcNow.AddMinutes(minutes.Value) : System.DateTime.MaxValue;
            await _userRepository.UpdateAsync(user);
        }

        public async Task UnlockUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.LockoutEndUtc = null;
            await _userRepository.UpdateAsync(user);
        }

        public Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            var hash = _encryptionService.CreatePasswordHash(password, user.PasswordSalt, PasswordFormat.Hashed);
            return Task.FromResult(hash == user.PasswordHash);
        }

        public Task<bool> IsPasswordExpiredAsync(AppUser user)
        {
            return Task.FromResult(false);
        }
        public async Task<IList<string>> GetRolesAsync(int userId)
        {
            var roles = await _roleRepository.GetAllAsync(q =>
                from r in q
                join ur in _userRoleRepository.Table on r.Id equals ur.RoleId
                where ur.UserId == userId
                select r
            );
            return roles.Select(r => r.Name).ToList();
        }

        public async Task<IList<Role>> GetRoleEntitiesAsync(int userId)
        {
            var roles = await _roleRepository.GetAllAsync(q =>
                from r in q
                join ur in _userRoleRepository.Table on r.Id equals ur.RoleId
                where ur.UserId == userId
                select r);
            return roles;
        }

        public async Task<bool> IsInRoleAsync(int userId, string roleSystemName)
        {
            var roles = await GetRolesAsync(userId);
            return roles.Contains(roleSystemName);
        }

        public async Task AddToRoleAsync(int userId, int roleId)
        {
            var ur = new UserRole { UserId = userId, RoleId = roleId };
            await _userRoleRepository.InsertAsync(ur);
        }

        public async Task RemoveFromRoleAsync(int userId, int roleId)
        {
            var ur = (await _userRoleRepository.GetAllAsync(q => q.Where(x => x.UserId == userId && x.RoleId == roleId))).FirstOrDefault();
            if (ur != null)
                await _userRoleRepository.DeleteAsync(ur);
        }
        #endregion

        #region Status Queries
        public async Task<IList<AppUser>> GetLockedUsersAsync()
            => await _userRepository.GetAllAsync(q => q.Where(u => u.LockoutEndUtc.HasValue));

        public async Task<IList<AppUser>> GetInactiveUsersAsync()
            => await _userRepository.GetAllAsync(q => q.Where(u => !u.IsActive));
        #endregion

        #region Preferences
        public async Task<UserPreference> GetPreferencesAsync(int userId)
            => (await _preferenceRepository.GetAllAsync(q => q.Where(p => p.UserId == userId))).FirstOrDefault();

        public async Task UpdatePreferencesAsync(UserPreference preference)
        {
            var entity = (await _preferenceRepository.GetAllAsync(q => q.Where(p => p.UserId == preference.UserId))).FirstOrDefault();
            if (entity == null)
                await _preferenceRepository.InsertAsync(preference);
            else
            {
                entity.LanguageId = preference.LanguageId;
                entity.EnableMfa = preference.EnableMfa;
                entity.NotifyByEmail = preference.NotifyByEmail;
                entity.NotifyBySms = preference.NotifyBySms;
                entity.NotifyInApp = preference.NotifyInApp;
                entity.UpdatedOnUtc = System.DateTime.UtcNow;
                await _preferenceRepository.UpdateAsync(entity);
            }
        }
        #endregion

        #region Audit
        public async Task<IList<AuditTrail>> GetUserAuditTrailAsync(int userId, int pageIndex = 0, int pageSize = 50)
        {
            var results = await _auditTrailService.SearchAsync(
                entityName: nameof(AppUser),
                entityId: userId,
                pageIndex: pageIndex,
                pageSize: pageSize
            );

            return results.Items;
        }
        #endregion

        public async Task RecordLoginFailureAsync(AppUser user)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLockedOut = true;
                user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
            }

            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetFailedAttemptsAsync(AppUser user)
        {
            user.FailedLoginAttempts = 0;
            user.IsLockedOut = false;
            user.LockoutEndUtc = null;

            await _userRepository.UpdateAsync(user);
        }

        public bool IsUserLockedOut(AppUser user)
        {
            if (!user.IsLockedOut) return false;
            if (user.LockoutEndUtc.HasValue && user.LockoutEndUtc.Value < DateTime.UtcNow)
            {
                user.IsLockedOut = false;
                user.LockoutEndUtc = null;
                return false;
            }
            return true;
        }

        public async Task AddUserLogAsync(int userId, string action, string clientIp)
        {
            var log = new UserLog
            {
                UserId = userId,
                Action = action,
                ClientIp = clientIp,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _userLogRepository.InsertAsync(log);
        }
        public async Task<bool> ValidatePasswordAsync(AppUser user, string password)
        {
            return _encryptionService.VerifyPassword(
                password,
                user.PasswordHash,
                user.PasswordSalt,
                user.PasswordFormat
            );
        }

        public async Task SetPasswordAsync(AppUser user, string newPassword)
        {
            // Fix: Generate a salt, then hash the password using the salt and PasswordFormat.Hashed
            var salt = _encryptionService.CreateSaltKey();
            var hash = _encryptionService.CreatePasswordHash(newPassword, salt, PasswordFormat.Hashed);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.PasswordFormat = PasswordFormat.Hashed;

            await _userRepository.UpdateAsync(user);
        }

        public async Task<AppUser> GetRegulatorUserAsync()
        {
            var users = await GetAllAsync(true);

            foreach (var user in users)
            {
                var roles = await GetRolesAsync(user.Id);
                if (roles.Contains("Regulator"))
                    return user;
            }

            return null;
        }
    }
}
