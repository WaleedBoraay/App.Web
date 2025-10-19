using App.Core.Domain.Registrations;
using App.Services;
using App.Services.Audit;
using App.Services.Directory;
using App.Services.Notifications;
using App.Services.Registrations;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Registrations;
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

		public ContactsApiController(
            IRegistrationService registrationService,
            IAuditTrailService auditService,
            IWorkContext workContext,
            IContactService contactService,
            ICountryService countryService,
            IEmailService emailService)
        {
            _registrationService = registrationService;
            _auditService = auditService;
            _workContext = workContext;
            _contactService = contactService;
            _countryService = countryService;
            _emailService = emailService;
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

            var entity = new FIContact
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

            await _auditService.LogUpdateAsync("FIContact", model.Id, 0, comment: "Contact updated");

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
