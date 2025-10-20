using App.Core;
using App.Core.Domain.Institutions;
using App.Core.Domain.Notifications;
using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Core.Domain.Users;
using App.Services;
using App.Services.Audit;
using App.Services.Directory;
using App.Services.Institutions;
using App.Services.Notifications;
using App.Services.Registrations;
using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Models.Registrations;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph.Models;

namespace App.Web.Api.Controllers
{
    [Route("api/entityregistrations")]
    [ApiController]
    public class EntityRegistrationsApiController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly IAuditTrailService _auditService;
        private readonly INotificationService _notificationService;
        private readonly ICountryService _countryService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IWorkContext _workContext;
        private readonly IInstitutionService _institutionService;
        private readonly IDocumentService _documentService;
        private readonly IContactService _contactService;
        private readonly IEmailService _emailService;

		public EntityRegistrationsApiController(
            IRegistrationService registrationService,
            IAuditTrailService auditService,
            INotificationService notificationService,
            ICountryService countryService,
            IUserService userService,
            IRoleService roleService,
            IWorkContext workContext,
            IInstitutionService institutionService,
            IDocumentService documentService,
            IContactService contactService,
            IEmailService emailService)
        {
            _registrationService = registrationService;
            _auditService = auditService;
            _notificationService = notificationService;
            _countryService = countryService;
            _userService = userService;
            _roleService = roleService;
            _workContext = workContext;
            _institutionService = institutionService;
            _documentService = documentService;
            _contactService = contactService;
            _emailService = emailService;
        }

		private async Task<string> GenerateRandomPassword(int length = 8)
		{
			const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
			var random = new Random();
			return new string(Enumerable.Repeat(validChars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}
		[HttpGet]
        public async Task<IActionResult> GetAllRegistrations(int userId)
        {
            var roles = await _roleService.GetRolesByUserIdAsync(userId);

            bool isMaker = roles.Any(r =>
                r.SystemName.Equals("Maker", StringComparison.OrdinalIgnoreCase) ||
                r.Name.Equals("Maker", StringComparison.OrdinalIgnoreCase));

            bool isChecker = roles.Any(r =>
                r.SystemName.Equals("Checker", StringComparison.OrdinalIgnoreCase) ||
                r.Name.Equals("Checker", StringComparison.OrdinalIgnoreCase));

            var allRegs = await _registrationService.GetAllAsync();

            var visibleRegs = allRegs.Where(r =>
                (isMaker && (r.StatusId == (int)RegistrationStatus.Draft ||
                             r.StatusId == (int)RegistrationStatus.ReturnedForEdit)) ||
                (isChecker && r.StatusId == (int)RegistrationStatus.Submitted)

            ).ToList();

			var model = new List<object>();
			foreach (var r in visibleRegs)
            {
                var createdByUser = await _userService.GetByIdAsync(r.CreatedByUserId);
                var contacts = await _registrationService.GetContactsByRegistrationIdAsync(r.Id);
                var regDocuments = await _documentService.GetRegistrationDocumentByIdAsync(r.Id);
				var statusLogs = await _registrationService.GetStatusHistoryAsync(r.Id);
                var country = await _countryService.GetByIdAsync(r.CountryId);

                model.Add(new
                {
                    r.Id,
                    r.InstitutionId,
                    r.InstitutionName,
                    r.LicenseNumber,
                    r.LicenseSectorId,
                    r.FinancialDomainId,
                    r.StatusId,
                    Status = ((RegistrationStatus)r.StatusId).ToString(),
                    CreatedOnUtc = r.CreatedOnUtc ?? DateTime.UtcNow,
                    CreatedByUserName = createdByUser?.Username ?? "Unknown",
                    Contacts = contacts ?? null,
                    Documents = regDocuments ?? null,
                    StatusLogs = statusLogs ?? null,
                    Country = country?.Name ?? "Unknown"
                });
            }

            return Ok(model);
        }

        [HttpGet("GetRegistrationById")]
        public async Task<IActionResult> GetRegistrationById(int userId)
        {
            var reg = await _registrationService.GetByIdAsync(userId);
            if (reg == null) return NotFound();

            var country = await _countryService.GetByIdAsync(reg.CountryId);
            var user = await _userService.GetByIdAsync(reg.CreatedByUserId);

            var model = new
            {
                reg.Id,
                reg.InstitutionId,
                reg.InstitutionName,
                reg.LicenseNumber,
                reg.LicenseSectorId,
                reg.IssueDate,
                reg.ExpiryDate,
                reg.StatusId,
                CountryName = country?.Name,
                CreatedByUserName = user?.Username,
                IsActive = user?.IsActive ?? false,
                Contacts = await _registrationService.GetContactsByRegistrationIdAsync(userId),
                Documents = await _registrationService.GetDocumentsByRegistrationIdAsync(userId),
                StatusLogs = await _registrationService.GetStatusHistoryAsync(userId)
            };

            return Ok(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRegistration([FromBody] RegistrationModel model ,int userId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

			var entity = new Registration
            {
                InstitutionName = model.InstitutionName,
                LicenseNumber = model.LicenseNumber,
                LicenseSectorId = model.LicenseSectorId,
                FinancialDomainId = model.FinancialDomainId,
                IssueDate = model.IssueDate,
                ExpiryDate = model.ExpiryDate,
                CountryId = model.CountryId,
                Address = model.Address,
                CreatedByUserId = userId,
                CreatedOnUtc = DateTime.UtcNow,
                Status = RegistrationStatus.Draft,
                StatusId = (int)RegistrationStatus.Draft
            };
            var reg = await _registrationService.InsertAsync(entity);
            //insert Contacts
            if (model.Contacts != null && model.Contacts.Any())
            {
                foreach (var contactModel in model.Contacts)
                {
                    var contact = new FIContact
                    {
                        RegistrationId = reg.Id,
                        FirstName = contactModel.FirstName,
                        MiddleName = contactModel.MiddleName,
                        LastName = contactModel.LastName,
                        NationalityCountryId = contactModel.NationalityCountryId,
                        ContactTypeId = contactModel.ContactTypeId,
                        JobTitle = contactModel.JobTitle,
                        ContactPhone = contactModel.ContactPhone,
                        BusinessPhone = contactModel.BusinessPhone,
                        Email = contactModel.Email,
                        CreatedOnUtc = DateTime.UtcNow
                        
					};
                    await _registrationService.AddContactAsync(reg.Id,contact);
                }
                if (model.Documents != null && model.Documents.Any())
                {
                    foreach (var docModel in model.Documents)
                    {
                        var doc = new FIDocument
                        {
                            DocumentTypeId = docModel.DocumentTypeId,
                            FilePath = docModel.FilePath,
                            UploadedOnUtc = DateTime.UtcNow
                        };  
						await _registrationService.AddDocumentAsync(reg.Id, doc);
                        var regDoc = new RegistrationDocument
						{
                            RegistrationId = reg.Id,
                            DocumentId = doc.Id
                        };
						await _documentService.InsertAsync(regDoc);
					}
				}
			}
			return Ok(new { success = true, registrationId = entity.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegistration(int id, [FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var reg = await _registrationService.GetByIdAsync(id);
            if (reg == null) return NotFound();

            reg.InstitutionName = model.InstitutionName;
            reg.LicenseNumber = model.LicenseNumber;
            reg.LicenseSectorId = model.LicenseSectorId;
            reg.FinancialDomainId = model.FinancialDomainId;
            reg.IssueDate = model.IssueDate;
            reg.ExpiryDate = model.ExpiryDate;
            reg.CountryId = model.CountryId;
            reg.Address = model.Address;
            reg.CreatedOnUtc = DateTime.UtcNow;

			await _registrationService.UpdateAsync(reg);
            await _auditService.LogUpdateAsync("Registration", reg.Id, reg.CreatedByUserId, "Registration edited");

            return Ok(new { success = true });
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitRegistration(int id, int userId)
        {
            await HandleStatusChange(id, RegistrationStatus.Submitted, userId, null);
            return Ok(new { success = true });
        }

        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateRegistration(int id, int userId)
        {
            var roles = await _roleService.GetRolesByUserIdAsync(userId);
            var user = await _userService.GetByIdAsync(userId);

            var reg = await _registrationService.GetByIdAsync(id);
            var contacts =  await _registrationService.GetContactsByRegistrationIdAsync(id);

            var regDocuments = await _documentService.GetRegistrationDocumentByIdAsync(id);

            var docement = await _documentService.GetByIdAsync(id); 
			var password = await GenerateRandomPassword(12);
			//create user for each contact
			foreach (var contact in contacts)
            {
				var users = new AppUser
				{
					Username = contact.FirstName,
					Email = contact.Email,
					IsActive = true,
					CreatedOnUtc = DateTime.UtcNow,

				};
               var insUser = await _userService.InsertAsync(users, password, user.Id);

				//assign role to user
				var role = await _roleService.GetAllAsync();
                var userRole = role.FirstOrDefault(r => r.SystemName == "Admin" || r.Name == "Admin");
                if (userRole != null)
                {
                    await _roleService.AddUserToRoleAsync(users.Id, userRole.Id);
				}
				//send email to user with credentials
				var emailBody = $"Dear {contact.FirstName} {contact.LastName},<br/><br/>" +
				$"Your account has been created.<br/>" +
				$"Username: {insUser.Username}<br/>" +
				$"Password: {password}<br/><br/>" +
				$"Please change your password after logging in.<br/><br/>" +
				$"https://suptech.online/<br/><br/>" +
				$"Best regards,<br/>" +
				$"The Team";
				await _emailService.SendEmailAsync(
                    to: users.Email,
                    subject: "Your Account Credentials",
                    body: emailBody);

			}

			if (roles.Any(r => r.SystemName == "Checker"))
            {
                await HandleStatusChange(id, RegistrationStatus.Approved, userId, null);
                return Ok(new { success = true });
            }

            return Forbid();
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveRegistration(int id, int userId)
        {
            await _registrationService.ApproveRegistrationAsync(id, userId);

            await _notificationService.SendAsync(
                registrationId: id,
                eventType: NotificationEvent.RegistrationApproved,
                triggeredByUserId: userId,
                recipientUserId: userId,
                channel: NotificationChannel.InApp,
                tokens: new Dictionary<string, string>
                {
                    ["RegistrationId"] = id.ToString(),
                    ["InstitutionName"] = (await _registrationService.GetByIdAsync(id))?.InstitutionName ?? "",
                    ["Status"] = RegistrationStatus.Approved.ToString(),
                });

            await _auditService.LogUpdateAsync("Registration", id, userId, "Registration approved");

            return Ok(new { success = true });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectRegistration(int id, [FromBody] string comment)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await _registrationService.RejectRegistrationAsync(id, currentUser.Id, comment);
            return Ok(new { success = true });
        }

        [HttpPost("{id}/return-for-edit")]
        public async Task<IActionResult> ReturnRegistrationForEdit(int id, [FromBody] string remarks, int userId)
        {
            await HandleStatusChange(id, RegistrationStatus.ReturnedForEdit, userId, remarks);
            await _auditService.LogUpdateAsync("Registration", id, userId, "Registration returned for edit",
                comment: remarks ?? "No remarks provided");
			return Ok(new { success = true });
        }

        private async Task HandleStatusChange(int regId, RegistrationStatus newStatus, int triggeredByUserId, string remarks)
        {
            var reg = await _registrationService.GetByIdAsync(regId);
            if (reg == null) return;

            reg.StatusId = (int)newStatus;
            reg.Status = newStatus;
            await _registrationService.UpdateAsync(reg);

            var institution = new Institution
            {
                Name = reg.InstitutionName,
                LicenseNumber = reg.LicenseNumber,
                LicenseSectorId = reg.LicenseSectorId,
                FinancialDomainId = reg.FinancialDomainId,
                CountryId = reg.CountryId,
                Address = reg.Address,
                IsActive = true
            };
            if (newStatus == RegistrationStatus.Approved)
            {
                await _institutionService.InsertAsync(institution);
			}

			await _auditService.LogUpdateAsync("Registration", reg.Id, triggeredByUserId, newValue: newStatus.ToString(),
                comment: remarks ?? $"Status changed to {newStatus}");

            var user = await _userService.GetByIdAsync(reg.CreatedByUserId);

            if (newStatus == RegistrationStatus.Approved && user != null)
                await _userService.ActivateAsync(user.Id);

            var eventType = MapStatusToEvent(newStatus);

            if (user != null)
            {
                await _notificationService.SendAsync(
                    registrationId: reg.Id,
                    eventType: eventType,
                    triggeredByUserId: triggeredByUserId,
                    recipientUserId: user.Id,
                    channel: NotificationChannel.InApp,
                    tokens: new Dictionary<string, string>
                    {
                        ["RegistrationId"] = reg.Id.ToString(),
                        ["InstitutionName"] = reg.InstitutionName,
                        ["Status"] = newStatus.ToString(),
                        ["TriggeredBy"] = (await _userService.GetByIdAsync(triggeredByUserId)).Username
                    });
            }
        }

        private NotificationEvent MapStatusToEvent(RegistrationStatus status)
        {
            return status switch
            {
                RegistrationStatus.Submitted => NotificationEvent.RegistrationSubmitted,
                RegistrationStatus.Approved => NotificationEvent.RegistrationApproved,
                RegistrationStatus.Rejected => NotificationEvent.RegistrationRejected,
                RegistrationStatus.ReturnedForEdit => NotificationEvent.RegistrationReturnedForEdit,
                RegistrationStatus.FinalSubmission => NotificationEvent.RegistrationFinalSubmission,
                RegistrationStatus.Archived => NotificationEvent.RegistrationArchived,
                _ => NotificationEvent.InstitutionCreated
            };
        }
    }
}
