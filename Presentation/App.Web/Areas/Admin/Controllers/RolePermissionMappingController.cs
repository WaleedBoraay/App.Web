using App.Services.Security;
using Microsoft.AspNetCore.Mvc;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RolePermissionMappingController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public RolePermissionMappingController(IRoleService roleService, IPermissionService permissionService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Edit(int roleId)
        {
            var role = await _roleService.GetByIdAsync(roleId);
            var allPermissions = await _permissionService.GetAllAsync();
            var granted = await _permissionService.GetPermissionsForRoleAsync(roleId);

            var model = new RolePermissionMappingModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                GrantedPermissions = granted,
                AllPermissions = allPermissions.Select(p => new PermissionModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SystemName = p.SystemName,
                    Category = p.Category,
                    Description = p.Description,
                    IsActive = p.IsActive
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RolePermissionMappingModel model, string[] selectedPermissions)
        {
            // clear old
            foreach (var perm in model.AllPermissions)
                await _permissionService.RevokePermissionFromRoleAsync(model.RoleId, perm.SystemName);

            // add new
            foreach (var permSysName in selectedPermissions)
                await _permissionService.GrantPermissionToRoleAsync(model.RoleId, permSysName);

            return RedirectToAction("Index", "Roles");
        }
    }
}
