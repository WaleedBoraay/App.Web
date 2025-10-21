using App.Core.Domain.Organization;
using App.Services.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DepartmentsController : Controller
    {
        private readonly IOrganizationsServices _organizationsServices;
        private readonly IDepartmentServices _departmentServices;
        private readonly ISectorServices _sectorServices;

		public DepartmentsController(IOrganizationsServices organizationsServices, IDepartmentServices departmentServices, ISectorServices sectorServices)
        {
            _organizationsServices = organizationsServices;
            _departmentServices = departmentServices;
            _sectorServices = sectorServices;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _departmentServices.GetAllDepartmentsAsync();
            return View(departments);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int sectorId)
        {
            if (sectorId <= 0) 
                return BadRequest();
			var sector = await _sectorServices.GetSectorByIdAsync(sectorId);
            var department = new Department
            {
                SectorId = sectorId,
                Sector = sector
			};
			return View(department);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Department model)
        {
            if (!ModelState.IsValid) 
                return View(model);
            await _departmentServices.CreateDepartmentAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _departmentServices.GetDepartmentByIdAsync(id);
            if (department == null) 
                return NotFound();
            return View(department);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Department model)
        {
            if (!ModelState.IsValid)
                return View(model);
            await _departmentServices.UpdateDepartmentAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int departmentId)
        {
            var department = await _departmentServices.GetDepartmentByIdAsync(id);
            if (department != null)
                await _departmentServices.DeleteDepartmentAsync(department);

            return RedirectToAction(nameof(Index));
        }
    }
}
