using App.Core;
using App.Core.Domain.Registrations;
using App.Services;
using App.Services.Audit;
using App.Services.Directory;
using App.Services.Registrations;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Registrations;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using static Pipelines.Sockets.Unofficial.SocketConnection;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactsController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly IAuditTrailService _auditService;
        private readonly IWorkContext _workContext;
        private readonly IContactService _contactService;
        private readonly ICountryService _countryService;

        public ContactsController(
            IRegistrationService registrationService,
            IAuditTrailService auditService,
            IWorkContext workContext,
            IContactService contactService,
            ICountryService countryService)
        {
            _registrationService = registrationService;
            _auditService = auditService;
            _workContext = workContext;
            _contactService = contactService;
            _countryService = countryService;
        }

        public async Task<IActionResult> Create(int registrationId)
        {
            var countries = await _countryService.GetAllAsync();

            // Fix for IDE0305: Use collection initializer for countries
            var model = new ContactModel
            {
                RegistrationId = registrationId,
                countries = countries.Select(c => new CountryModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ContactModel model)
        {
            if (!ModelState.IsValid) return View(model);

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
                CreatedOnUtc = System.DateTime.UtcNow
            };

            await _registrationService.AddContactAsync(model.RegistrationId, entity);
            await _auditService.LogCreateAsync("FIContact", entity.Id, 0, "Contact created");

            return RedirectToAction("Details", "Registrations", new { id = model.RegistrationId });
        }

        // GET: /Admin/Contacts/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var contacts = await _registrationService.GetContactsByRegistrationIdAsync(0); // get all
            var entity = contacts.FirstOrDefault(c => c.Id == id);
            if (entity == null) return NotFound();

            var countries = await _countryService.GetAllAsync();

            var model = new ContactModel
            {
                Id = entity.Id,
                RegistrationId = entity.RegistrationId,
                ContactTypeId = entity.ContactTypeId,
                JobTitle = entity.JobTitle,
                FirstName = entity.FirstName,
                MiddleName = entity.MiddleName,
                LastName = entity.LastName,
                ContactPhone = entity.ContactPhone,
                BusinessPhone = entity.BusinessPhone,
                Email = entity.Email,
                NationalityCountryId = entity.NationalityCountryId,
                CreatedOnUtc = entity.CreatedOnUtc,
                UpdatedOnUtc = entity.UpdatedOnUtc,
                countries = countries.Select(c => new CountryModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ContactModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var contacts = await _registrationService.GetContactsByRegistrationIdAsync(model.RegistrationId);
            var entity = contacts.FirstOrDefault(c => c.Id == model.Id);
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

            return RedirectToAction("Details", "Registrations", new { id = model.RegistrationId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int registrationId)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await _registrationService.RemoveContactAsync(id);
            await _auditService.LogDeleteAsync("FIContact", id, currentUser.Id, "Contact deleted");
            return RedirectToAction("Details", "Registrations", new { id = registrationId });
        }
    }
}
