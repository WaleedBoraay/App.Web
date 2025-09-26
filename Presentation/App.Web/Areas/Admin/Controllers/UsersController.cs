using App.Core.Domain.Organization;
using App.Core.Domain.Users;
using App.Core.Security;
using App.Services;
using App.Services.Notifications;
using App.Services.Organization;
using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Organization;
using App.Web.Areas.Admin.Models.Users;
using App.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IRoleService _roleService;
        private readonly IOrganizationsServices _orgService;

        public UsersController(
            IUserService userService,
            IWorkContext workContext,
            IPermissionService permissionService,
            INotificationService notificationService,
            IRoleService roleService,
            IOrganizationsServices orgService)
        {
            _userService = userService;
            _workContext = workContext;
            _permissionService = permissionService;
            _notificationService = notificationService;
            _roleService = roleService;
            _orgService = orgService;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _orgService.GetAllDepartmentsAsync();
            var units = await _orgService.GetAllUnitsAsync();
            var subUnits = await _orgService.GetAllSubUnitsAsync();
            var users = await _userService.GetAllAsync(false);

            var filtered = users
                .Where(u => u.InstitutionId == null && u.RegistrationId == null)
                .ToList();

            var models = filtered.Select(u => new UserModel
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

                DepartmentId = u.DepartmentId,
                UnitId = u.UnitId,
                SubUnitId = u.SubUnitId,

                DepartmentName = u.DepartmentId.HasValue
                    ? departments.FirstOrDefault(d => d.Id == u.DepartmentId)?.Name
                    : null,
                UnitName = u.UnitId.HasValue
                    ? units.FirstOrDefault(x => x.Id == u.UnitId)?.Name
                    : null,
                SubUnitName = u.SubUnitId.HasValue
                    ? subUnits.FirstOrDefault(x => x.Id == u.SubUnitId)?.Name
                    : null
            }).ToList();

            return View(models);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            var permission = await _permissionService.AuthorizeAsync(currentUser.Id, AppPermissions.User_Create);
            if (!permission)
            {
                await _notificationService.NotifyErrorAsync("You.dont.have.permission.to.create.a.new.user");
                return RedirectToAction(nameof(Index));
            }

            var departments = await _orgService.GetAllDepartmentsAsync();
            var units = await _orgService.GetAllUnitsAsync();
            var subUnits = await _orgService.GetAllSubUnitsAsync();

            var model = new UserModel
            {
                Departments = departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }),
                Units = units.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Name }),
                SubUnits = subUnits.Select(su => new SelectListItem { Value = su.Id.ToString(), Text = su.Name })
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = new AppUser
            {
                Username = model.UserName,
                Email = model.Email,
                IsActive = model.Active,
                IsLockedOut = model.IsLockedOut,
                FailedLoginAttempts = 0,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,

                DepartmentId = model.DepartmentId,
                UnitId = model.UnitId,
                SubUnitId = model.SubUnitId
            };

            var currentUser = await _workContext.GetCurrentUserAsync();
            await _userService.InsertAsync(entity, model.Password, currentUser.Id);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var departments = await _orgService.GetAllDepartmentsAsync();
            var units = await _orgService.GetAllUnitsAsync();
            var subUnits = await _orgService.GetAllSubUnitsAsync();

            ViewBag.Departments = departments;
            ViewBag.Units = units;
            ViewBag.SubUnits = subUnits;

            var model = new UserModel
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email,
                Active = user.IsActive,
                IsLockedOut = user.IsLockedOut,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LastLoginDateUtc = user.LastLoginDateUtc,
                CreatedOnUtc = user.CreatedOnUtc,
                UpdatedOnUtc = user.UpdatedOnUtc,
                DepartmentId = user.DepartmentId,
                UnitId = user.UnitId,
                SubUnitId = user.SubUnitId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.Username = model.UserName;
            user.Email = model.Email;
            user.IsActive = model.Active;
            user.IsLockedOut = model.IsLockedOut;
            user.UpdatedOnUtc = DateTime.UtcNow;

            user.DepartmentId = model.DepartmentId;
            user.UnitId = model.UnitId;
            user.SubUnitId = model.SubUnitId;

            var currentUser = await _workContext.GetCurrentUserAsync();

            if (!string.IsNullOrEmpty(model.Password))
            {
                await _userService.ResetPasswordAsync(user.Id, model.Password, currentUser.Id);
            }
            else
            {
                await _userService.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new UserModel
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email,
                Active = user.IsActive
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userService.DeleteAsync(user.Id);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _roleService.GetRolesByUserIdAsync(id);
            var departments = await _orgService.GetAllDepartmentsAsync();
            var units = await _orgService.GetAllUnitsAsync();
            var subUnits = await _orgService.GetAllSubUnitsAsync();
            var allRoles = await _roleService.GetAllAsync();

            var model = new UserModel
            {
                Id = user.Id,
                UserName = user.Username,
                Email = user.Email,
                Active = user.IsActive,
                IsLockedOut = user.IsLockedOut,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LastLoginDateUtc = user.LastLoginDateUtc,
                CreatedOnUtc = user.CreatedOnUtc,
                UpdatedOnUtc = user.UpdatedOnUtc,
                Roles = roles.Select(r => new RoleModel { Id = r.Id, Name = r.Name }).ToList(),
                DepartmentId = user.DepartmentId,
                UnitId = user.UnitId,
                SubUnitId = user.SubUnitId
            };

            var vm = new UserDetailsModel
            {
                User = model,
                AllRoles = allRoles.Select(r => new RoleModel { Id = r.Id, Name = r.Name }).ToList(),
                Departments = departments,
                Units = units,
                SubUnits = subUnits
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            await _userService.ActivateAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _userService.DeactivateAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Lock(int id)
        {
            await _userService.LockUserAsync(id, 60);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(int id)
        {
            await _userService.UnlockUserAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(int userId, int roleId, int? departmentId, int? unitId, int? subUnitId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            user.DepartmentId = departmentId;
            user.UnitId = unitId;
            user.SubUnitId = subUnitId;
            await _userService.UpdateAsync(user);

            if (roleId != 0)
            {
                await _roleService.AddUserToRoleAsync(userId, roleId);
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(int userId, int roleId)
        {
            await _roleService.RemoveUserFromRoleAsync(userId, roleId);
            return RedirectToAction(nameof(Details), new { id = userId });
        }

    }
}
