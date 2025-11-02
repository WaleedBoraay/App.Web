using App.Core.Domain.Organization;
using App.Core.Domain.Registrations;
using App.Services.Organization;
using App.Services.Registrations;
using App.Web.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Api.Admin.Controllers
{
	[ApiController]
	[Route("api/departments")]
	public class DepartmentsApiController : ControllerBase
	{
		private readonly IOrganizationsServices _organizationsServices;
		private readonly ISectorServices _sectorServices;
		private readonly IDepartmentServices _departmentServices;
		private readonly IContactService _contactService;
		public DepartmentsApiController(IOrganizationsServices organizationsServices, IDepartmentServices departmentServices, ISectorServices sectorServices,IContactService contactService)
		{
			_organizationsServices = organizationsServices;
			_departmentServices = departmentServices;
			_sectorServices = sectorServices;
			_contactService = contactService;
		}

		[HttpGet("getalldepartments")]
		public async Task<IActionResult> GetAllDepartments()
		{
			var departments = await _departmentServices.GetAllDepartmentsAsync();
			var result = new List<DepartmentModel>();

			foreach (var dep in departments)
			{
				var sector = await _sectorServices.GetSectorByIdAsync(dep.SectorId);
				var contact = await _contactService.GetByIdAsync(dep.ContactId);

				var model = new DepartmentModel
				{
					Id = dep.Id,
					DepartmentName = dep.Name,
					DepartmentDescription = dep.DepartmentDescription,
					SectorId = dep.SectorId,
					SectorName = sector?.Name ?? string.Empty,
					ContactId = dep.ContactId,
					ContactName = contact != null ? $"{contact.FirstName} {contact.LastName}" : string.Empty,
					ContactTypeId = dep.ContactTypeId,
					ContactTypes = (ContactType)dep.ContactTypeId
				};

				result.Add(model);
			}

			return Ok(result);
		}

		[HttpGet("sector/{sectorId:int}")]
		public async Task<IActionResult> GetBySector(int sectorId)
		{
			var departments = await _departmentServices.GetDepartmentsBySectorIdAsync(sectorId);

			foreach (var dip in departments)
			{
				var sec = await _sectorServices.GetSectorByIdAsync(dip.SectorId);
				var model = new DepartmentModel
				{
					Id = dip.Id,
					DepartmentName = dip.Name,
					DepartmentDescription = dip.DepartmentDescription,
					SectorId = dip.SectorId,
					ContactId = dip.ContactId,
					ContactTypeId = dip.ContactTypeId,
					SectorName = sec.Name
				};
				return Ok(model);

			}
			return Ok(departments);
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetById(int id)
		{
			var department = await _departmentServices.GetDepartmentByIdAsync(id);
			if (department == null)
				return NotFound(new { message = "Department not found" });
			var model = new DepartmentModel
			{
				Id = department.Id,
				DepartmentName = department.Name,
				DepartmentDescription = department.DepartmentDescription,
				SectorId = department.SectorId,
			};

			return Ok(model);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] DepartmentModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var dep = new Department
			{
				Name = model.DepartmentName,
				DepartmentDescription = model.DepartmentDescription,
				SectorId = model.SectorId
			};
			await _departmentServices.CreateDepartmentAsync(dep);
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
			existing.DepartmentDescription = model.DepartmentDescription;
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
