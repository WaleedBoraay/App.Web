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

        public DepartmentsController(IOrganizationsServices organizationsServices)
        {
            _organizationsServices = organizationsServices;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _organizationsServices.GetAllDepartmentsAsync();
            return View(departments);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(Department model)
        {
            if (!ModelState.IsValid) return View(model);
            await _organizationsServices.CreateDepartmentAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dept = await _organizationsServices.GetDepartmentByIdAsync(id);
            if (dept == null) return NotFound();
            return View(dept);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Department model)
        {
            if (!ModelState.IsValid) return View(model);
            await _organizationsServices.UpdateDepartmentAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _organizationsServices.GetDepartmentByIdAsync(id);
            if (dept != null)
                await _organizationsServices.DeleteDepartmentAsync(dept);

            return RedirectToAction(nameof(Index));
        }
    }
}
