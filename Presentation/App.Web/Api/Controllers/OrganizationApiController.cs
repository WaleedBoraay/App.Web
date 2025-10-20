using App.Core.Domain.Organization;
using App.Services.Organization;
using App.Services.Security;
using App.Services.Users;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ISectorServices _sectorServices;
        private readonly IDepartmentServices _departmentServices;
        private readonly IUnitServices _unitServices;



		public OrganizationApiController(
            IUserService userService,
            IRoleService roleService,
            ISectorServices sectorServices,
            IDepartmentServices departmentServices,
            IUnitServices unitServices)
        {
            _userService = userService;
            _roleService = roleService;
            _sectorServices = sectorServices;
            _departmentServices = departmentServices;
            _unitServices = unitServices;
        }

        [HttpGet("structure")]
        public async Task<IActionResult> GetOrganizationStructure()
        {
            IList<Sector>? sectors = await _sectorServices.GetAllSectorsAsync();
            var departments = await _departmentServices.GetAllDepartmentsAsync();
            var units = await _unitServices.GetAllUnitsAsync();
            var allUsers = await _userService.GetAllAsync();
            var allRoles = await _roleService.GetAllAsync();

            var model = sectors.Select(sec => new
            {
                SectorId = sec.Id,
                SectorName = sec.Name,
                SectorDescription = sec.SectorDescription,
				Users = allUsers
                .Where(x => x.SectorId != null && x.SectorId == sec.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Username,
                    x.Email,
                    Roles = _roleService.GetRolesByUserIdAsync(x.Id).Result
                        .Select(r => new { r.Id, r.Name })
                        .ToList()
                }),

				Departments = departments
                    .Where(dep => dep.SectorId == sec.Id)
                    .Select(dep => new
                    {
                        UnitId = dep.Id,
                        UnitName = dep.Name,
                        Units = units
							.Where(uni => uni.DepartmentId == dep.Id)
                            .Select(uni => new
                            {
                                UnitId = uni.Id,
                                UnitName = uni.Name,
                                UniteDescriptione = uni.Description,
								Users = allUsers
                                    .Where(x => x.UnitId == dep.Id)
                                    .Select(x => new
                                    {
                                        x.Id,
                                        x.Username,
                                        x.Email,
                                        Roles = _roleService.GetRolesByUserIdAsync(x.Id).Result
                                            .Select(r => new { r.Id, r.Name })
                                            .ToList()
                                    })
                            })
                    })
            });

            return Ok(new
            {
                Departments = model,
                AllUsers = allUsers.Select(u => new { u.Id, u.Username, u.Email }),
                AllRoles = allRoles.Select(r => new { r.Id, r.Name })
            });
        }

        // ✅ Department endpoints
        [HttpPost("sector")]
        public async Task<IActionResult> CreateSector([FromBody] Sector model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { message = "sector name is required." });

            await _sectorServices.CreateSectorAsync(model);
            return CreatedAtAction(nameof(GetOrganizationStructure), new { id = model.Id }, model);
        }

        [HttpDelete("sector/{id:int}")]
        public async Task<IActionResult> DeleteSector(int id)
        {
            var sector = await _sectorServices.GetSectorByIdAsync(id);
            if (sector == null)
                return NotFound(new { message = "Sector not found" });

            await _sectorServices.DeleteSectorAsync(sector);
            return NoContent();
        }

        // ✅ Unit endpoints
        [HttpPost("department")]
        public async Task<IActionResult> CreateDepartment([FromBody] Department model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { message = "Department name is required." });

            await _departmentServices.CreateDepartmentAsync(model);
            return CreatedAtAction(nameof(GetOrganizationStructure), new { id = model.Id }, model);
        }

        [HttpDelete("department/{id:int}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var unit = await _departmentServices.GetDepartmentByIdAsync(id);
            if (unit == null)
                return NotFound(new { message = "Department not found" });

            await _departmentServices.DeleteDepartmentAsync(unit);
            return NoContent();
        }

        // ✅ SubUnit endpoints
        [HttpPost("unit")]
        public async Task<IActionResult> CreateUnit([FromBody] Unit model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { message = "Unit name is required." });

            await _unitServices.CreateUnitAsync(model);
            return CreatedAtAction(nameof(GetOrganizationStructure), new { id = model.Id }, model);
        }

        [HttpDelete("unit/{id:int}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var unit = await _unitServices.GetUnitByIdAsync(id);
            if (unit == null)
                return NotFound(new { message = "Unit not found" });

            await _unitServices.DeleteUnitAsync(unit);
            return NoContent();
        }

        // ✅ User assignment to org units
        [HttpPost("assign-user")]
        public async Task<IActionResult> AssignUser([FromBody] AssignUserModel model)
        {
            var user = await _userService.GetByIdAsync(model.UserId);
            if (user == null)
                return NotFound(new { message = "User not found" });
            if(user.SectorId != null)
            {
				user.SectorId = model.SectorId;
			}

            if(user.DepartmentId != null)
            {
                user.DepartmentId = model.DepartmentId;
			}
            if(user.UnitId != null)
            {
				user.UnitId = model.UnitId;
			}
			await _userService.UpdateAsync(user);

            if (model.RoleId.HasValue)
                await _roleService.AddUserToRoleAsync(model.UserId, model.RoleId.Value);

            return Ok(new { message = "User assigned successfully." });
        }

        [HttpPost("remove-user-role")]
        public async Task<IActionResult> RemoveUserRole([FromBody] RemoveUserRoleModel model)
        {
            await _roleService.RemoveUserFromRoleAsync(model.UserId, model.RoleId);
            return Ok(new { message = "Role removed from user." });
        }
    }

    public class AssignUserModel
    {
        public int UserId { get; set; }
        public int? RoleId { get; set; }
        public int? SectorId { get; set; }
        public int? DepartmentId { get; set; }
        public int? UnitId { get; set; }
    }

    public class RemoveUserRoleModel
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
