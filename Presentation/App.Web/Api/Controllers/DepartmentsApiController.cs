using App.Core.Domain.Organization;
using App.Services.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Api.Admin.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsApiController : ControllerBase
    {
        private readonly IOrganizationsServices _organizationsServices;
        private readonly IDepartmentServices _departmentServices;

		public DepartmentsApiController(IOrganizationsServices organizationsServices, IDepartmentServices departmentServices)
        {
            _organizationsServices = organizationsServices;
            _departmentServices = departmentServices;
        }

        [HttpGet("sector/{sectorId:int}")]
        public async Task<IActionResult> GetBySector(int sectorId)
        {
            var departments = await _departmentServices.GetDepartmentsBySectorIdAsync(sectorId);
            return Ok(departments);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _departmentServices.GetDepartmentByIdAsync(id);
            if (department == null)
                return NotFound(new { message = "Department not found" });

            return Ok(department);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Department model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _departmentServices.CreateDepartmentAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Department model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _departmentServices.GetDepartmentByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Department not found" });

            existing.Name = model.Name;
            existing.SectorId = model.SectorId;
            await _departmentServices.UpdateDepartmentAsync(existing);
            return Ok(existing);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _departmentServices.GetDepartmentByIdAsync(id);
            if (department == null)
                return NotFound(new { message = "Department not found" });

            await _departmentServices.DeleteDepartmentAsync(department);
            return NoContent();
        }
    }
}
