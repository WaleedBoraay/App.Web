using App.Core;
using App.Core.Domain.Notifications;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegistrationsController : Controller
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
        private readonly IEmailService _emailService;

        public RegistrationsController(
            IRegistrationService registrationService,
            IAuditTrailService auditService,
            INotificationService notificationService,
            ICountryService countryService,
            IUserService userService,
            IRoleService roleService,
            IWorkContext workContext,
            IInstitutionService institutionService,
            IDocumentService documentService,
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
            _emailService = emailService;
        }

        //private method genrate random password
        private async Task<string> GenerateRandomPassword(int length = 8)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // GET: /Admin/Registrations
        public async Task<IActionResult> Index()
        {
            var user = await _workContext.GetCurrentUserAsync();
            var roles = await _roleService.GetRolesByUserIdAsync(user.Id);

            bool isMaker = roles.Any(r => r.SystemName == "Maker" || r.Name == "Maker");
            bool isChecker = roles.Any(r => r.SystemName == "Checker" || r.Name == "Checker");
            bool isRegulator = roles.Any(r => r.SystemName == "Regulator");
            bool isInspector = roles.Any(r => r.SystemName == "Inspector");

            var allRegs = await _registrationService.GetAllAsync();

            // ✅ فلترة حسب الـ role
            var visibleRegs = allRegs.Where(r =>
                (isMaker && r.StatusId == (int)RegistrationStatus.Draft) ||
                (isMaker && r.StatusId == (int)RegistrationStatus.ReturnedForEdit) ||
                (isChecker && r.StatusId == (int)RegistrationStatus.Submitted)
            ).ToList();

            var model = new RegistrationListModel
            {
                IsMaker = isMaker,
                IsChecker = isChecker,
                IsRegulator = isRegulator,
                IsInspector = isInspector,
                Registrations = visibleRegs.Select(r => new RegistrationModel
                {
                    Id = r.Id,
                    InstitutionId = r.InstitutionId,
                    InstitutionName = r.InstitutionName,
                    LicenseNumber = r.LicenseNumber,
                    StatusId = r.StatusId,
                    Status = (RegistrationStatus)r.StatusId,
                    CreatedOnUtc = r.CreatedOnUtc ?? DateTime.UtcNow,
                    CreatedByUserName = user.Username
                }).ToList()
            };

            return View(model);
        }


        public async Task<IActionResult> Create()
        {
            var countries = await _countryService.GetAllAsync();

            var model = new RegistrationModel
            {
                AvailableCountries = countries.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegistrationModel model, IFormFile LicenseFile, IFormFile DocumentFile)
        {
            if (!ModelState.IsValid)
            {
                // نرجع نفس الفورم مع القوائم
                model.AvailableCountries = (await _countryService.GetAllAsync())
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                return View(model);
            }

            var currentUser = await _workContext.GetCurrentUserAsync();

            // 1️⃣ إنشاء كيان التسجيل
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
                CreatedByUserId = currentUser.Id,
                CreatedOnUtc = DateTime.UtcNow,
                Status = RegistrationStatus.Draft,
                StatusId = (int)RegistrationStatus.Draft
            };

            await _registrationService.InsertAsync(registration);

            // 2️⃣ رفع الملفات
            if (LicenseFile != null && LicenseFile.Length > 0)
            {
                var doc = new FIDocument
                {
                    FilePath = await SaveFileAsync(LicenseFile),
                    UploadedOnUtc = DateTime.UtcNow,
                    DocumentType = DocumentType.License
                };
                await _documentService.AddDocumentToRegistrationAsync(registration.Id, doc);
            }

            if (DocumentFile != null && DocumentFile.Length > 0)
            {
                var doc = new FIDocument
                {
                    FilePath = await SaveFileAsync(DocumentFile),
                    UploadedOnUtc = DateTime.UtcNow,
                    DocumentType = DocumentType.Document
                };
                await _documentService.AddDocumentToRegistrationAsync(registration.Id, doc);
            }

            if (model.Contacts != null && model.Contacts.Any())
            {
                foreach (var contactModel in model.Contacts)
                {
                    var contact = new FIContact
                    {
                        RegistrationId = registration.Id,
                        ContactTypeId = contactModel.ContactTypeId,
                        JobTitle = contactModel.JobTitle,
                        FirstName = contactModel.FirstName,
                        MiddleName = contactModel.MiddleName,
                        LastName = contactModel.LastName,
                        ContactPhone = contactModel.ContactPhone,
                        BusinessPhone = contactModel.BusinessPhone,
                        Email = contactModel.Email,
                        NationalityCountryId = contactModel.NationalityCountryId,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await _registrationService.AddContactAsync(registration.Id, contact);
                    //now add contact to AppUser And Genrate password and send email to contact
                    var user = new AppUser
                    {
                        Username = contact.Email,
                        Email = contact.Email,
                        IsActive = true, // inactive until registration is approved
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    var password = await GenerateRandomPassword();
                    await _userService.InsertAsync(user, password);
                    //after contact is created send email to contact
                    var emailBody = $"Dear {contact.FirstName},<br/><br/>" +
                                    $"Your account has been created.<br/>" +
                                    $"Username: {user.Username}<br/>" +
                                    $"Password: {password}<br/><br/>" +
                                    $"Please change your password after logging in.<br/><br/>" +
                                    $"Best regards,<br/>" +
                                    $"The Team";
                    await _emailService.SendEmailAsync(contact.Email, "Account Created", emailBody);

                }

                // ممكن هنا تبعت نوتيفيكيشن أو إيميل لو حبيت
                await _auditService.LogCreateAsync("Registration", registration.Id, currentUser.Id, "Registration created");
                //send notification to the maker
                await _notificationService.SendAsync(
                    registrationId: registration.Id,
                    eventType: NotificationEvent.InstitutionCreated,
                    triggeredByUserId: currentUser.Id,
                    recipientUserId: currentUser.Id,
                    channel: NotificationChannel.InApp,
                    tokens: new Dictionary<string, string>
                    {
                        ["RegistrationId"] = registration.Id.ToString(),
                        ["InstitutionName"] = registration.InstitutionName,
                        ["Status"] = registration.Status.ToString(),
                        ["TriggeredBy"] = currentUser.Username
                    }
                );
                //send email to contact 

                return RedirectToAction(nameof(Index));
            }

            // If there are no contacts, redirect to details or index (choose appropriate action)
            return RedirectToAction(nameof(Details), new { id = registration.Id });
        }


        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uniqueName = $"{Guid.NewGuid()}_{file.FileName}";
            var path = Path.Combine("wwwroot", "uploads", uniqueName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{uniqueName}";
        }
        // GET: /Admin/Registrations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var reg = await _registrationService.GetByIdAsync(id);
            if (reg == null) return NotFound();

            var country = await _countryService.GetByIdAsync(reg.CountryId);
            var user = await _userService.GetByIdAsync(reg.CreatedByUserId);

            var currentUser = await _workContext.GetCurrentUserAsync();

            var model = new RegistrationModel
            {
                Id = reg.Id,
                InstitutionId = reg.InstitutionId,
                InstitutionName = reg.InstitutionName,
                LicenseNumber = reg.LicenseNumber,
                LicenseSectorId = reg.LicenseSectorId,
                IssueDate = reg.IssueDate,
                ExpiryDate = reg.ExpiryDate,
                StatusId = reg.StatusId,
                CountryName = country?.Name,
                CreatedByUserName = user?.Username,
                IsActive = user?.IsActive ?? false,
                Contacts = (await _registrationService.GetContactsByRegistrationIdAsync(id))
                    .Select(c => new ContactModel
                    {
                        Id = c.Id,
                        RegistrationId = c.RegistrationId,
                        ContactTypeId = c.ContactTypeId,
                        JobTitle = c.JobTitle,
                        FirstName = c.FirstName,
                        MiddleName = c.MiddleName,
                        LastName = c.LastName,
                        ContactPhone = c.ContactPhone,
                        BusinessPhone = c.BusinessPhone,
                        Email = c.Email,
                        NationalityCountryId = c.NationalityCountryId,
                        CreatedOnUtc = c.CreatedOnUtc,
                        UpdatedOnUtc = c.UpdatedOnUtc
                    }).ToList(),
                Documents = (await _registrationService.GetDocumentsByRegistrationIdAsync(id))
                    .Select(d => new DocumentModel
                    {
                        Id = d.Id,
                        DocumentType = d.DocumentType,
                        FilePath = d.FilePath,
                        UploadedOnUtc = d.UploadedOnUtc
                    }).ToList(),
                StatusLogs = (await _registrationService.GetStatusHistoryAsync(id))
                    .Select(h => new StatusLogModel
                    {
                        Id = h.Id,
                        RegistrationId = h.RegistrationId,
                        RegistrationStatus = h.RegistrationStatus,
                        ValidationStatus = h.ValidationStatus,
                        ApprovalStatus = h.ApprovalStatus,
                        AuditStatus = h.AuditStatus,
                        PerformedBy = h.PerformedBy,
                        ActionDateUtc = h.ActionDateUtc,
                        Remarks = h.Remarks
                    }).ToList()
            };

            return View(model);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var reg = await _registrationService.GetByIdAsync(id);
            if (reg == null) return NotFound();

            var countries = await _countryService.GetAllAsync();

            var model = new RegistrationModel
            {
                Id = reg.Id,
                InstitutionId = reg.InstitutionId,
                InstitutionName = reg.InstitutionName,
                LicenseNumber = reg.LicenseNumber,
                LicenseSectorId = reg.LicenseSectorId,
                FinancialDomainId = reg.FinancialDomainId,
                IssueDate = reg.IssueDate,
                ExpiryDate = reg.ExpiryDate,
                CountryId = reg.CountryId,
                Address = reg.Address,
                StatusId = reg.StatusId,
                Status = (RegistrationStatus)reg.StatusId,
                AvailableCountries = countries.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == reg.CountryId
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                var countries = await _countryService.GetAllAsync();
                model.AvailableCountries = countries.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == model.CountryId
                }).ToList();
                return View(model);
            }

            var reg = await _registrationService.GetByIdAsync(model.Id);
            if (reg == null) return NotFound();

            reg.InstitutionName = model.InstitutionName;
            reg.LicenseNumber = model.LicenseNumber;
            reg.LicenseSectorId = model.LicenseSectorId;
            reg.FinancialDomainId = model.FinancialDomainId;
            reg.IssueDate = model.IssueDate;
            reg.ExpiryDate = model.ExpiryDate;
            reg.CountryId = model.CountryId;
            reg.Address = model.Address;

            await _registrationService.UpdateAsync(reg);

            await _auditService.LogUpdateAsync("Registration", reg.Id, reg.CreatedByUserId, "Registration edited");

            return RedirectToAction(nameof(Details), new { id = reg.Id });
        }

        [HttpPost]
        public async Task<IActionResult> UploadRegistrationDocument(int registrationId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "No file selected";
                return RedirectToAction("Details", new { id = registrationId });
            }

            var uniqueName = $"{Guid.NewGuid()}_{file.FileName}";
            var path = Path.Combine("wwwroot", "uploads", uniqueName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new FIDocument
            {
                FilePath = $"/uploads/{uniqueName}",
                UploadedOnUtc = DateTime.UtcNow,
                DocumentType = DocumentType.Document
            };

            await _documentService.AddDocumentToRegistrationAsync(registrationId, document);

            TempData["Success"] = "Document uploaded successfully";
            return RedirectToAction("Details", new { id = registrationId });
        }

        // Workflow Actions

        [HttpPost]
        public async Task<IActionResult> Submit(int id, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await HandleStatusChange(id, RegistrationStatus.Submitted, currentUser.Id, remarks);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Validate(int id, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            var roles = await _roleService.GetRolesByUserIdAsync(currentUser.Id);

            if (roles.Any(r => r.SystemName == "Checker"))
            {
                await HandleStatusChange(id, RegistrationStatus.Approved, currentUser.Id, remarks);
            }
            else
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await _registrationService.ApproveRegistrationAsync(id, currentUser.Id);
            //Get maker user role 
            var roles = await _roleService.GetRolesByUserIdAsync(currentUser.Id);
            // لو عايزين نبعت notification للـ Maker
            var makerRole = roles.FirstOrDefault(r => r.SystemName == "Maker");
            // ممكن بعد كده نعمل notification للـ Maker
            await _notificationService.SendAsync(
                registrationId: id,
                eventType: NotificationEvent.RegistrationApproved,
                triggeredByUserId: currentUser.Id,
                recipientUserId: currentUser.Id,
                channel: NotificationChannel.InApp,
                tokens: new Dictionary<string, string>
                {
                    ["RegistrationId"] = id.ToString(),
                    ["InstitutionName"] = (await _registrationService.GetByIdAsync(id))?.InstitutionName ?? "",
                    ["Status"] = RegistrationStatus.Approved.ToString(),
                    ["TriggeredBy"] = currentUser.Username
                }
            );
            await _auditService.LogUpdateAsync("Registration", id, currentUser.Id, "Registration approved");

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string comment)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await _registrationService.RejectRegistrationAsync(id, currentUser.Id, comment);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ReturnForEdit(int id, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await HandleStatusChange(id, RegistrationStatus.ReturnedForEdit, currentUser.Id, remarks);
            return RedirectToAction(nameof(Details), new { id });
        }

        // Helper
        private async Task HandleStatusChange(int regId, RegistrationStatus newStatus, int triggeredByUserId, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            var reg = await _registrationService.GetByIdAsync(regId);
            if (reg == null) return;

            reg.StatusId = (int)newStatus;
            reg.Status = newStatus;
            await _registrationService.UpdateAsync(reg);

            // Audit
            await _auditService.LogUpdateAsync("Registration", reg.Id, currentUser.Id, newValue: newStatus.ToString(),
                comment: remarks ?? $"Status changed to {newStatus}");

            // Get registration owner
            var user = await _userService.GetByIdAsync(reg.CreatedByUserId);

            // If approved → activate user
            if (newStatus == RegistrationStatus.Approved && user != null)
                await _userService.ActivateAsync(user.Id);

            // Map to notification event
            var eventType = MapStatusToEvent(newStatus);

            // Send notification
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
                    }
                );
            }
        }

        private NotificationEvent MapStatusToEvent(RegistrationStatus status)
        {
            return status switch
            {
                RegistrationStatus.Submitted => NotificationEvent.RegistrationSubmitted,
                //RegistrationStatus.UnderReview => NotificationEvent.RegistrationValidated,
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
