using App.Core;
using App.Core.Domain.Audit;
using App.Core.Domain.Notifications;
using App.Core.Domain.Users;
using App.Services;
using App.Services.Audit;
using App.Services.Authentication;
using App.Services.Notifications;
using App.Services.Security;
using App.Services.Users;
using App.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRoleService _roleService;
        private readonly IAuditTrailService _auditTrailService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;

        public AccountController(
            IUserService userService,
            IAuthenticationService authenticationService,
            IRoleService roleService,
            IAuditTrailService auditTrailService,
            INotificationService notificationService,
            IWorkContext workContext)
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _roleService = roleService;
            _auditTrailService = auditTrailService;
            _notificationService = notificationService;
            _workContext = workContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if username already exists
            var existing = await _userService.GetByUsernameAsync(model.Username);
            if (existing != null)
            {
                ModelState.AddModelError("", "Username already exists.");
                return View(model);
            }

            var user = new AppUser
            {
                Username = model.Username,
                Email = model.Email,
                IsActive = true,
                CreatedOnUtc = DateTime.UtcNow
            };

            // Create new user (password hashing inside service)
            var createdUser = await _userService.InsertAsync(user, model.Password);
            // Audit trail for registration

            var auditCreate = _auditTrailService.InsertAuditTrailAsync(new AuditTrail
            {
                EntityName = "AppUser",
                EntityId = createdUser.Id,
                Action = AuditActionType.UserRegistration,
                ChangedOnUtc = DateTime.UtcNow,
                Comment = $"User registered: {model.Username}",
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            });


            await _auditTrailService.InsertUserAuditTrailAsync(new UserAuditTrail
            {
                UserId = createdUser.Id,
                AuditTrailId = (await auditCreate).Id
            });


            if (createdUser == null)
            {
                ModelState.AddModelError("", "Could not create user.");
                return View(model);
            }

            // Assign default role (Maker)
            var rolesList = await _roleService.GetAllAsync();
            var makerRole = rolesList.FirstOrDefault(r => r.SystemName == "Maker");
            if (makerRole != null)
                await _roleService.AddUserToRoleAsync(createdUser.Id, makerRole.Id);

            // Auto-login
            var roles = await _userService.GetRolesAsync(createdUser.Id); // returns IList<string>
            await _authenticationService.SignInAsync(createdUser, false, roles);

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }


        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // نحاول نجيب اليوزر باليوزرنيم أو الإيميل
            var user = await _userService.GetByUsernameAsync(model.Username)
                       ?? await _userService.GetByEmailAsync(model.Username);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Invalid username/email or inactive account.");
                return View(model);
            }

            // Lockout check
            if (_userService.IsUserLockedOut(user))
            {
                ModelState.AddModelError("", "Your account is locked. Try again later.");
                return View(model);
            }

            // Validate password
            var isValid = await _userService.ValidatePasswordAsync(user, model.Password);
            if (!isValid)
            {
                await _userService.RecordLoginFailureAsync(user);
                await _userService.AddUserLogAsync(user.Id, "LoginFailure", HttpContext.Connection.RemoteIpAddress?.ToString());
                ModelState.AddModelError("", "Invalid username/email or password.");
                return View(model);
            }

            // Reset failed attempts
            await _userService.ResetFailedAttemptsAsync(user);
            await _userService.AddUserLogAsync(user.Id, "LoginSuccess", HttpContext.Connection.RemoteIpAddress?.ToString());

            // Load roles for claims
            var roles = await _userService.GetRolesAsync(user.Id);

            // Sign in with roles
            await _authenticationService.SignInAsync(user, model.RememberMe, roles);

            await _auditTrailService.InsertAuditTrailAsync(new AuditTrail
            {
                EntityName = "AppUser",
                EntityId = user.Id,
                Action = AuditActionType.UserLogin,
                ChangedOnUtc = DateTime.UtcNow,
                Comment = $"User logged in: {user.Username}",
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        // GET: /Account/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await _authenticationService.SignOutAsync();

            await _auditTrailService.InsertAuditTrailAsync(new AuditTrail
            {
                EntityName = "AppUser",
                EntityId = currentUser.Id,
                Action = AuditActionType.Logout,
                ChangedOnUtc = DateTime.UtcNow,
                Comment = $"User logged out: {User.Identity.Name}",
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userService.GetByUsernameAsync(User.Identity.Name);
            if (user == null)
                return NotFound();

            var model = new ProfileModel
            {
                Username = user.Username,
                Email = user.Email,
                TwoFactorEnabled = user.TwoFactorEnabled
            };
            return View(model);
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByUsernameAsync(User.Identity.Name);
            if (user == null)
                return NotFound();

            user.Email = model.Email;
            user.TwoFactorEnabled = model.TwoFactorEnabled;

            if (!string.IsNullOrEmpty(model.NewPassword))
                await _userService.SetPasswordAsync(user, model.NewPassword);

            await _userService.UpdateAsync(user);
            await _userService.AddUserLogAsync(user.Id, "ProfileUpdated", HttpContext.Connection.RemoteIpAddress?.ToString());

            ViewBag.StatusMessage = "Profile updated successfully.";
            return View(model);
        }
    }
}
