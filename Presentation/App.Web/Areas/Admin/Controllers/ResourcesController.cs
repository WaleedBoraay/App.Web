using App.Services.Localization;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ResourcesController : Controller
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        public ResourcesController(ILocalizationService localizationService, ILanguageService languageService)
        {
            _localizationService = localizationService;
            _languageService = languageService;

        }

        [HttpGet]
        public async Task<IActionResult> Index(int languageId = 1, string q = null)
        {
            var lang = await _languageService.GetAllAsync();

            var list = await _localizationService.GetAllResourcesAsync(languageId);
            if (!string.IsNullOrWhiteSpace(q))
                list = list.Where(x => x.ResourceName.Contains(q) || (x.ResourceValue ?? string.Empty).Contains(q)).ToList();
            ViewBag.LanguageId = languageId;
            ViewBag.Query = q;
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var languages = await _languageService.GetAllAsync();
            var model = new ResourceEditModel
            {
                Languages = languages.ToList()
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResourceEditModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Languages = (await _languageService.GetAllAsync()).ToList();
                return View(model);
            }

            var existing = await _localizationService.GetByNameAsync(model.ResourceName.Trim(), model.LanguageId);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.ResourceName), "Resource already exists");
                model.Languages = (await _languageService.GetAllAsync()).ToList();
                return View(model);
            }

            await _localizationService.InsertResourceAsync(new App.Core.Domain.Localization.LocaleStringResource
            {
                LanguageId = model.LanguageId,
                ResourceName = model.ResourceName.Trim(),
                ResourceValue = model.ResourceValue ?? string.Empty
            });

            TempData["Success"] = "Saved";
            return RedirectToAction(nameof(Index), new { languageId = model.LanguageId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _localizationService.GetByIdAsync(id);
            if (resource == null) return NotFound();

            var languages = await _languageService.GetAllAsync();

            var model = new ResourceEditModel
            {
                Id = resource.Id,
                ResourceName = resource.ResourceName,
                ResourceValue = resource.ResourceValue,
                LanguageId = resource.LanguageId,
                Languages = languages.ToList()
            };

            return View(model);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ResourceEditModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Languages = (await _languageService.GetAllAsync()).ToList();
                return View(model);
            }

            var resource = await _localizationService.GetByIdAsync(model.Id);
            if (resource == null) return NotFound();

            resource.ResourceName = model.ResourceName.Trim();
            resource.ResourceValue = model.ResourceValue ?? string.Empty;
            resource.LanguageId = model.LanguageId;

            await _localizationService.UpdateResourceAsync(resource);

            TempData["Success"] = "Saved";
            return RedirectToAction(nameof(Index), new { languageId = model.LanguageId });
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int languageId, int id)
        {
            var resource = await _localizationService.GetByIdAsync(id);
            if (resource == null) return NotFound();
            await _localizationService.DeleteResourceAsync(resource);
            TempData["Success"] = "Deleted";
            return RedirectToAction(nameof(Index), new { languageId });
        }
    }
}
