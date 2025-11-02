using App.Core.Domain.Organization;
using App.Services.Organization;
using App.Web.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/units")]
    public class UnitsApiController : ControllerBase
    {
        private readonly IUnitServices _unitServices;
        private readonly IDepartmentServices _departmentServices;
        private readonly ISectorServices _sectorServices;

		public UnitsApiController(IUnitServices unitServices, IDepartmentServices departmentServices,ISectorServices sectorServices)
        {
            _unitServices = unitServices;
            _departmentServices = departmentServices;
            _sectorServices = sectorServices;
		}

        [HttpGet("getallunits")]
		public async Task<IActionResult> GetAllUnits()
        {
            var units = await _unitServices.GetAllUnitsAsync();
            var result = new List<UnitModel>();
			foreach (var unit in units)
            {
                var department = await _departmentServices.GetDepartmentByIdAsync(unit.DepartmentId);
                var sector = await _sectorServices.GetSectorByIdAsync(department.SectorId);
				var model = new UnitModel
                {
                    Id = unit.Id,
                    UnitName = unit.Name,
                    DepartmentId = unit.DepartmentId,
                    DepartmentName = department != null ? department.Name : string.Empty,
                    SectorName = sector != null ? sector.Name : string.Empty,
				};
                result.Add(model);

			}
			return Ok(result);
        }


		[HttpGet("department/{departmentId:int}")]
        public async Task<IActionResult> GetByUnit(int departmentId)
        {
            var units = await _unitServices.GetUnitsByDepartmentIdAsync(departmentId);
            foreach (var unit in units)
                {
                var department = await _departmentServices.GetDepartmentByIdAsync(unit.DepartmentId);
                var model = new UnitModel
                {
                    Id = unit.Id,
                    UnitName = unit.Name,
                    DepartmentId = unit.DepartmentId,
                    DepartmentName = department != null ? department.Name : string.Empty,

                };
			}
			return Ok(units);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            //Get all Units

            var unit = await _unitServices.GetUnitByIdAsync(id);
            if (unit == null)
                return NotFound(new { message = "Unit not found" });

            return Ok(unit);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Unit model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _unitServices.CreateUnitAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Unit model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _unitServices.GetUnitByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Unit not found" });

            existing.Name = model.Name;
            existing.DepartmentId = model.DepartmentId;
            await _unitServices.UpdateUnitAsync(existing);
            return Ok(existing);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _unitServices.GetUnitByIdAsync(id);
            if (unit == null)
                return NotFound(new { message = "Unit not found" });

            await _unitServices.DeleteUnitAsync(unit);
            return NoContent();
        }
	}
}
