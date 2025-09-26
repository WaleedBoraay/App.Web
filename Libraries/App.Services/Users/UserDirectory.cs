using System;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;

namespace App.Services.Users
{
    public class UserDirectory : IUserDirectory
    {
        private readonly IUserService _userService;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<AppUser> _userRepository;

        public UserDirectory(
            IUserService userService,
            IRepository<Role> roleRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<AppUser> userRepository)
        {
            _userService = userService;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
        }

        public async Task<bool> ActivateAsync(int userId)
        {
            await _userService.ActivateAsync(userId);
            return true;
        }

        public async Task<bool> DeactivateAsync(int userId)
        {
            await _userService.DeactivateAsync(userId);
            return true;
        }

        public async Task<bool> AssignRolesAsync(int userId, string[] roles)
        {
            if (roles == null) return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // Fix for CS0121: Specify default parameters to resolve ambiguity
            var allRoles = await _roleRepository.GetAllAsync((IQueryable<Role> q) => q, true);
            var roleIds = allRoles
                .Where(r => roles.Any(x => string.Equals(x, r.SystemName, StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(x, r.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(r => r.Id)
                .Distinct()
                .ToList();

            // Remove current mappings
            var existing = await _userRoleRepository.GetAllAsync(q => q.Where(ur => ur.UserId == userId));
            if (existing.Any())
                await _userRoleRepository.DeleteAsync(existing);

            // Add new mappings
            foreach (var rid in roleIds)
                await _userRoleRepository.InsertAsync(new UserRole { UserId = userId, RoleId = rid });

            return true;
        }
    }
}
