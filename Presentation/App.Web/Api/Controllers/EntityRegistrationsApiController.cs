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
using App.Web.Api.Models;
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
			// 1️⃣ Get user roles
			var roles = await _roleService.GetRolesByUserIdAsync(userId);

			bool isMaker = roles.Any(r =>
				r.SystemName.Equals("Maker", StringComparison.OrdinalIgnoreCase) ||
				r.Name.Equals("Maker", StringComparison.OrdinalIgnoreCase));

			bool isChecker = roles.Any(r =>
				r.SystemName.Equals("Checker", StringComparison.OrdinalIgnoreCase) ||
				r.Name.Equals("Checker", StringComparison.OrdinalIgnoreCase));

			// 2️⃣ Get all registrations
			var allRegs = await _registrationService.GetAllAsync();

			// 3️⃣ Filter based on user role
			List<Registration>? visibleRegs = allRegs.Where(r =>
				(isMaker && (r.StatusId == (int)RegistrationStatus.Draft ||
							 r.StatusId == (int)RegistrationStatus.ReturnedForEdit)) ||
				(isChecker && r.StatusId == (int)RegistrationStatus.Submitted)
			).ToList();

			// 4️⃣ Build result list
			var result = new List<object>();

			foreach (var r in visibleRegs)
			{
				var createdByUser = await _userService.GetByIdAsync(r.CreatedByUserId);
				var country = await _countryService.GetByIdAsync(r.CountryId);
				var institution = await _institutionService.GetByIdAsync(r.InstitutionId);

				// 🟢 Contacts
				var contacts = await _registrationService.GetContactsByRegistrationIdAsync(r.Id);
				var contactModels = contacts.Select(c => new
				{
					c.Id,
					c.FirstName,
					c.MiddleName,
					c.LastName,
					FullName = $"{c.FirstName} {c.LastName}",
					c.JobTitle,
					c.Email,
					c.ContactPhone,
					c.BusinessPhone,
					c.ContactTypeId,
					c.CreatedOnUtc
				}).ToList();

				// 🟢 Documents
				var docs = await _registrationService.GetDocumentsByRegistrationIdAsync(r.Id);
				var docModels = docs.Select(d => new
				{
					d.Id,
					d.DocumentTypeId,
					DocumentType = d.DocumentType.ToString(),
					d.FilePath,
					d.UploadedOnUtc,
					d.ContactId,
					ContactName = contacts.FirstOrDefault(c => c.Id == d.ContactId) != null
						? $"{contacts.FirstOrDefault(c => c.Id == d.ContactId).FirstName} {contacts.FirstOrDefault(c => c.Id == d.ContactId).LastName}"
						: null
				}).ToList();

				var log = await _auditService.GetUserAuditTrailsByRegistrationIdAsync(r.Id);
				var Log = log.Select(l => new
				{
					l.Id,
					l.EntityId,
					l.Action,
					l.ChangedOnUtc,
					l.OldValue,
					l.NewValue,
					l.Comment
				}).ToList();

				// 🟢 Add model
				result.Add(new
				{
					r.Id,
					r.InstitutionId,
					InstitutionName = institution?.Name ?? r.InstitutionName,
					r.LicenseNumber,
					r.LicenseSectorId,
					LicenseSector = ((LicenseSector)r.LicenseSectorId).ToString(),
					r.FinancialDomainId,
					FinancialDomains = ((FinancialDomain)r.FinancialDomainId).ToString(),
					r.IssueDate,
					r.ExpiryDate,
					r.StatusId,
					//Get string representation of status
					Status = ((RegistrationStatus)r.StatusId).ToString(),
					r.Address,
					CountryId = r.CountryId,
					CountryName = (await _countryService.GetByIdAsync(r.CountryId)).Name,
					CreatedByUserName = createdByUser?.Username,
					CreatedByUserEmail = createdByUser?.Email,
					CreatedOn = r.CreatedOnUtc,
					Contacts = contactModels,
					Documents = docModels,
					Log = log
				});
			}

			return Ok(result);
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


		#region Upload Document
		[HttpPost("upload-document")]
		public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] int contactId, [FromForm] int documentTypeId)
		{
			if (file == null || file.Length == 0)
				return BadRequest(new { success = false, message = "No file uploaded." });

			var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
			if (!Directory.Exists(uploadsDir))
				Directory.CreateDirectory(uploadsDir);

			var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
			var filePath = Path.Combine(uploadsDir, uniqueFileName);
			var relativePath = $"/uploads/documents/{uniqueFileName}";

			await using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			var entity = new Document
			{
				FilePath = relativePath,
				UploadedOnUtc = DateTime.UtcNow,
				ContactId = contactId,
				DocumentTypeId = documentTypeId
			};

			await _documentService.InsertAsync(entity);

			await _auditService.LogCreateAsync("Document", entity.Id, 0, "Document uploaded before registration");

			return Ok(new
			{
				success = true,
				documentId = entity.Id,
				path = entity.FilePath,
				documentTypeId = entity.DocumentTypeId,
				message = "Document uploaded successfully."
			});
		}
		#endregion
		[HttpPost]
		[RequestSizeLimit(50_000_000)]
		public async Task<IActionResult> CreateRegistration([FromForm] RegistrationModel model, [FromQuery] int userId)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userService.GetByIdAsync(userId);

			var registration = new Registration
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

			await _registrationService.InsertAsync(registration);

			if (model.Contacts?.Any() == true)
			{
				foreach (var c in model.Contacts)
				{
					var contact = new Core.Domain.Registrations.Contact
					{
						RegistrationId = registration.Id,
						FirstName = c.FirstName,
						MiddleName = c.MiddleName,
						LastName = c.LastName,
						JobTitle = c.JobTitle,
						Email = c.Email,
						ContactPhone = c.ContactPhone,
						BusinessPhone = c.BusinessPhone,
						NationalityCountryId = c.NationalityCountryId,
						ContactTypeId = (int)c.ContactTypes,
						CreatedOnUtc = DateTime.UtcNow
					};
					await _registrationService.AddContactAsync(registration.Id, contact);
				}
			}

			var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
			if (!Directory.Exists(uploadsDir))
				Directory.CreateDirectory(uploadsDir);

			var docFiles = new[]
			{
	("LicenseFile", model.Documents?.FirstOrDefault()?.LicenseFile),
	("OtherDocument", model.Documents?.FirstOrDefault()?.OtherDocument),
	("PassportDocument", model.Documents?.FirstOrDefault()?.PassportDocument),
	("CivilIdDocument", model.Documents?.FirstOrDefault()?.CivilIdDocument)
};

			foreach (var (type, file) in docFiles)
			{

					var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
					var filePath = Path.Combine(uploadsDir, fileName);

					using (var stream = new FileStream(filePath, FileMode.Create))
						await file.CopyToAsync(stream);

					var contactRegi = await _registrationService.GetContactsByRegistrationIdAsync(registration.Id);

					int documentTypeId = type switch
					{
						"PassportDocument" => (int)DocumentType.Passport,
						"CivilIdDocument" => (int)DocumentType.CivilId,
						"LicenseFile" => (int)DocumentType.License,
						"OtherDocument" => (int)DocumentType.Document,
						_ => (int)DocumentType.Document
					};

					foreach (var contact in contactRegi)
					{
						var document = new Document
						{
							ContactId = contact.Id,
							DocumentTypeId = documentTypeId,
							FileName = fileName,
							FilePath = $"/uploads/documents/{fileName}",
							UploadedOnUtc = DateTime.UtcNow
						};

						await _registrationService.AddDocumentAsync(registration.Id, document);
					}
				
			}

			var emailBody = $@"
        Dear {user.Username},<br/><br/>
        Your registration has been created successfully with ID: <b>{registration.Id}</b>.<br/>
        <a href='https://suptech.online/'>https://suptech.online/</a><br/><br/>
        Best regards,<br/>The Team";

			await _emailService.SendEmailAsync(user.Email, "Registration Created", emailBody);

			return Ok(new
			{
				success = true,
				registrationId = registration.Id,
				message = "Registration created and documents uploaded successfully."
			});
		}





		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateRegistration(int id, [FromBody] RegistrationModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var reg = await _registrationService.GetByIdAsync(id);
			if (reg == null)
				return NotFound();

			reg.InstitutionName = model.InstitutionName;
			reg.LicenseNumber = model.LicenseNumber;
			reg.LicenseSectorId = model.LicenseSectorId;
			reg.FinancialDomainId = model.FinancialDomainId;
			reg.IssueDate = model.IssueDate;
			reg.ExpiryDate = model.ExpiryDate;
			reg.CountryId = model.CountryId;
			reg.Address = model.Address;

			await _registrationService.UpdateAsync(reg);

			var existingDocuments = await _registrationService.GetDocumentsByRegistrationIdAsync(reg.Id);
			var existingContacts = await _registrationService.GetContactsByRegistrationIdAsync(reg.Id);

			if (model.Contacts != null && model.Contacts.Any())
			{
				var contactIdsToKeep = model.Contacts
					.Where(c => c.Id > 0)
					.Select(c => c.Id)
					.ToList();

				foreach (var existingContact in existingContacts)
				{
					if (!contactIdsToKeep.Contains(existingContact.Id))
						await _contactService.DeleteAsync(existingContact.Id);
				}

				foreach (var contactModel in model.Contacts)
				{
					if (contactModel.Id > 0)
					{
						var existingContact = existingContacts.FirstOrDefault(c => c.Id == contactModel.Id);
						if (existingContact != null)
						{
							existingContact.FirstName = contactModel.FirstName;
							existingContact.MiddleName = contactModel.MiddleName;
							existingContact.LastName = contactModel.LastName;
							existingContact.NationalityCountryId = contactModel.NationalityCountryId;
							existingContact.ContactTypeId = contactModel.ContactTypeId;
							existingContact.JobTitle = contactModel.JobTitle;
							existingContact.ContactPhone = contactModel.ContactPhone;
							existingContact.BusinessPhone = contactModel.BusinessPhone;
							existingContact.Email = contactModel.Email;
							await _registrationService.UpdateContactAsync(existingContact);
						}
					}
					else
					{
						var newContact = new Core.Domain.Registrations.Contact
						{
							RegistrationId = reg.Id,
							FirstName = contactModel.FirstName,
							MiddleName = contactModel.MiddleName,
							LastName = contactModel.LastName,
							NationalityCountryId = contactModel.NationalityCountryId,
							JobTitle = contactModel.JobTitle,
							ContactPhone = contactModel.ContactPhone,
							BusinessPhone = contactModel.BusinessPhone,
							Email = contactModel.Email,
							CreatedOnUtc = DateTime.UtcNow,
							ContactTypeId = (int)contactModel.ContactTypes
						};
						await _registrationService.AddContactAsync(reg.Id, newContact);
					}
				}
			}

			if (model.Documents != null && model.Documents.Any())
			{
				var documentIdsToKeep = model.Documents
					.Where(d => d.Id > 0)
					.Select(d => d.Id)
					.ToList();

				foreach (var existingDoc in existingDocuments)
				{
					if (!documentIdsToKeep.Contains(existingDoc.Id))
						await _documentService.DeleteAsync(existingDoc.Id);
				}

				foreach (var docModel in model.Documents)
				{
					if (docModel.Id > 0)
					{
						var existingDoc = existingDocuments.FirstOrDefault(d => d.Id == docModel.Id);
						if (existingDoc != null)
						{
							existingDoc.DocumentTypeId = docModel.DocumentTypeId;
							existingDoc.FilePath = docModel.FilePath;
							existingDoc.ContactId = docModel.ContactId;
							existingDoc.UploadedOnUtc = DateTime.UtcNow;
							await _documentService.UpdateAsync(existingDoc);
						}
					}
					else
					{
						var newDoc = new Document
						{
							DocumentTypeId = docModel.DocumentTypeId,
							FilePath = docModel.FilePath,
							UploadedOnUtc = DateTime.UtcNow,
							ContactId = docModel.ContactId

						};
						await _registrationService.AddDocumentAsync(reg.Id, newDoc);

						var regDoc = new RegistrationDocument
						{
							RegistrationId = reg.Id,
							DocumentId = newDoc.Id
						};
						await _documentService.InsertAsync(regDoc);
					}
				}
			}

			await _auditService.LogUpdateAsync("Registration", reg.Id, reg.CreatedByUserId, "Registration updated");

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
					RegistrationId = reg.Id

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

			//send email for user 

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

		[HttpPost("return-for-edit/{id}")]
		public async Task<IActionResult> ReturnRegistrationForEdit(int id, [FromBody] ReturnForEditModel model, [FromQuery] int userId)
		{
			if (model == null)
				return BadRequest("Invalid payload.");

			// 🟢 1. Update status to ReturnedForEdit
			await HandleStatusChange(id, RegistrationStatus.ReturnedForEdit, userId, model.Comment);

			// 🟢 2. Send notification to the maker
			var registration = await _registrationService.GetByIdAsync(id);

			await _notificationService.SendAsync(
				registrationId: id,
				eventType: NotificationEvent.RegistrationReturnedForEdit,
				triggeredByUserId: userId,
				recipientUserId: registration.CreatedByUserId,
				channel: NotificationChannel.InApp,
				tokens: new Dictionary<string, string>
				{
					["RegistrationId"] = id.ToString(),
					["InstitutionName"] = registration?.InstitutionName ?? "",
					["Status"] = RegistrationStatus.ReturnedForEdit.ToString(),
					["Remarks"] = model.Comment ?? "No remarks provided",
					["Sections"] = string.Join(", ", model.Sections ?? new List<string>())
				});

			return Ok(new
			{
				success = true,
				registrationId = id,
				status = RegistrationStatus.ReturnedForEdit.ToString(),
				sections = model.Sections,
				comment = model.Comment
			});
		}


		private async Task HandleStatusChange(int regId, RegistrationStatus newStatus, int triggeredByUserId, string remarks)
        {
            var reg = await _registrationService.GetByIdAsync(regId);
			var statusBeforeChange = reg.Status;
			var statusIdBeforeChange = reg.StatusId;
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
				oldValue: ((RegistrationStatus)statusIdBeforeChange).ToString(),
				comment: remarks);

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
