using App.Services.Security;
using Microsoft.AspNetCore.Mvc;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Api.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    public class PermissionsApiController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IPermissionSyncService _permissionSyncService;

        public PermissionsApiController(
            IPermissionService permissionService,
            IPermissionSyncService permissionSyncService)
        {
            _permissionService = permissionService;
            _permissionSyncService = permissionSyncService;
        }

        // GET: api/admin/permissions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var permissions = await _permissionService.GetAllAsync();
            var model = permissions.Select(p => new PermissionModel
            {
                Id = p.Id,
                Name = p.Name,
                SystemName = p.SystemName,
                Category = p.Category,
                Description = p.Description,
                IsActive = p.IsActive
            }).ToList();

            return Ok(model);
        }

        // POST: api/admin/permissions/sync
        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            await _permissionSyncService.SyncPermissionsAsync();
            return Ok(new { success = true, message = "Permissions synchronized successfully" });
        }
    }
}
