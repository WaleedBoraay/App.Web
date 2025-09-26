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

            // فلترة: بس اللي InstitutionId = null && RegistrationId = null
            var filteredUsers = users
                .Where(u => u.InstitutionId == null && u.RegistrationId == null)
                .ToList();

            var model = new List<UserRoleModel>();

            foreach (var user in filteredUsers)
            {
                var userRoles = await _roleService.GetRolesByUserIdAsync(user.Id);
                model.Add(new UserRoleModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Roles = userRoles.Select(r => r.Name).ToList()
                });
            }

            ViewData["AllRoles"] = roles;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(AssignRoleToUserModel model)
        {
            await _roleService.AddUserToRoleAsync(model.UserId, model.RoleId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(AssignRoleToUserModel model)
        {
            await _roleService.RemoveUserFromRoleAsync(model.UserId, model.RoleId);
            return RedirectToAction(nameof(Index));
        }
    }
}
