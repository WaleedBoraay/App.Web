using App.Core.Domain.Directory;
using App.Core.RepositoryServices;
using App.Services.Directory;
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
public class CountryController : Controller
{
    private readonly ICountryService _countryService;

    public CountryController(ICountryService countryService)
    {
        _countryService = countryService;
    }

    [HttpGet]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var countries = await _countryService.GetAllAsync();
        var model = countries.Select(c => c.ToModel<CountryModel>()).ToList();
        return View(model);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CountryModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _countryService.InsertAsync(model.ToEntity<Country>());
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var country = await _countryService.GetByIdAsync(id);
        if (country == null) return NotFound();

        return View(country.ToModel<CountryModel>());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CountryModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _countryService.UpdateAsync(model.ToEntity<Country>());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _countryService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
