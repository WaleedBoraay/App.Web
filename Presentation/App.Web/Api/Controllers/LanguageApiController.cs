using App.Core.Domain.Localization;
using App.Services.Localization;
using App.Web.Areas.Admin.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LanguageApiController : ControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;

    public LanguageApiController(
        ILanguageService languageService,
        ILocalizationService localizationService
    )
    {
        _languageService = languageService;
        _localizationService = localizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
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

        return Ok(model);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
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

        return Ok(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LanguageModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

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
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LanguageModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = await _languageService.GetByIdAsync(id);
        if (entity == null) return NotFound();

        entity.Name = model.Name;
        entity.LanguageCulture = model.LanguageCulture;
        entity.UniqueSeoCode = model.UniqueSeoCode;
        entity.FlagImageFileName = model.FlagImageFileName;
        entity.Rtl = model.Rtl;
        entity.Published = model.Published;
        entity.DisplayOrder = model.DisplayOrder;

        await _languageService.UpdateAsync(entity);
        return Ok(entity);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _languageService.GetByIdAsync(id);
        if (entity == null) return NotFound();

        await _languageService.DeleteAsync(entity);
        return NoContent();
    }

    [HttpPost("change")]
    public IActionResult Change([FromQuery] string lang, [FromQuery] string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(lang))
        {
            Response.Cookies.Append(
                "App.WorkingLanguage",
                lang,
                new CookieOptions { Expires = DateTime.UtcNow.AddYears(1) }
            );
        }

        return Ok(new { success = true, returnUrl = returnUrl ?? "/" });
    }
}
