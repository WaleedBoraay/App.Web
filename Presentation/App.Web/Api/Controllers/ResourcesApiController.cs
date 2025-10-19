using App.Services.Localization;
using App.Web.Areas.Admin.Models.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.Controllers
{
    [Route("api/resources")]
    [ApiController]
    [Authorize]
    public class ResourcesApiController : ControllerBase
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;

        public ResourcesApiController(
            ILocalizationService localizationService,
            ILanguageService languageService)
        {
            _localizationService = localizationService;
            _languageService = languageService;
        }

        // GET: api/admin/resources?languageId=1&q=abc
        [HttpGet]
        public async Task<IActionResult> GetAll(int languageId = 1, string? q = null)
        {
            var list = await _localizationService.GetAllResourcesAsync(languageId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                list = list.Where(x =>
                    x.ResourceName.Contains(q) ||
                    (x.ResourceValue ?? string.Empty).Contains(q)
                ).ToList();
            }

            return Ok(list);
        }

        // GET: api/admin/resources/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var resource = await _localizationService.GetByIdAsync(id);
            if (resource == null) return NotFound();

            return Ok(resource);
        }

        // POST: api/admin/resources
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResourceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _localizationService.GetByNameAsync(model.ResourceName.Trim(), model.LanguageId);
            if (existing != null)
                return Conflict(new { message = "Resource already exists" });

            await _localizationService.InsertResourceAsync(new App.Core.Domain.Localization.LocaleStringResource
            {
                LanguageId = model.LanguageId,
                ResourceName = model.ResourceName.Trim(),
                ResourceValue = model.ResourceValue ?? string.Empty
            });

            return Ok(new { success = true, message = "Resource created successfully" });
        }

        // PUT: api/admin/resources/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResourceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var resource = await _localizationService.GetByIdAsync(id);
            if (resource == null) return NotFound();

            resource.ResourceName = model.ResourceName.Trim();
            resource.ResourceValue = model.ResourceValue ?? string.Empty;
            resource.LanguageId = model.LanguageId;

            await _localizationService.UpdateResourceAsync(resource);

            return Ok(new { success = true, message = "Resource updated successfully" });
        }

        // DELETE: api/admin/resources/{id}?languageId=1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, int languageId)
        {
            var resource = await _localizationService.GetByIdAsync(id);
            if (resource == null) return NotFound();

            await _localizationService.DeleteResourceAsync(resource);

            return Ok(new { success = true, message = "Resource deleted successfully" });
        }
    }
}
