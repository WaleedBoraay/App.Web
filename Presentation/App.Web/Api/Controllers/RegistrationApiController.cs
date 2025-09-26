using App.Core.Domain.Directory;
using App.Core.Domain.Institutions;
using App.Core.Domain.Notifications;
using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Core.Domain.Users;
using App.Services;
using App.Services.Directory;
using App.Services.Files;
using App.Services.Institutions;
using App.Services.Localization;
using App.Services.Notifications;
using App.Services.Registrations;
using App.Services.Security;
using App.Services.Users;
using App.Web.Api.DTOs;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationApiController : ControllerBase
{

    #region Dependencies
    private readonly IRegistrationService _registrationService;
    private readonly IInstitutionService _institutionService;
    private readonly INotificationService _notificationService;
    private readonly ICountryService _countryService;
    private readonly IDocumentService _documentService;
    private readonly IFileValidationService _fileValidation;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    #endregion

    #region Ctor
    public RegistrationApiController(
        IRegistrationService registrationService,
        IInstitutionService institutionService,
        INotificationService notificationService,
        ICountryService countryService,
        IDocumentService documentService,
        IFileValidationService fileValidation,
        IUserService userService,
        IRoleService roleService,
        ILocalizationService localizationService,
        IWorkContext workContext)
    {
        _registrationService = registrationService;
        _institutionService = institutionService;
        _notificationService = notificationService;
        _countryService = countryService;
        _documentService = documentService;
        _fileValidation = fileValidation;
        _userService = userService;
        _roleService = roleService;
        _localizationService = localizationService;
        _workContext = workContext;
    }
    #endregion

    [HttpPost("institute")]
    public async Task<IActionResult> SaveInstitutionAsync([FromForm] InstituteDto dto)
    {
        if (dto == null)
            throw new InvalidOperationException(
                await _localizationService.GetResourceAsync("Institution.data.is.required."));

        var institute = new Core.Domain.Institutions.Institution
        {
            Name = dto.Name,
            BusinessPhoneNumber = dto.BusinessPhoneNumber,
            Email = dto.Email,
            LicenseNumber = dto.LicenseNumber,
            LicenseSectorId = (int)dto.licenseSector,
            FinancialDomainId = (int)dto.financialDomain,
            LicenseIssueDate = dto.IssueDate ?? DateTime.UtcNow,
            LicenseExpiryDate = dto.ExpiryDate ?? DateTime.UtcNow.AddYears(1),
            CountryId = dto.CountryId,
            Address = dto.Address,
            IsActive = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _institutionService.InsertAsync(institute);

        if (dto.LicenseFile != null)
            await SaveDocumentsAsync(DocumentType.License, dto.LicenseFile, institute.Id);

        if (dto.DocumentFile != null)
            await SaveDocumentsAsync(DocumentType.Document, dto.DocumentFile, institute.Id);

        var username = institute.Name.Replace(" ", "").ToLowerInvariant();
        var email = institute.Email.Replace(" ", "").ToLowerInvariant();

        var user = new AppUser
        {
            Username = username,
            Email = email,
            InstitutionId = institute.Id,
            IsActive = false,
            CreatedOnUtc = DateTime.UtcNow
        };

        var createdUser = await _userService.InsertAsync(user, dto.Password);

        var reg = new Registration
        {
            InstitutionId = institute.Id,
            InstitutionName = institute.Name,
            LicenseNumber = institute.LicenseNumber,
            LicenseSectorId = institute.LicenseSectorId,
            FinancialDomainId = institute.FinancialDomainId,
            CountryId = institute.CountryId,
            Address = institute.Address,
            IssueDate = institute.LicenseIssueDate,
            ExpiryDate = institute.LicenseExpiryDate,
            CreatedOnUtc = DateTime.UtcNow,
            StatusId = (int)RegistrationStatus.Draft,
            Status = RegistrationStatus.Draft,
            CreatedByUserId = createdUser.Id,
        };

        await _registrationService.InsertAsync(reg);

        await _notificationService.SendAsync(
            registrationId: reg.Id,
            eventType: NotificationEvent.UserCreated,
            triggeredByUserId: 0,
            recipientUserId: createdUser.Id,
            channel: NotificationChannel.InApp,
            tokens: new Dictionary<string, string>
            {
                ["Username"] = user.Username,
                ["Email"] = user.Email,
                ["Password"] = dto.Password
            }
        );

        var adminUsers = await _userService.GetAllAsync();
        foreach (var admin in adminUsers.Where(a => a.InstitutionId == null && a.IsActive))
        {
            await _notificationService.SendAsync(
                reg.Id,
                NotificationEvent.InstitutionCreated,
                triggeredByUserId: createdUser.Id,
                recipientUserId: admin.Id,
                channel: NotificationChannel.InApp,
                tokens: new Dictionary<string, string>
                {
                    ["InstitutionName"] = institute.Name,
                    ["ContactEmail"] = user.Email,
                    ["Username"] = user.Username,
                    ["Password"] = dto.Password
                }
            );
        }

        foreach (var adminUser in adminUsers)
        {
            var roles = await _userService.GetRolesAsync(adminUser.Id);
            if (roles.Contains("Validator"))
            {
                await _notificationService.SendAsync(
                    reg.Id,
                    NotificationEvent.RegistrationSubmitted,
                    triggeredByUserId: createdUser.Id,
                    recipientUserId: adminUser.Id,
                    channel: NotificationChannel.InApp,
                    tokens: new Dictionary<string, string>
                    {
                        ["InstituteName"] = institute.Name,
                        ["InstituteId"] = institute.Id.ToString(),
                        ["CreatedUser"] = user.Username
                    }
                );
            }
        }

        return Ok(new
        {
            message = await _localizationService.GetResourceAsync("Institute registration flow completed"),
            institutionId = institute.Id,
            registrationId = reg.Id,
            userId = createdUser.Id,
            username = user.Username,
            email = user.Email,
            password = dto.Password,
            isActive = createdUser.IsActive
        });
    }

    //Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new
            {
                message = await _localizationService.GetResourceAsync("Invalid.login.credentials")
            });

        var user = await _userService.GetByUsernameAsync(dto.Username);
        if (user == null)
            return Unauthorized(new
            {
                message = await _localizationService.GetResourceAsync("User.not.found")
            });

        var isValid = await _userService.ValidatePasswordAsync(user, dto.Password);
        if (!isValid)
            return Unauthorized(new
            {
                message = await _localizationService.GetResourceAsync("Invalid.password")
            });
        var institution = await _institutionService.GetByIdAsync(user.InstitutionId ?? 0);
        var documents = await _documentService.GetDocumentsByInstituteIdAsync(user.InstitutionId ?? 0);

        return Ok(new
        {
            message = await _localizationService.GetResourceAsync("Login.successful"),
            username = user.Username,
            email = user.Email,
            isActive = user.IsActive,
            Institution = institution,
            Documents = documents
        });
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

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateRegistrationStatus(int id, [FromBody] int newStatusId)
    {
        var reg = await _registrationService.GetByIdAsync(id);
        if (reg == null)
            return NotFound(new { message = "Registration not found." });

        reg.StatusId = newStatusId;
        await _registrationService.UpdateAsync(reg);

        var user = await _userService.GetByIdAsync(reg.CreatedByUserId);

        var currentUser = await _workContext.GetCurrentUserAsync(); // ← هنا

        var eventType = MapStatusToEvent((RegistrationStatus)newStatusId);

        await _notificationService.SendAsync(
            registrationId: reg.Id,
            eventType: eventType,
            triggeredByUserId: currentUser.Id,   // ← الأدمن اللي عمل الحركة
            recipientUserId: user.Id,            // ← اليوزر اللي هيتبعتله
            channel: NotificationChannel.InApp,
            tokens: new Dictionary<string, string>
            {
                ["Username"] = user.Username,
                ["InstitutionName"] = reg.InstitutionName,
                ["Status"] = ((RegistrationStatus)newStatusId).ToString()
            }
        );

        return Ok(new { message = "Status updated successfully", newStatus = reg.StatusId });
    }


    [HttpPost("{id:int}/documents")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocuments(int institutionId, [FromForm] DocumentDto dto)
    {
        if (dto == null || dto.DocumentFile == null)
            return BadRequest(new { message = "No files uploaded." });

        if (dto.DocumentFile != null)
            await SaveDocumentsAsync(DocumentType.Document, dto.DocumentFile, institutionId);

        if (dto.LicenseFile != null)
            await SaveDocumentsAsync(DocumentType.License, dto.LicenseFile, institutionId);

        return Ok(new { message = "Files uploaded successfully." });
    }

    [HttpPost("{id:int}/contacts")]
    public async Task<IActionResult> AddContact(int id, [FromBody] ContactDto dto)
    {
        var contact = new FIContact
        {
            RegistrationId = id,
            ContactTypeId = (int)dto.ContactType,
            JobTitle = dto.JobTitle,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            Email = dto.Email,
            ContactPhone = dto.Phone,
            CivilId = dto.CivilId,
            PassportId = dto.PassportId
        };
        await _registrationService.AddContactAsync(id, contact);
        return Ok(new { message = "Contact added" });
    }

    [HttpPut("{id:int}/track")]
    public async Task<IActionResult> UpdateTrack(int id, [FromBody] TrackDto dto)
    {
        await UpdateTrackInternalAsync(id, dto);
        return Ok(new { message = "Track updated" });
    }

    [HttpPut("{id:int}/account")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountDto dto)
    {
        await UpdateAccountInternalAsync(id, dto);
        return Ok(new { message = "Account info saved" });
    }

    [HttpGet("{id:int}/status-history")]
    public async Task<IActionResult> StatusHistory(int id)
    {
        var logs = await _registrationService.GetStatusHistoryAsync(id);
        return Ok(logs);
    }

    // Notifications
    [HttpPost("{id:int}/notify")]
    public async Task<IActionResult> SendNotification(int id, [FromBody] NotificationDto dto)
    {
        await _notificationService.SendAsync(id, dto.EventType, dto.TriggeredByUserId, dto.RecipientUserId, dto.Channel, dto.Tokens);
        return Ok(new { message = "Notification sent" });
    }

    [HttpGet("{userId:int}/notifications")]
    public async Task<IActionResult> GetNotifications(int userId)
    {
        var notes = await _notificationService.GetLogsByUserAsync(userId);
        return Ok(notes);
    }

    [HttpPut("{userId:int}/notifications/{notificationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int userId, int notificationId)
    {
        await _notificationService.MarkAsReadAsync(notificationId, userId);
        return Ok(new { message = "Notification marked as read" });
    }

    // Countries
    [HttpPost("country")]
    public async Task<IActionResult> CreateCountry([FromBody] CountryDto dto)
    {
        var country = new Country { Name = dto.Name, TwoLetterIsoCode = dto.IsoCode };
        await _countryService.InsertAsync(country);
        return Ok(new { message = "Country created", countryId = country.Id });
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries()
    {
        var list = await _countryService.GetAllAsync();
        return Ok(list);
    }


    // Enums lookups
    [HttpGet("lookups/license-sectors")] public IActionResult GetLicenseSectors() => Ok(EnumToList<LicenseSector>());
    [HttpGet("lookups/license-types")] public IActionResult GetLicenseTypes() => Ok(EnumToList<LicenseType>());
    [HttpGet("lookups/financial-domains")] public IActionResult GetFinancialDomains() => Ok(EnumToList<FinancialDomain>());
    [HttpGet("lookups/statuses")] public IActionResult GetStatuses() => Ok(EnumToList<RegistrationStatus>());



    private async Task<Registration> SaveRegistrationAsync(InstituteRegistrationDto dto, int institutionId)
    {
        var reg = new Registration
        {
            InstitutionId = institutionId,
            InstitutionName = dto.InstitutionName,
            LicenseNumber = dto.LicenseNumber,
            LicenseTypeId = dto.LicenseTypeId,
            LicenseSectorId = dto.LicenseSectorId,
            FinancialDomainId = dto.FinancialDomainId,
            CountryId = dto.CountryId,
            Address = dto.Address,
            IssueDate = NormalizeDate(dto.IssueDate),
            ExpiryDate = NormalizeDate(dto.ExpiryDate),
            CreatedOnUtc = DateTime.UtcNow,
            StatusId = (int)RegistrationStatus.Draft,
            CreatedByUserId = dto.CreatedByUserId
        };
        await _registrationService.InsertAsync(reg);
        return reg;
    }

    private async Task SaveDocumentsAsync(DocumentType type, IFormFile file, int institutionId)
    {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fileupload");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Validate
        var isValid = await _fileValidation.ValidateAsync(file.FileName, file.Length);
        if (!isValid)
            throw new InvalidOperationException($"Invalid file: {file.FileName}");

        // Save physical file
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        // Save metadata
        var doc = new FIDocument
        {
            UploadedOnUtc = DateTime.UtcNow,
            DocumentType = type,
            FilePath = $"/fileupload/{uniqueFileName}"
        };

        await _documentService.InsertAsync(doc);
        await _documentService.InsertAsync(new InstituteDocument
        {
            DocumentId = doc.Id,
            InstituteId = institutionId
        });
    }

    private async Task SaveContactsAsync(InstituteRegistrationDto dto, int registrationId)
    {
        if (dto.Contacts == null || !dto.Contacts.Any())
            return;

        foreach (var c in dto.Contacts)
        {
            // 1) Save the Contact itself
            var contact = new FIContact
            {
                RegistrationId = registrationId,
                JobTitle = c.JobTitle,
                FirstName = c.FirstName,
                MiddleName = c.MiddleName,
                LastName = c.LastName,
                Email = c.Email,
                ContactPhone = c.ContactPhone,
                BusinessPhone = c.BusinessPhone,
                CivilId = c.CivilId,
                PassportId = c.PassportId,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _registrationService.AddContactAsync(registrationId, contact);
        }
    }

    private async Task UpdateTrackInternalAsync(int registrationId, TrackDto dto)
    {
        var reg = await _registrationService.GetByIdAsync(registrationId);
        if (reg == null) throw new InvalidOperationException("Registration not found.");
        reg.StatusId = (int)dto.Status;
        await _registrationService.UpdateAsync(reg);
    }

    private async Task UpdateAccountInternalAsync(int registrationId, AccountDto dto)
    {
        var reg = await _registrationService.GetByIdAsync(registrationId);
        if (reg == null) throw new InvalidOperationException("Registration not found.");
        reg.UpdatedByUserId = dto.UpdatedByUserId;
        await _registrationService.UpdateAsync(reg);
    }

    private static DateTime? NormalizeDate(DateTime? dt)
    {
        if (!dt.HasValue) return null;
        var min = new DateTime(1753, 1, 1);
        return dt.Value >= min ? dt : null;
    }

    private static IEnumerable<object> EnumToList<T>() where T : Enum =>
        System.Enum.GetValues(typeof(T)).Cast<T>().Select(x => new { id = Convert.ToInt32(x), name = x.ToString() }).ToList();

}
