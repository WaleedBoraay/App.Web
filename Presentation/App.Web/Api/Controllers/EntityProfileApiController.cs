using App.Core.Domain.Institutions;
using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Services.Directory;
using App.Services.Institutions;
using App.Services.Registrations;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Institutes;
using App.Web.Areas.Admin.Models.Registrations;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.Controllers
{
    [Route("api/entityinstitutes")]
    [ApiController]
    public class EntityProfileApiController : ControllerBase
    {
        private readonly IInstitutionService _institutionService;
        private readonly ICountryService _countryService;
        private readonly IBranchService _branchService;
        private readonly IRegistrationService _registrationService;
        private readonly IDocumentService _documentService;

        public EntityProfileApiController(
            IInstitutionService institutionService,
            ICountryService countryService,
            IBranchService branchService,
            IRegistrationService registrationService,
            IDocumentService documentService)
        {
            _institutionService = institutionService;
            _countryService = countryService;
            _branchService = branchService;
            _registrationService = registrationService;
            _documentService = documentService;
        }

        // GET: api/admin/institutes
        [HttpGet]
        public async Task<IActionResult> GetAll(string? searchName, string? licenseNumber, int? countryId)
        {
            var countries = await _countryService.GetAllAsync();

            var searchResult = await _institutionService.SearchAsync(
                name: searchName,
                licenseNumber: licenseNumber,
                countryId: countryId,
                pageIndex: 0,
                pageSize: 50
            );

            var results = searchResult.Items.Select(i => new
            {
                i.Id,
                i.Name,
                i.LicenseNumber,
                i.BusinessPhoneNumber,
                i.Email,
                CountryName = countries.FirstOrDefault(c => c.Id == i.CountryId)?.Name,
                i.IsActive
            });

            return Ok(results);
        }

        // GET: api/admin/institutes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var institute = await _institutionService.GetByIdAsync(id);
            if (institute == null) return NotFound();

            var countries = await _countryService.GetAllAsync();
            var registration = (await _registrationService.GetAllAsync())
                                .FirstOrDefault(r => r.InstitutionId == institute.Id);
            var branches = (await _branchService.GetAllAsync())
                                .Where(b => b.InstitutionId == institute.Id);

            var documents = await _documentService.GetDocumentsByInstituteIdAsync(id);

            return Ok(new
            {
                institute.Id,
                institute.Name,
                institute.LicenseNumber,
                institute.Email,
                institute.BusinessPhoneNumber,
                institute.CountryId,
                CountryName = countries.FirstOrDefault(c => c.Id == institute.CountryId)?.Name,
                institute.IsActive,
                Registration = registration,
                Branches = branches,
                Documents = documents
            });
        }

        // POST: api/admin/institutes
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InstituteEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var institute = new Institution
            {
                Name = model.Name,
                LicenseNumber = model.LicenseNumber,
                LicenseSectorId = model.LicenseSectorId,
                FinancialDomainId = model.FinancialDomainId,
                LicenseIssueDate = model.LicenseIssueDate,
                LicenseExpiryDate = model.LicenseExpiryDate,
                CountryId = model.CountryId,
                Address = model.Address,
                IsActive = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _institutionService.InsertAsync(institute);

            return Ok(new { success = true, instituteId = institute.Id });
        }

        // PUT: api/admin/institutes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InstituteEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = await _institutionService.GetByIdAsync(id);
            if (entity == null) return NotFound();

            entity.Name = model.Name;
            entity.BusinessPhoneNumber = model.BusinessPhoneNumber;
            entity.Email = model.Email;
            entity.LicenseNumber = model.LicenseNumber;
            entity.LicenseSectorId = model.LicenseSectorId;
            entity.FinancialDomainId = model.FinancialDomainId;
            entity.LicenseIssueDate = model.LicenseIssueDate;
            entity.LicenseExpiryDate = model.LicenseExpiryDate;
            entity.CountryId = model.CountryId;
            entity.Address = model.Address;
            entity.IsActive = model.IsActive;
            entity.UpdatedOnUtc = DateTime.UtcNow;

            await _institutionService.UpdateAsync(entity);

            return Ok(new { success = true });
        }

        // DELETE: api/admin/institutes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _institutionService.GetByIdAsync(id);
            if (entity == null) return NotFound();

            await _institutionService.DeleteAsync(id);
            return Ok(new { success = true });
        }

        // POST: api/admin/institutes/{id}/upload-document
        [HttpPost("{id}/upload-document")]
        public async Task<IActionResult> UploadDocument(int id, IFormFile file, DocumentType documentType = DocumentType.License)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file selected" });

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadsPath, uniqueName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var document = new Document
            {
                FilePath = $"/uploads/{uniqueName}",
                UploadedOnUtc = DateTime.UtcNow,
                DocumentType = documentType
            };

            await _documentService.AddDocumentToInstituteAsync(id, document);

            return Ok(new
            {
                success = true,
                filePath = document.FilePath,
                uploadedOn = document.UploadedOnUtc,
                documentType = document.DocumentType.ToString()
            });
        }

        // 🔹 Branch APIs

        [HttpPost("{id}/branches")]
        public async Task<IActionResult> CreateBranch(int id, [FromBody] BranchEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var branch = new Branch
            {
                InstitutionId = id,
                Name = model.Name,
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email,
                CountryId = model.CountryId,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _branchService.InsertAsync(branch);
            return Ok(new { success = true, branchId = branch.Id });
        }

        [HttpPut("branches/{branchId}")]
        public async Task<IActionResult> UpdateBranch(int branchId, [FromBody] BranchEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var branch = await _branchService.GetByIdAsync(branchId);
            if (branch == null) return NotFound();

            branch.Name = model.Name;
            branch.Address = model.Address;
            branch.Phone = model.Phone;
            branch.Email = model.Email;
            branch.CountryId = model.CountryId;
            branch.UpdatedOnUtc = DateTime.UtcNow;

            await _branchService.UpdateAsync(branch);
            return Ok(new { success = true });
        }

        [HttpDelete("branches/{branchId}")]
        public async Task<IActionResult> DeleteBranch(int branchId)
        {
            var branch = await _branchService.GetByIdAsync(branchId);
            if (branch == null) return NotFound();

            await _branchService.DeleteAsync(branchId);
            return Ok(new { success = true });
        }
    }
}
