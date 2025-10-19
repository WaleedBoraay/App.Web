using App.Core.Domain.Organization;
using App.Services.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [Route("api/Sectors")]
    [ApiController]
    public class SectorApiController : ControllerBase
    {
        private readonly IOrganizationsServices _organizationsServices;
        private readonly ISectorServices _sectorServices;

		public SectorApiController(IOrganizationsServices organizationsServices, ISectorServices sectorServices)
        {
            _organizationsServices = organizationsServices;
            _sectorServices = sectorServices;
		}

        // GET: api/admin/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sectors = await _sectorServices.GetAllSectorsAsync();
            return Ok(sectors);
        }

        // GET: api/admin/departments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sector = await _sectorServices.GetSectorByIdAsync(id);
            if (sector == null) return NotFound();
            return Ok(sector);
        }

        // POST: api/admin/departments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Sector model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await _sectorServices.CreateSectorAsync(model);
            return Ok(new { success = true, message = "Sector created successfully" });
        }

        // PUT: api/admin/departments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Sector model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sector = await _sectorServices.GetSectorByIdAsync(id);
            if (sector == null) return NotFound();

            model.Id = id; // Ensure the id is set correctly
            await _sectorServices.UpdateSectorAsync(model);

            return Ok(new { success = true, message = "Sector updated successfully" });
        }

        // DELETE: api/admin/departments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sector = await _sectorServices.GetSectorByIdAsync(id);
            if (sector == null) return NotFound();

            await _sectorServices.DeleteSectorAsync(sector);
            return Ok(new { success = true, message = "Sector deleted successfully" });
        }
    }
}
