using App.Core.Domain.Organization;
using App.Services.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubUnitsController : Controller
    {
        private readonly IOrganizationsServices _organizationsServices;

        public SubUnitsController(IOrganizationsServices organizationsServices)
        {
            _organizationsServices = organizationsServices;
        }

        public async Task<IActionResult> Index(int unitId)
        {
            var subUnits = await _organizationsServices.GetSubUnitsByUnitIdAsync(unitId);
            ViewBag.UnitId = unitId;
            return View(subUnits);
        }

        [HttpGet]
        public IActionResult Create(int unitId)
        {
            return View(new SubUnit { UnitId = unitId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubUnit model)
        {
            if (!ModelState.IsValid) return View(model);
            await _organizationsServices.CreateSubUnitAsync(model);
            return RedirectToAction(nameof(Index), new { unitId = model.UnitId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var subUnit = await _organizationsServices.GetSubUnitByIdAsync(id);
            if (subUnit == null) return NotFound();
            return View(subUnit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubUnit model)
        {
            if (!ModelState.IsValid) return View(model);
            await _organizationsServices.UpdateSubUnitAsync(model);
            return RedirectToAction(nameof(Index), new { unitId = model.UnitId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int unitId)
        {
            var subUnit = await _organizationsServices.GetSubUnitByIdAsync(id);
            if (subUnit != null)
                await _organizationsServices.DeleteSubUnitAsync(subUnit);

            return RedirectToAction(nameof(Index), new { unitId });
        }
    }
}
