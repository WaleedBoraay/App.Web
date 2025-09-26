using App.Core.Security;
using App.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = SystemRoles.Admin)]
    [Route("admin/user-management")]
    public class UserManagementController : Controller
    {
        private readonly IUserDirectory _userDirectory;

        public UserManagementController(IUserDirectory userDirectory)
        {
            _userDirectory = userDirectory;
        }

        [HttpPost("activate/{userId}")]
        public async Task<IActionResult> Activate(int userId)
        {
            var result = await _userDirectory.ActivateAsync(userId);
            if (!result)
                return BadRequest($"Failed to activate user with ID {userId}.");

            return Ok($"User with ID {userId} has been activated.");
        }

        [HttpPost("deactivate/{userId}")]
        public async Task<IActionResult> Deactivate(int userId)
        {
            var result = await _userDirectory.DeactivateAsync(userId);
            if (!result)
                return BadRequest($"Failed to deactivate user with ID {userId}.");

            return Ok($"User with ID {userId} has been deactivated.");
        }

        [HttpPost("assign-roles/{userId}")]
        public async Task<IActionResult> AssignRoles(int userId, [FromBody] string[] roles)
        {
            if (roles == null || roles.Length == 0)
                return BadRequest("Roles cannot be empty.");

            var result = await _userDirectory.AssignRolesAsync(userId, roles);
            if (!result)
                return BadRequest($"Failed to assign roles to user with ID {userId}.");

            return Ok($"Roles have been assigned to user with ID {userId}.");
        }
    }
}