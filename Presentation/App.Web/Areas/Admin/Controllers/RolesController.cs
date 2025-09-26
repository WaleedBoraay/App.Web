using App.Services.Security;
using Microsoft.AspNetCore.Mvc;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleService.GetAllAsync();
            var model = roles.Select(r => new RoleModel
            {
                Id = r.Id,
                Name = r.Name,
                SystemName = r.SystemName,
                Description = r.Description,
                IsActive = r.IsActive,
                IsSystemRole = r.IsSystemRole
            }).ToList();

            return View(model);
        }

        public IActionResult Create() => View(new RoleModel());

        [HttpPost]
        public async Task<IActionResult> Create(RoleModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await _roleService.InsertAsync(new Core.Domain.Security.Role
            {
                Name = model.Name,
                SystemName = model.SystemName,
                Description = model.Description,
                IsActive = model.IsActive,
                IsSystemRole = model.IsSystemRole,
                CreatedOnUtc = DateTime.UtcNow
            });

            return RedirectToAction(nameof(Index));
        }
    }
}
