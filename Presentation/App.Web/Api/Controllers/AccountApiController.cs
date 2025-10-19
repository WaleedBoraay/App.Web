using App.Core;
using App.Core.Domain.Audit;
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

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRoleService _roleService;
        private readonly IAuditTrailService _auditTrailService;
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;

        public AccountApiController(
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

        // POST: api/Account/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _userService.GetByUsernameAsync(model.Username);
            if (existing != null)
                return BadRequest(new { message = "Username already exists." });

            var user = new AppUser
            {
                Username = model.Username,
                Email = model.Email,
                IsActive = true,
                CreatedOnUtc = DateTime.UtcNow
            };

            var createdUser = await _userService.InsertAsync(user, model.Password);
            if (createdUser == null)
                return BadRequest(new { message = "Could not create user." });

            var auditCreate = await _auditTrailService.InsertAuditTrailAsync(new AuditTrail
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
                AuditTrailId = auditCreate.Id
            });

            var rolesList = await _roleService.GetAllAsync();
            var makerRole = rolesList.FirstOrDefault(r => r.SystemName == "Maker");
            if (makerRole != null)
                await _roleService.AddUserToRoleAsync(createdUser.Id, makerRole.Id);

            var roles = await _userService.GetRolesAsync(createdUser.Id);
            await _authenticationService.SignInAsync(createdUser, false, roles);

            return Ok(new { message = "Registration successful", user = createdUser });
        }

        // POST: api/Account/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userService.GetByUsernameAsync(model.Username)
                       ?? await _userService.GetByEmailAsync(model.Username);

            if (user == null || !user.IsActive)
                return Unauthorized(new { message = "Invalid username/email or inactive account." });

            if (_userService.IsUserLockedOut(user))
                return Unauthorized(new { message = "Your account is locked. Try again later." });

            var isValid = await _userService.ValidatePasswordAsync(user, model.Password);
            if (!isValid)
            {
                await _userService.RecordLoginFailureAsync(user);
                return Unauthorized(new { message = "Invalid username/email or password." });
            }

            await _userService.ResetFailedAttemptsAsync(user);

            var roles = await _userService.GetRolesAsync(user.Id);
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

            return Ok(new { message = "Login successful", user, roles });
        }

        // POST: api/Account/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await _authenticationService.SignOutAsync();

            await _auditTrailService.InsertAuditTrailAsync(new AuditTrail
            {
                EntityName = "AppUser",
                EntityId = currentUser?.Id ?? 0,
                Action = AuditActionType.Logout,
                ChangedOnUtc = DateTime.UtcNow,
                Comment = $"User logged out: {currentUser?.Username}",
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            return Ok(new { message = "Logged out successfully" });
        }

        // GET: api/Account/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized(new { message = "User not found" });

            var model = new ProfileModel
            {
                Username = currentUser.Username,
                Email = currentUser.Email,
                TwoFactorEnabled = currentUser.TwoFactorEnabled
            };
            return Ok(model);
        }

        // PUT: api/Account/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _workContext.GetCurrentUserAsync();
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            user.Email = model.Email;
            user.TwoFactorEnabled = model.TwoFactorEnabled;

            if (!string.IsNullOrEmpty(model.NewPassword))
                await _userService.SetPasswordAsync(user, model.NewPassword);

            await _userService.UpdateAsync(user);

            await _userService.AddUserLogAsync(user.Id, "ProfileUpdated", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpGet("GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            var roles = await _roleService.GetRolesByUserIdAsync(user.Id);
            if (user == null) return Unauthorized();

            return Ok(new
            {
                user.Id,
                user.Email,
                user.Username,
                RoleName = roles
            });
        }
    }
}
