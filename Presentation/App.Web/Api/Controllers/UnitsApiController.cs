using App.Core.Domain.Organization;
using App.Services.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/units")]
    public class UnitsApiController : ControllerBase
    {
        private readonly IUnitServices _unitServices;

		public UnitsApiController(IUnitServices unitServices)
        {
            _unitServices = unitServices;
        }

        [HttpGet("department/{departmentId:int}")]
        public async Task<IActionResult> GetByUnit(int departmentId)
        {
            var units = await _unitServices.GetUnitsByDepartmentIdAsync(departmentId);
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
