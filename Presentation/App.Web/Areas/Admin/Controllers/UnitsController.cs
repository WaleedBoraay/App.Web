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
        private readonly IUnitServices _unitServices;

		public UnitsController(IOrganizationsServices organizationsServices, IUnitServices unitServices)
        {
            _organizationsServices = organizationsServices;
            _unitServices = unitServices;
        }

        public async Task<IActionResult> Index()
        {
            var Units = await _unitServices.GetAllUnitsAsync();
            return View(Units);
        }

        [HttpGet]
        public IActionResult Create(int departmentId)
        {
            return View(new Unit { DepartmentId = departmentId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Unit model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _unitServices.CreateUnitAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var unit = await _unitServices.GetUnitByIdAsync(id);
            if (unit == null)
                return NotFound();

            return View(unit);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Unit model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _unitServices.UpdateUnitAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int unitId)
        {
            var unit = await _unitServices.GetUnitByIdAsync(id);
            if (unit != null)
                await _unitServices.DeleteUnitAsync(unit);

            return RedirectToAction(nameof(Index));
        }
    }
}
