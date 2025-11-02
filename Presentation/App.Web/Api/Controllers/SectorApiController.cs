using App.Core.Domain.Organization;
using App.Core.Domain.Registrations;
using App.Core.Domain.Users;
using App.Services.Organization;
using App.Services.Registrations;
using App.Web.Areas.Admin.Models.Organization;
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
        private readonly IRegistrationService _registrationServices;
        private readonly IContactService _contactService;

		public SectorApiController(IOrganizationsServices organizationsServices, ISectorServices sectorServices, IRegistrationService registrationService, IContactService contactService)
        {
            _organizationsServices = organizationsServices;
            _sectorServices = sectorServices;
            _registrationServices = registrationService;
            _contactService = contactService;
		}

        // GET: api/admin/departments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sectors = await _sectorServices.GetAllSectorsAsync();
            var contact = await _contactService.GetAllAsync();
			var model = sectors.Select(s => new SectorModel
            {
                Id = s.Id,
                SectorName = s.Name,
                SectorDescription = s.SectorDescription,
                ContactId = s.ContactId,
                ContactTypeId = s.ContactTypeId,
				Contacts = contact

            }).ToList();
			return Ok(model);
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
        public async Task<IActionResult> Create([FromBody] SectorModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
			//Create sector entity
            var sector = new Sector
            {
                Name = model.SectorName,
                SectorDescription = model.SectorDescription,    
                ContactId = model.ContactId,
                ContactTypeId = model.ContactTypeId
			};
            var insSec = await _sectorServices.CreateSectorAsync(sector);

			return Ok(new { success = true, message = "Sector created successfully" });
        }

        // PUT: api/admin/departments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Sector model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sector = await _sectorServices.GetSectorByIdAsync(id);
            if (sector == null) return NotFound();

            model.Id = id;
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
