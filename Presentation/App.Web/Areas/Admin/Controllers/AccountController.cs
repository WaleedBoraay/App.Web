using App.Services;
using App.Services.Common;
using App.Services.Users;
using App.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;

        public AccountController(IUserService userService, IWorkContext workContext)
        {
            _userService = userService;
            _workContext = workContext;
        }

        // GET: /Admin/Account/Manage
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            var model = new ManageAccountModel
            {
                Id = currentUser.Id,
                UserName = currentUser.Username,
                Email = currentUser.Email,
                Active = currentUser.IsActive,
                LastLoginDateUtc = currentUser.LastLoginDateUtc
            };

            return View(model);
        }

        // POST: /Admin/Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageAccountModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            // Update email
            currentUser.Email = model.Email;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                await _userService.ResetPasswordAsync(currentUser.Id, model.NewPassword, currentUser.Id);
            }
            else
            {
                await _userService.UpdateAsync(currentUser);
            }

            ViewData["Success"] = "Account updated successfully";
            return View(model);
        }

        // POST: /Admin/Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account", new { area = "" });
        }
    }
}
