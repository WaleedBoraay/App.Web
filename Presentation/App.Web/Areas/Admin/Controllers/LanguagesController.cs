using App.Core.Domain.Localization;
using App.Services.Localization;
using App.Web.Areas.Admin.Models;
using App.Web.Infrastructure.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class LanguagesController : Controller
{
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    public LanguagesController(
        ILanguageService languageService,
        ILocalizationService localizationService
        )
    {
        _languageService = languageService;
        _localizationService = localizationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var languages = await _languageService.GetAllAsync();
        var model = languages.Select(language => new LanguageModel
        {
            Id = language.Id,
            Name = language.Name,
            LanguageCulture = language.LanguageCulture,
            UniqueSeoCode = language.UniqueSeoCode,
            FlagImageFileName = language.FlagImageFileName,
            Rtl = language.Rtl,
            Published = language.Published,
            DisplayOrder = language.DisplayOrder
        }).ToList();
        return View(model);
    }

    [HttpGet]
    public IActionResult Create() => View(new LanguageModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LanguageModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = new Language
        {
            Name = model.Name,
            LanguageCulture = model.LanguageCulture,
            UniqueSeoCode = model.UniqueSeoCode,
            FlagImageFileName = model.FlagImageFileName,
            Rtl = model.Rtl,
            Published = model.Published,
            DisplayOrder = model.DisplayOrder
        };
        await _languageService.InsertAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var language = await _languageService.GetByIdAsync(id);
        if (language == null) return NotFound();

        var model = new LanguageModel
        {
            Id = language.Id,
            Name = language.Name,
            LanguageCulture = language.LanguageCulture,
            UniqueSeoCode = language.UniqueSeoCode,
            FlagImageFileName = language.FlagImageFileName,
            Rtl = language.Rtl,
            Published = language.Published,
            DisplayOrder = language.DisplayOrder
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LanguageModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var entity = await _languageService.GetByIdAsync(model.Id);
        if (entity == null) return NotFound();
        var updateentity = new Language
        {
            Name = model.Name,
            LanguageCulture = model.LanguageCulture,
            UniqueSeoCode = model.UniqueSeoCode,
            FlagImageFileName = model.FlagImageFileName,
            Rtl = model.Rtl,
            Published = model.Published,
            DisplayOrder = model.DisplayOrder
        };
        await _languageService.UpdateAsync(entity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _languageService.GetByIdAsync(id);
        if (entity != null)
            await _languageService.DeleteAsync(entity);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Change(string lang, string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(lang))
        {
            Response.Cookies.Append(
                "App.WorkingLanguage",
                lang,
                new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
            );
        }

        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home"));
    }

}
