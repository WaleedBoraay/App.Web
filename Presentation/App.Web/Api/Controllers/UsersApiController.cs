using App.Core.Domain.Users;
using App.Core.Security;
using App.Services;
using App.Services.Notifications;
using App.Services.Organization;
using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Users;
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

		public UsersApiController(
            IUserService userService,
            IWorkContext workContext,
            IPermissionService permissionService,
            INotificationService notificationService,
            IRoleService roleService,
            IOrganizationsServices orgService,
            ISectorServices sectorService,
            IDepartmentServices departmentService,
            IUnitServices unitService)
        {
            _userService = userService;
            _workContext = workContext;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _roleService = roleService;
            _orgService = orgService;
            _sectorService = sectorService;
            _departmentService = departmentService;
            _unitService = unitService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _orgService.GetAllDepartmentsAsync();
            var units = await _orgService.GetAllUnitsAsync();
            var subUnits = await _orgService.GetAllSubUnitsAsync();
            var users = await _userService.GetAllAsync(false);

            var filtered = users
                .Where(u => u.InstitutionId == null && u.RegistrationId == null)
                .Select(u => new UserModel
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
                    SectorId = u.SectorId,
                    DepartmentId = u.DepartmentId,
                    UnitId = u.UnitId,
                    SectoreName = u.SectorId.HasValue
                        ? departments.FirstOrDefault(d => d.Id == u.SectorId)?.Name
                        : null,
					DepartmentName = u.DepartmentId.HasValue
                        ? units.FirstOrDefault(x => x.Id == u.DepartmentId)?.Name
                        : null,
					UnitName = u.UnitId.HasValue
                        ? subUnits.FirstOrDefault(x => x.Id == u.UnitId)?.Name
                        : null
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
            var insUser = await _userService.InsertAsync(entity, model.Password, 0);
            var sector = await _sectorService.GetSectorByIdAsync(model.SectorId ?? 0);
            var department = await _departmentService.GetDepartmentByIdAsync(model.DepartmentId ?? 0);
            var unit = await _unitService.GetUnitByIdAsync(model.UnitId ?? 0);
            if(sector != null)
                entity.Sector = sector;
            if(department != null)
                entity.Department = department;
            if(unit != null)
                entity.Unit = unit;
            await _userService.UpdateAsync(entity);

			return Ok(new { success = true, userId = entity.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] UserModel model)
        {

			var sector = await _sectorService.GetSectorByIdAsync(model.SectorId ?? 0);
			var department = await _departmentService.GetDepartmentByIdAsync(model.DepartmentId ?? 0);
			var unit = await _unitService.GetUnitByIdAsync(model.UnitId ?? 0);

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

			if (sector != null)
				user.Sector = sector;
			if (department != null)
				user.Department = department;
			if (unit != null)
				user.Unit = unit;

			if (!string.IsNullOrEmpty(model.Password))
                await _userService.ResetPasswordAsync(user.Id, model.Password, user.Id);
            else
                await _userService.UpdateAsync(user);

            return Ok(new { success = true });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            var userrole = await _roleService.GetRolesByUserIdAsync(user.Id);
            foreach (var role in userrole)
            {
                await _roleService.RemoveUserFromRoleAsync(user.Id, role.Id);
			}
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
