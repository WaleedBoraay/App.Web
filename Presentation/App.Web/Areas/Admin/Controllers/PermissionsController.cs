using App.Services.Security;
using Microsoft.AspNetCore.Mvc;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PermissionsController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IPermissionSyncService _permissionSyncService;


        public PermissionsController(IPermissionService permissionService, IPermissionSyncService permissionSyncService)
        {
            _permissionService = permissionService;
            _permissionSyncService = permissionSyncService;

        }

        public async Task<IActionResult> Index()
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

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            await _permissionSyncService.SyncPermissionsAsync();
            return RedirectToAction(nameof(Index));
        }
    }

}
