using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Models.Security;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.Controllers
{
    [Route("api/user-roles")]
    [ApiController]
    public class UserRolesApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UserRolesApiController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync(false);
            var roles = await _roleService.GetAllAsync();

            var filteredUsers = users.Where(u => u.InstitutionId == null && u.RegistrationId == null);

            var result = filteredUsers.Select(async u => new UserRoleModel
            {
                UserId = u.Id,
                Username = u.Username,
                Roles = (await _roleService.GetRolesByUserIdAsync(u.Id)).Select(r => r.Name).ToList()
            }).Select(t => t.Result).ToList();

            return Ok(new
            {
                Users = result,
                AllRoles = roles.Select(r => new { r.Id, r.Name }).ToList()
            });
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateRoles([FromBody] UserRoleUpdateListModel model)
        {
            foreach (var userRole in model.UserRoles)
            {
                var user = await _userService.GetByIdAsync(userRole.UserId);
                if (user == null) continue;

                await _roleService.ClearRolesAsync(user.Id);
                foreach (var roleId in userRole.SelectedRoleIds)
                    await _roleService.AddUserToRoleAsync(user.Id, roleId);
            }

            return Ok(new { success = true });
        }
    }
}
