using App.Core;
using App.Core.Domain.Notifications;
using App.Core.Domain.Registrations;
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

        public RegistrationsController(
            IRegistrationService registrationService,
            IAuditTrailService auditService,
            INotificationService notificationService,
            ICountryService countryService,
            IUserService userService,
            IRoleService roleService,
            IWorkContext workContext,
            IInstitutionService institutionService,
            IDocumentService documentService)
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
        }

        // GET: /Admin/Registrations
        public async Task<IActionResult> Index()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            var roles = await _roleService.GetByIdAsync(currentUser.Id);

            var regs = await _registrationService.GetAllAsync();

            // Filtering by Role
            //if (roles.Name == "Maker")
            //    regs = regs.Where(r => r.Status == RegistrationStatus.Draft && r.CreatedByUserId == currentUser.Id).ToList();

            //if (roles.Name == "Checker" || roles.Name == "Validator")
            //    regs = regs.Where(r => r.Status == RegistrationStatus.Submitted).ToList();

            //if (roles.Name == "Regulator")
            //    regs = regs.Where(r => r.Status == RegistrationStatus.UnderReview).ToList();

            //if (roles.Name == "Inspector")
            //    regs = regs.Where(r => r.Status == RegistrationStatus.Approved).ToList();

            var model = regs.Select(r => new RegistrationModel
            {
                Id = r.Id,
                InstitutionId = r.InstitutionId,
                InstitutionName = r.InstitutionName,
                LicenseNumber = r.LicenseNumber,
                LicenseSectorId = r.LicenseSectorId,
                IssueDate = r.IssueDate,
                ExpiryDate = r.ExpiryDate,
                StatusId = r.StatusId,
                CreatedByUserName = currentUser.Username,
                UpdatedByUserName = currentUser.Username,
                CreatedOnUtc = r.CreatedOnUtc ?? System.DateTime.UtcNow
            }).ToList();

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
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

            }
            var institutions = await _institutionService.GetAllAsync();
            var institution = institutions.FirstOrDefault(i => i.Id == model.InstitutionId);
            var currentUser = await _workContext.GetCurrentUserAsync();

            model.CreatedByUserName = currentUser.Username;
            model.UpdatedByUserName = currentUser.Username;
            var entity = new Registration
            {
                InstitutionId = model.InstitutionId,
                InstitutionName = institution.Name,
                LicenseNumber = model.LicenseNumber,
                LicenseSectorId = model.LicenseSectorId,
                IssueDate = model.IssueDate,
                ExpiryDate = model.ExpiryDate,
                CountryId = model.CountryId,
                CreatedByUserId = currentUser.Id,
                CreatedOnUtc = DateTime.UtcNow,
                StatusId = (int)RegistrationStatus.Draft,
                Status = RegistrationStatus.Draft
            };

            await _registrationService.InsertAsync(entity);

            if (LicenseFile != null && LicenseFile.Length > 0)
            {
                var doc = new FIDocument
                {
                    FilePath = await SaveFileAsync(LicenseFile),
                    UploadedOnUtc = DateTime.UtcNow,
                    DocumentType = DocumentType.License
                };
                await _documentService.AddDocumentToRegistrationAsync(entity.Id, doc);
            }

            if (DocumentFile != null && DocumentFile.Length > 0)
            {
                var doc = new FIDocument
                {
                    FilePath = await SaveFileAsync(DocumentFile),
                    UploadedOnUtc = DateTime.UtcNow,
                    DocumentType = DocumentType.Document
                };
                await _documentService.AddDocumentToRegistrationAsync(entity.Id, doc);
            }


            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Validate(int id, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            var roles = await _roleService.GetRolesByUserIdAsync(currentUser.Id);

            if (roles.Any(r => r.SystemName == "Checker"))
            {
                await HandleStatusChange(id, RegistrationStatus.UnderReview, currentUser.Id, remarks);
            }
            else if (roles.Any(r => r.SystemName == "Validator"))
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
        public async Task<IActionResult> Approve(int id, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await HandleStatusChange(id, RegistrationStatus.Approved, currentUser.Id, remarks);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await HandleStatusChange(id, RegistrationStatus.Rejected, currentUser.Id, remarks);
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> ReturnForEdit(int id, string remarks)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            await HandleStatusChange(id, RegistrationStatus.ReturnedForEdit, currentUser.Id, remarks);
            return RedirectToAction(nameof(Details), new { id });
        }

        // ================== Helper ==================
        private async Task HandleStatusChange(int regId, RegistrationStatus newStatus, int triggeredByUserId, string remarks)
        {
            var reg = await _registrationService.GetByIdAsync(regId);
            if (reg == null) return;

            reg.StatusId = (int)newStatus;
            reg.Status = newStatus;
            await _registrationService.UpdateAsync(reg);

            // Audit
            await _auditService.LogUpdateAsync("Registration", reg.Id, triggeredByUserId,
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
                RegistrationStatus.UnderReview => NotificationEvent.RegistrationValidated,
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
