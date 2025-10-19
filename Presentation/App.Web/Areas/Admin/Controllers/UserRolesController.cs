using App.Services.Security;
using App.Services.Users;
using Microsoft.AspNetCore.Mvc;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserRolesController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UserRolesController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllAsync(false);
            var roles = await _roleService.GetAllAsync();

            var filteredUsers = users
                .Where(u => u.InstitutionId == null && u.RegistrationId == null)
                .ToList();

            var model = new UserRolesIndexModel
            {
                Users = new List<UserRoleModel>(),
                AllRoles = roles.Select(r => new RoleModel
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };

            foreach (var user in filteredUsers)
            {
                var userRoles = await _roleService.GetRolesByUserIdAsync(user.Id);
                model.Users.Add(new UserRoleModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Roles = userRoles.Select(r => r.Name).ToList()
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoles(UserRoleUpdateListModel model)
        {
            foreach (var userRole in model.UserRoles)
            {
                var user = await _userService.GetByIdAsync(userRole.UserId);
                if (user == null) continue;

                await _roleService.ClearRolesAsync(user.Id);

                foreach (var roleId in userRole.SelectedRoleIds)
                {
                    await _roleService.AddUserToRoleAsync(user.Id, roleId);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
