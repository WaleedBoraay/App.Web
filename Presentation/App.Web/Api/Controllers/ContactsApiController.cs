using App.Core.Domain.Registrations;
using App.Core.Domain.Users;
using App.Services;
using App.Services.Audit;
using App.Services.Directory;
using App.Services.Notifications;
using App.Services.Registrations;
using App.Services.Security;
using App.Services.Users;
using App.Web.Api.Models;
using App.Web.Areas.Admin.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class ContactsApiController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly IAuditTrailService _auditService;
        private readonly IWorkContext _workContext;
        private readonly IContactService _contactService;
        private readonly ICountryService _countryService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

		public ContactsApiController(
            IRegistrationService registrationService,
            IAuditTrailService auditService,
            IWorkContext workContext,
            IContactService contactService,
            ICountryService countryService,
            IEmailService emailService,
            IUserService userService,
            IRoleService roleService)
        {
            _registrationService = registrationService;
            _auditService = auditService;
            _workContext = workContext;
            _contactService = contactService;
            _countryService = countryService;
            _emailService = emailService;
            _userService = userService;
            _roleService = roleService;
		}

		private async Task<string> GenerateRandomPassword(int length = 8)
		{
			const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
			var random = new Random();
			return new string(Enumerable.Repeat(validChars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		//Get all contacts
		[HttpGet("getallcontact")]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _contactService.GetAllAsync();
            return Ok(contacts);
		}

		// GET: api/admin/contacts/by-registration/{registrationId}
		[HttpGet("by-registration/{registrationId}")]
        public async Task<IActionResult> GetByRegistration(int registrationId)
        {
            var contacts = await _registrationService.GetContactsByRegistrationIdAsync(registrationId);
            return Ok(contacts);
        }

        // GET: api/admin/contacts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contacts = await _registrationService.GetContactsByRegistrationIdAsync(0); // get all
            var entity = contacts.FirstOrDefault(c => c.Id == id);
            if (entity == null) return NotFound();

            return Ok(entity);
        }

        // POST: api/admin/contacts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContactModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new Contact
            {
                RegistrationId = model.RegistrationId,
                ContactTypeId = model.ContactTypeId,
                JobTitle = model.JobTitle,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                ContactPhone = model.ContactPhone,
                BusinessPhone = model.BusinessPhone,
                Email = model.Email,
                NationalityCountryId = model.NationalityCountryId,
                CreatedOnUtc = DateTime.UtcNow
            };

           await _registrationService.AddContactAsync(model.RegistrationId, entity);

            var password = await GenerateRandomPassword(10);
			var user = new AppUser
            {
                Username = entity.Email,
                Email = entity.Email,
                IsActive = true,
                CreatedOnUtc = DateTime.UtcNow,
                RegistrationId = entity.RegistrationId
            };

            var insUser = await _userService.InsertAsync(user, password);
            var role = await _roleService.GetAllAsync();
            var Admin = role.FirstOrDefault(r => r.SystemName == "Admin" || r.Name == "Admin");

            if (Admin != null)
            {
                await _roleService.AddUserToRoleAsync(insUser.Id, Admin.Id);
			}


			//Send email notification to the contact
			var emailBody = $"Dear {entity.FirstName} {entity.LastName},<br/><br/>" +
$"Your account has been created.<br/>" +
$"Username: {insUser.Username}<br/>" +
$"Password: {password}<br/><br/>" +
$"Please change your password after logging in.<br/><br/>" +
$"https://suptech.online/<br/><br/>" +
$"Best regards,<br/>" +
$"The Team";
			await _emailService.SendEmailAsync(
				to: user.Email,
				subject: "Your Account Credentials",
				body: emailBody);

			return Ok(new { success = true, contactId = entity.Id });
        }

        // PUT: api/admin/contacts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ContactModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var contacts = await _registrationService.GetContactsByRegistrationIdAsync(model.RegistrationId);
            var entity = contacts.FirstOrDefault(c => c.Id == id);
            if (entity == null) return NotFound();

            entity.ContactTypeId = model.ContactTypeId;
            entity.JobTitle = model.JobTitle;
            entity.FirstName = model.FirstName;
            entity.MiddleName = model.MiddleName;
            entity.LastName = model.LastName;
            entity.ContactPhone = model.ContactPhone;
            entity.BusinessPhone = model.BusinessPhone;
            entity.Email = model.Email;
            entity.NationalityCountryId = model.NationalityCountryId;

            await _registrationService.UpdateContactAsync(entity);

            await _auditService.LogUpdateAsync("Contact", model.Id.Value, 0, comment: "Contact updated");

            return Ok(new { success = true });
        }

        // DELETE: api/admin/contacts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int registrationId)
        {
            await _registrationService.RemoveContactAsync(id);

            return Ok(new { success = true });
        }
    }
}
