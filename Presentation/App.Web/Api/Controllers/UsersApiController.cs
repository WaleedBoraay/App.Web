using App.Core.Domain.Users;
using App.Core.Security;
using App.Services;
using App.Services.Notifications;
using App.Services.Organization;
using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Users;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IRoleService _roleService;
        private readonly IOrganizationsServices _orgService;
        private readonly ISectorServices _sectorService;
        private readonly IDepartmentServices _departmentService;
        private readonly IUnitServices _unitService;
        private readonly IEmailService _emailService;

		public UsersApiController(
            IUserService userService,
            IWorkContext workContext,
            IPermissionService permissionService,
            INotificationService notificationService,
            IRoleService roleService,
            IOrganizationsServices orgService,
            ISectorServices sectorServices,
            IDepartmentServices departmentServices,
            IUnitServices unitServices,
            IEmailService emailService)
        {
            _userService = userService;
            _workContext = workContext;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _roleService = roleService;
            _orgService = orgService;
            _sectorService = sectorServices;
            _departmentService = departmentServices;
            _unitService = unitServices;
            _emailService = emailService;
		}



        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sectors = await _sectorService.GetAllSectorsAsync();
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var units = await _unitService.GetAllUnitsAsync();

			var users = await _userService.GetAllAsync(false);

			var filtered = users
                .Select(async u => new UserModel
                {

                    Id = u.Id,
                    UserName = u.Username,
                    Email = u.Email,
                    Active = u.IsActive,
                    IsLockedOut = u.IsLockedOut,
                    FailedLoginAttempts = u.FailedLoginAttempts,
                    LastLoginDateUtc = u.LastLoginDateUtc,
                    CreatedOnUtc = u.CreatedOnUtc,
                    UpdatedOnUtc = u.UpdatedOnUtc,
                    RoleName = string.Join(", ", _roleService.GetRolesByUserIdAsync(u.Id).Result.Select(r => r.Name)),
					SectorId = u.SectorId,
                    SectoreName = sectors.Where(s => s.Id == u.SectorId).Select(n=>n.Name).FirstOrDefault(),
					DepartmentId = u.DepartmentId,
                    DepartmentName = departments.Where(d => d.Id == u.DepartmentId).Select(n => n.Name).FirstOrDefault(),
					UnitId = u.UnitId,
                    UnitName = units.Where(x => x.Id == u.UnitId).Select(n => n.Name).FirstOrDefault()
					//assign sector, department, unit names


				}).ToList();

			return Ok(filtered);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new AppUser
            {
                Username = model.UserName,
                Email = model.Email,
                IsActive = model.Active,
                IsLockedOut = model.IsLockedOut,
                FailedLoginAttempts = 0,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                SectorId = model.SectorId,
                DepartmentId = model.DepartmentId,
                UnitId = model.UnitId
            };

            //var currentUser = await _workContext.GetCurrentUserAsync();
            var user = await _userService.InsertAsync(entity, model.Password, 0);
            //Send email to userwith password setup instructions could be added here
            if (user != null)
            {

				var emailBody = $"Dear {user.Username},<br/><br/>" +
				$"Your account has been created.<br/>" +
				$"Username: {user.Username}<br/>" +
				$"Password: {model.Password}<br/><br/>" +
				$"Please change your password after logging in.<br/><br/>" +
				$"https://suptech.online/<br/><br/>" +
				$"Best regards,<br/>" +
				$"The Team";
				await _emailService.SendEmailAsync(
					to: user.Email,
					subject: "Your Account Credentials",
					body: emailBody);

            }


				return Ok(new { success = true, userId = entity.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] UserModel model)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.Username = model.UserName;
            user.Email = model.Email;
            user.IsActive = model.Active;
            user.IsLockedOut = model.IsLockedOut;
            user.UpdatedOnUtc = DateTime.UtcNow;
            user.SectorId = model.SectorId;
            user.DepartmentId = model.DepartmentId;
            user.UnitId = model.UnitId;

            var currentUser = await _workContext.GetCurrentUserAsync();
            if (!string.IsNullOrEmpty(model.Password))
                await _userService.ResetPasswordAsync(user.Id, model.Password, currentUser.Id);
            else
                await _userService.UpdateAsync(user);

            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            await _userService.DeleteAsync(user.Id);
            return Ok(new { success = true });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _roleService.GetRolesByUserIdAsync(id);
            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.IsActive,
                Roles = roles.Select(r => new { r.Id, r.Name })
            });
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            await _userService.ActivateAsync(id);
            return Ok(new { success = true });
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _userService.DeactivateAsync(id);
            return Ok(new { success = true });
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> Lock(int id)
        {
            await _userService.LockUserAsync(id, 60);
            return Ok(new { success = true });
        }

        [HttpPost("{id}/unlock")]
        public async Task<IActionResult> Unlock(int id)
        {
            await _userService.UnlockUserAsync(id);
            return Ok(new { success = true });
        }

        [HttpPost("{id}/assign-role")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] int roleId)
        {
            await _roleService.AddUserToRoleAsync(id, roleId);
            return Ok(new { success = true });
        }

        [HttpPost("{id}/remove-role")]
        public async Task<IActionResult> RemoveRole(int id, [FromBody] int roleId)
        {
            await _roleService.RemoveUserFromRoleAsync(id, roleId);
            return Ok(new { success = true });
        }
    }
}
