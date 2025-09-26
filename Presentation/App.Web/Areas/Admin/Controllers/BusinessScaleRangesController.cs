using App.Core.Domain.Ref;
using App.Core.RepositoryServices;
using App.Services.Localization;
using App.Web.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class BusinessScaleRangesController : Controller
{
    private readonly IRepository<BusinessScaleRange> _businessScaleRangerepository;
    private readonly ILocalizationService _localizationService;
    public BusinessScaleRangesController(ILocalizationService localizationService,
        IRepository<BusinessScaleRange> businessScaleRangerepository
        )
    {
        _localizationService = localizationService;
        _businessScaleRangerepository = businessScaleRangerepository;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _businessScaleRangerepository.GetAllAsync(query =>
        query.OrderBy(x => x.MinValue));
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new BusinessScaleRangeModel();
        return View("Edit", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BusinessScaleRangeModel model, CancellationToken ct)
    {
        if (model.MinValue > model.MaxValue)
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.BusinessScaleRanges.Fields.MinGreaterThanMax"));

        if (!ModelState.IsValid)
            return View("Edit", model);
        await _businessScaleRangerepository.InsertAsync(new BusinessScaleRange
        {
            RangeLabel = model.RangeLabel,
            MinValue = model.MinValue,
            MaxValue = model.MaxValue
        });

        //add success message like notifycation 
        TempData["Success"] = await _localizationService.GetResourceAsync("Admin.Common.Saved");

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _businessScaleRangerepository.GetByIdAsync(id);
        if (model is null)
            return NotFound();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BusinessScaleRangeModel model)
    {
        if (model.MinValue > model.MaxValue)
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.BusinessScaleRanges.Fields.MinGreaterThanMax"));
        if (!ModelState.IsValid)
            return View(model);
        var e = await _businessScaleRangerepository.GetByIdAsync(id);
        if (e is null)
            return NotFound();
        e.RangeLabel = model.RangeLabel;
        e.MinValue = model.MinValue;
        e.MaxValue = model.MaxValue;
        await _businessScaleRangerepository.UpdateAsync(e);
        TempData["Success"] = await _localizationService.GetResourceAsync("Admin.Common.Saved");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _businessScaleRangerepository.GetByIdAsync(id);
        if (e is null)
            return NotFound();
        await _businessScaleRangerepository.DeleteAsync(e);
        TempData["Success"] = await _localizationService.GetResourceAsync("Admin.Common.Deleted");
        return RedirectToAction(nameof(Index));
    }
}
