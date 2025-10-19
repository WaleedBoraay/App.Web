using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Models.Security;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.Controllers
{
    [Route("api/user-permission-overrides")]
    [ApiController]
    public class UserPermissionOverridesApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPermissionService _permissionService;

        public UserPermissionOverridesApiController(
            IUserService userService,
            IPermissionService permissionService)
        {
            _userService = userService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            var allPerms = await _permissionService.GetAllAsync();

            var model = new List<UserPermissionOverrideModel>();

            foreach (var user in users)
            {
                var overrides = await _permissionService.GetUserOverridesAsync(user.Id);

                var granted = overrides
                    .Where(o => o.EndsWith("Allow"))
                    .Select(o => o.Split(':')[0])
                    .ToList();

                var denied = overrides
                    .Where(o => o.EndsWith("Deny"))
                    .Select(o => o.Split(':')[0])
                    .ToList();

                model.Add(new UserPermissionOverrideModel
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Granted = granted,
                    Denied = denied,
                    AllPermissions = allPerms.Select(p => new PermissionModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        SystemName = p.SystemName,
                        Category = p.Category,
                        Description = p.Description,
                        IsActive = p.IsActive
                    }).ToList()
                });
            }

            return Ok(model);
        }

        [HttpPost("{userId}/grant")]
        public async Task<IActionResult> Grant(int userId, [FromBody] string permissionSystemName)
        {
            await _permissionService.SetUserOverrideAsync(userId, permissionSystemName, true);
            return Ok(new { success = true });
        }

        [HttpPost("{userId}/deny")]
        public async Task<IActionResult> Deny(int userId, [FromBody] string permissionSystemName)
        {
            await _permissionService.SetUserOverrideAsync(userId, permissionSystemName, false);
            return Ok(new { success = true });
        }

        [HttpDelete("{userId}/remove")]
        public async Task<IActionResult> Remove(int userId, [FromBody] string permissionSystemName)
        {
            await _permissionService.RemoveUserOverrideAsync(userId, permissionSystemName);
            return Ok(new { success = true });
        }
    }
}
