using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Directory;
using App.Web.Areas.Admin.Models;
using App.Core.Domain.Directory;
using App.Web.Infrastructure.Mapper;
using System.Threading.Tasks;
using System.Linq;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StateProvinceController : Controller
    {
        private readonly IStateProvinceService _stateService;

        public StateProvinceController(IStateProvinceService stateService)
        {
            _stateService = stateService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int countryId)
        {
            var states = await _stateService.GetByCountryIdAsync(countryId);
            var model = states.Select(s => s.ToModel<StateProvinceModel>()).ToList();
            ViewBag.CountryId = countryId;
            return View(model);
        }

        [HttpGet]
        public IActionResult Create(int countryId)
            => View(new StateProvinceModel { CountryId = countryId });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StateProvinceModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = model.ToEntity<StateProvince>();
            await _stateService.InsertAsync(entity);
            return RedirectToAction(nameof(Index), new { countryId = model.CountryId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var state = await _stateService.GetByIdAsync(id);
            if (state == null) return NotFound();

            var model = state.ToModel<StateProvinceModel>();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StateProvinceModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = await _stateService.GetByIdAsync(model.Id.Value);
            if (entity == null) return NotFound();

            model.ToEntity(entity);
            await _stateService.UpdateAsync(entity);
            return RedirectToAction(nameof(Index), new { countryId = model.CountryId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int countryId)
        {
            var entity = await _stateService.GetByIdAsync(id);
            if (entity != null)
                await _stateService.DeleteAsync(entity.Id);

            return RedirectToAction(nameof(Index), new { countryId });
        }
    }
}
