using App.Core.Domain.Organization;
using App.Services.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SectorsController : Controller
    {
        private readonly IOrganizationsServices _organizationsServices;
        private readonly ISectorServices _sectorServices;

		public SectorsController(IOrganizationsServices organizationsServices, ISectorServices sectorServices)
        {
            _organizationsServices = organizationsServices;
            _sectorServices = sectorServices;
		}

        public async Task<IActionResult> Index()
        {
            var sectors = await _sectorServices.GetAllSectorsAsync();
            return View(sectors);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(Sector model)
        {
            if (!ModelState.IsValid)
                return View(model);
            await _sectorServices.CreateSectorAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sector = await _sectorServices.GetSectorByIdAsync(id);
            if (sector == null)
                return NotFound();
            return View(sector);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Sector model)
        {
            if (!ModelState.IsValid)
                return View(model);
            await _sectorServices.UpdateSectorAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var sector = await _sectorServices.GetSectorByIdAsync(id);
            if (sector != null)
                await _sectorServices.DeleteSectorAsync(sector);

            return RedirectToAction(nameof(Index));
        }
    }
}
