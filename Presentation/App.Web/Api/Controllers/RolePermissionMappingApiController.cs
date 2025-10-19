using App.Services.Security;
using Microsoft.AspNetCore.Mvc;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Api.Controllers
{
    [Route("api/role-permissions")]
    [ApiController]
    public class RolePermissionMappingApiController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;

        public RolePermissionMappingApiController(IRoleService roleService, IPermissionService permissionService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
        }

        // GET: api/admin/role-permissions/{roleId}
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRolePermissions(int roleId)
        {
            var role = await _roleService.GetByIdAsync(roleId);
            if (role == null) return NotFound();

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

            return Ok(model);
        }

        // PUT: api/admin/role-permissions/{roleId}
        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] string[] selectedPermissions)
        {
            var role = await _roleService.GetByIdAsync(roleId);
            if (role == null) return NotFound();

            var allPermissions = await _permissionService.GetAllAsync();

            // clear old
            foreach (var perm in allPermissions)
                await _permissionService.RevokePermissionFromRoleAsync(roleId, perm.SystemName);

            // add new
            foreach (var permSysName in selectedPermissions)
                await _permissionService.GrantPermissionToRoleAsync(roleId, permSysName);

            return Ok(new { success = true, message = "Role permissions updated successfully" });
        }
    }
}
