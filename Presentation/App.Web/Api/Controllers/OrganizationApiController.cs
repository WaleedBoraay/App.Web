using App.Core.Domain.Notifications;
using App.Core.Domain.Organization;
using App.Core.Domain.Users;
using App.Services.Notifications;
using App.Services.Organization;
using App.Services.Registrations;
using App.Services.Security;
using App.Services.Users;
using App.Web.Api.Models;
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
        private readonly INotificationService _notificationService;
        private readonly IContactService _contactService;
        private readonly IEmailService _emailService;
        private readonly IRegistrationService _registrationService;



		public OrganizationApiController(
            IUserService userService,
            IRoleService roleService,
            ISectorServices sectorServices,
            IDepartmentServices departmentServices,
            IUnitServices unitServices,
            INotificationService notificationService,
            IContactService contactService,
            IEmailService emailService,
            IRegistrationService registrationService)
        {
            _userService = userService;
            _roleService = roleService;
            _sectorServices = sectorServices;
            _departmentServices = departmentServices;
            _unitServices = unitServices;
            _notificationService = notificationService;
            _contactService = contactService;
            _emailService = emailService;
            _registrationService = registrationService;
		}

		private async Task<string> GenerateRandomPassword(int length = 8)
		{
			const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
			var random = new Random();
			return new string(Enumerable.Repeat(validChars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		//Get the entire organization structure with users and roles
		[HttpGet("GetOrganizationWithUserId")]
		public async Task<IActionResult> GetOrganizationWithUserId(int userId)
		{
			var user = await _userService.GetByIdAsync(userId);
			if (user == null)
				return NotFound(new { message = "User not found" });

			if (!user.RegistrationId.HasValue || user.RegistrationId.Value <= 0)
				return BadRequest(new { message = "User is not assigned to any bank (registration)." });

			var registration = await _registrationService.GetByIdAsync(user.RegistrationId.Value);
			if (registration == null)
				return NotFound(new { message = "Registration not found" });

			var registrationId = registration.Id;

			var contacts = (await _contactService.GetAllAsync())
				.Where(c => c.RegistrationId == registrationId)
				.ToList();

			var sectors = (await _sectorServices.GetAllSectorsAsync())
				.Where(s => s.ContactId != 0 && contacts.Any(c => c.Id == s.ContactId))
				.ToList();

			var departments = (await _departmentServices.GetAllDepartmentsAsync())
				.Where(d => d.ContactId != 0 && contacts.Any(c => c.Id == d.ContactId))
				.ToList();

			var units = (await _unitServices.GetAllUnitsAsync())
				.Where(u => u.ContactId != 0 && contacts.Any(c => c.Id == u.ContactId))
				.ToList();

			var hasData = contacts.Any() || sectors.Any() || departments.Any() || units.Any();
			if (!hasData)
			{
				return Ok(new
				{
					message = "No organization structure found for this bank. Please add your organization data.",
					registration = new
					{
						registration.Id,
						registration.InstitutionName,
						registration.LicenseNumber
					},
					hasData = false
				});
			}

			var organization = sectors.Select(sector => new
			{
				SectorId = sector.Id,
				SectorName = sector.Name,
				SectorDescription = sector.SectorDescription,
				ContactId = sector.ContactId,
				ContactTypeId = sector.ContactTypeId,
				ContactTypeName = sector.ContactTypes.ToString(),
				Departments = departments
					.Where(d => d.SectorId == sector.Id)
					.Select(dep => new
					{
						DepartmentId = dep.Id,
						DepartmentName = dep.Name,
						DepartmentDescription = dep.DepartmentDescription,
						ContactId = dep.ContactId,
						ContactTypeId = dep.ContactTypeId,
						ContactTypeName = dep.ContactTypes.ToString(),
						Units = units
							.Where(u => u.DepartmentId == dep.Id)
							.Select(u => new
							{
								UnitId = u.Id,
								UnitName = u.Name,
								UnitDescription = u.Description,
								ContactId = u.ContactId,
								ContactTypeId = u.ContactTypeId,
								ContactTypeName = u.ContactTypes.ToString()
							}).ToList()
					}).ToList()
			}).ToList();

			var response = new
			{
				message = "Organization structure retrieved successfully.",
				hasData = true,
				registration = new
				{
					registration.Id,
					registration.InstitutionName,
					registration.LicenseNumber
				},
				user = new
				{
					user.Id,
					user.Username,
					user.Email,
					user.RegistrationId,
					user.SectorId,
					user.DepartmentId,
					user.UnitId
				},
				contacts = contacts.Select(c => new
				{
					c.Id,
					c.FirstName,
					c.MiddleName,
					c.LastName,
					c.Email,
					c.ContactPhone,
					c.BusinessPhone,
					c.JobTitle,
					c.ContactTypeId,
					ContactTypeName = c.ContactTypes.ToString(),
					c.NationalityCountryId
				}),
				organization
			};

			return Ok(organization);
		}



		[HttpGet("structure")]
        public async Task<IActionResult> GetOrganizationStructure()
        {
            var sectors = await _sectorServices.GetAllSectorsAsync();
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
            var notifiedUsers = allUsers.Where(u => u.SectorId == null || u.DepartmentId == null || u.UnitId == null)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    Roles = _roleService.GetRolesByUserIdAsync(u.Id).Result
                        .Select(r => new { r.Id, r.Name })
                        .ToList()
                });
            var senNotNotifications = _notificationService.SendAsync(
                null,
                NotificationEvent.SectorCreated,
                0,
                0,
                NotificationChannel.InApp,
                new Dictionary<string, string>
                {
                    { "Message", "Organization structure was retrieved." }
                });
			return Ok(new
            {
                Departments = model,
                AllUsers = allUsers.Select(u => new { u.Id, u.Username, u.Email }),
                AllRoles = allRoles.Select(r => new { r.Id, r.Name }),
                Nofifi = notifiedUsers,
                Notify = senNotNotifications
			});
        }


		#region Sector
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

		#endregion

		#region Department
		[HttpPost("department")]
        public async Task<IActionResult> CreateDepartmentBySectorId([FromBody] DepartmentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Department name is required." });
            var department = new Department
            {
                Name = model.DepartmentName,
				DepartmentDescription = model.DepartmentDescription,
                SectorId = model.SectorId,
                ContactId = model.ContactId,
                ContactTypeId = model.ContactTypeId
			};
			await _departmentServices.CreateDepartmentAsync(department);
            return CreatedAtAction(nameof(GetOrganizationStructure), new { id = model.Id }, model);
        }

        [HttpDelete("department/{id:int}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _departmentServices.GetDepartmentByIdAsync(id);
            if (department == null)
                return NotFound(new { message = "Department not found" });

            await _departmentServices.DeleteDepartmentAsync(department);
            return NoContent();
        }

		#endregion

		#region Unit
		[HttpPost("unit")]
        public async Task<IActionResult> CreateUnitByDepartmentId([FromBody] UnitModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Unit name is required." });

            var unit = new Unit
                {
                Name = model.UnitName,
                Description = model.UnitDescription,
                DepartmentId = model.DepartmentId,
                ContactId = model.ContactId,
                ContactTypeId = model.ContactTypeId
			};

			await _unitServices.CreateUnitAsync(unit);
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

		#endregion

		//assign user to sector
		[HttpPost("assign-user-to-sector")]
		public async Task<IActionResult> AssignUser([FromBody] AssignUserModel model)
		{
			var user = await _userService.GetByIdAsync(model.UserId);
			if (user == null)
				return NotFound(new { message = "User not found" });

            var password = await GenerateRandomPassword(10);
			// Update sector if provided
			if (model.SectorId != null)
			{
				user.SectorId = model.SectorId;
				await _userService.UpdateAsync(user);
			}

			// Assign role if valid
			if (model.RoleId.HasValue && model.RoleId.Value != 0)
			{
				var userRoles = await _roleService.GetRolesByUserIdAsync(model.UserId);
				bool alreadyHasRole = userRoles.Any(r => r.Id == model.RoleId.Value);

				if (!alreadyHasRole)
				{
					await _roleService.AddUserToRoleAsync(model.UserId, model.RoleId.Value);
				}
			}

			return Ok(new { message = "User assigned successfully." });
		}
		//assign user to department
		[HttpPost("assign-user-to-department")]
        public async Task<IActionResult> AssignUserToDepartment([FromBody] AssignUserModel model)
        {
            var user = await _userService.GetByIdAsync(model.UserId);
			if (user == null)
                return NotFound(new { message = "User not found" });
            if (model.DepartmentId != null)
            {
				user.DepartmentId = model.DepartmentId;
				await _userService.UpdateAsync(user);
			}


			// Assign role if valid
			if (model.RoleId.HasValue && model.RoleId.Value != 0)
			{
				var userRoles = await _roleService.GetRolesByUserIdAsync(model.UserId);
				bool alreadyHasRole = userRoles.Any(r => r.Id == model.RoleId.Value);

				if (!alreadyHasRole)
				{
					await _roleService.AddUserToRoleAsync(model.UserId, model.RoleId.Value);
				}
			}
			return Ok(new { message = "User assigned to department successfully." });
		}

        //assign user to unit
        [HttpPost("assign-user-to-unit")]
        public async Task<IActionResult> AssignUserToUnit([FromBody] AssignUserModel model)
        {
            var user = await _userService.GetByIdAsync(model.UserId);

			if (user == null)
                return NotFound(new { message = "User not found" });

            if (model.UnitId != null)
            {
				user.UnitId = model.UnitId;
				await _userService.UpdateAsync(user);
			}


			// Assign role if valid
			if (model.RoleId.HasValue && model.RoleId.Value != 0)
			{
				var userRoles = await _roleService.GetRolesByUserIdAsync(model.UserId);
				bool alreadyHasRole = userRoles.Any(r => r.Id == model.RoleId.Value);

				if (!alreadyHasRole)
				{
					await _roleService.AddUserToRoleAsync(model.UserId, model.RoleId.Value);
				}
			}
			return Ok(new { message = "User assigned to unit successfully." });
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
