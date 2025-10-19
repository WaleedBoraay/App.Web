using App.Core.Domain.Organization;
using App.Services.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UnitsController : Controller
    {
        private readonly IOrganizationsServices _organizationsServices;

        public UnitsController(IOrganizationsServices organizationsServices)
        {
            _organizationsServices = organizationsServices;
        }

        public async Task<IActionResult> Index(int departmentId)
        {
            var units = await _organizationsServices.GetUnitsByDepartmentIdAsync(departmentId);
            ViewBag.DepartmentId = departmentId;
            return View(units);
        }

        [HttpGet]
        public IActionResult Create(int departmentId)
        {
            return View(new Department { SectorId = departmentId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Department model)
        {
            if (!ModelState.IsValid) return View(model);
            await _organizationsServices.CreateUnitAsync(model);
            return RedirectToAction(nameof(Index), new { departmentId = model.SectorId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _organizationsServices.GetUnitByIdAsync(id);
            if (unit == null) return NotFound();
            return View(unit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Department model)
        {
            if (!ModelState.IsValid) return View(model);
            await _organizationsServices.UpdateUnitAsync(model);
            return RedirectToAction(nameof(Index), new { departmentId = model.SectorId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int departmentId)
        {
            var unit = await _organizationsServices.GetUnitByIdAsync(id);
            if (unit != null)
                await _organizationsServices.DeleteUnitAsync(unit);

            return RedirectToAction(nameof(Index), new { departmentId });
        }
    }
}
