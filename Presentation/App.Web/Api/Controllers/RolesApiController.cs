using App.Services.Security;
using App.Web.Areas.Admin.Models.Security;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.Controllers
{
    [Route("api/roles")]
    [ApiController]
    public class RolesApiController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesApiController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/admin/roles
        [HttpGet]
        public async Task<IActionResult> GetAll()
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

            return Ok(model);
        }

        // GET: api/admin/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();

            var model = new RoleModel
            {
                Id = role.Id,
                Name = role.Name,
                SystemName = role.SystemName,
                Description = role.Description,
                IsActive = role.IsActive,
                IsSystemRole = role.IsSystemRole
            };

            return Ok(model);
        }

        // POST: api/admin/roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var role = new App.Core.Domain.Security.Role
            {
                Name = model.Name,
                SystemName = model.SystemName,
                Description = model.Description,
                IsActive = model.IsActive,
                IsSystemRole = model.IsSystemRole,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _roleService.InsertAsync(role);

            return Ok(new { success = true, roleId = role.Id });
        }

        // PUT: api/admin/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();

            role.Name = model.Name;
            role.SystemName = model.SystemName;
            role.Description = model.Description;
            role.IsActive = model.IsActive;
            role.IsSystemRole = model.IsSystemRole;
            role.UpdatedOnUtc = DateTime.UtcNow;

            await _roleService.UpdateAsync(role);

            return Ok(new { success = true });
        }

        // DELETE: api/admin/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            if (role == null) return NotFound();

            await _roleService.DeleteAsync(role.Id);

            return Ok(new { success = true });
        }
    }
}
