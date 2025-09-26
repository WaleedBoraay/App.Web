using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Users;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UsersStatusController : Controller
    {
        private readonly IUserService _userService;
        public UsersStatusController(IUserService userService) { _userService = userService; }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var res = _userService.ActivateAsync(id);
            return Redirect(Request.Headers["Referer"].ToString() ?? "/Admin/Users");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id, string reason = null)
        {
            var res = _userService.DeactivateAsync(id);
            return Redirect(Request.Headers["Referer"].ToString() ?? "/Admin/Users");
        }
    }
}
