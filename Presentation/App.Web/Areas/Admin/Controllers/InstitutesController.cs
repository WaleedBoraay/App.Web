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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InstitutesController : Controller
    {
        private readonly IInstitutionService _institutionService;
        private readonly ICountryService _countryService;
        private readonly IBranchService _branchService;
        private readonly IRegistrationService _registrationService;
        private readonly IDocumentService _documentService;

        public InstitutesController(
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

        public async Task<IActionResult> Index(string searchName, string licenseNumber, int? countryId)
        {
            var countries = await _countryService.GetAllAsync();

            var searchResult = await _institutionService.SearchAsync(
                name: searchName,
                licenseNumber: licenseNumber,
                countryId: countryId,
                pageIndex: 0,
                pageSize: 50
            );

            var model = new InstituteSearchModel
            {
                SearchName = searchName,
                LicenseNumber = licenseNumber,
                CountryId = countryId,
                AvailableCountries = countries.Select(c => new CountryModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList(),
                Results = searchResult.Items.Select(i => new InstituteListModel
                {
                    InstituteId = i.Id,
                    InstituteName = i.Name,
                    LicenseNumber = i.LicenseNumber,
                    BusinessPhoneNumber = i.BusinessPhoneNumber,
                    Email = i.Email,
                    CountryName = countries.FirstOrDefault(c => c.Id == i.CountryId)?.Name,
                    IsActive = i.IsActive
                }).ToList()
            };

            return View(model);
        }

        // GET: /Admin/Institutes/Create
        public async Task<IActionResult> Create()
        {
            var countries = await _countryService.GetAllAsync();
            var model = new InstituteEditModel
            {
                AvailableCountries = (await _countryService.GetAllAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToList(),

                AvailableLicenseSectors = Enum.GetValues(typeof(LicenseSector))
                    .Cast<LicenseSector>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString()
                    }).ToList(),

                AvailableFinancialDomains = Enum.GetValues(typeof(FinancialDomain))
                    .Cast<FinancialDomain>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString()
                    }).ToList()
            };

            return View(model);
        }

        // POST: /Admin/Institutes/Create
        [HttpPost]
        public async Task<IActionResult> Create(InstituteEditModel model, IFormFile LicenseFile, IFormFile DocumentFile)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableCountries = (await _countryService.GetAllAsync())
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
                return View(model);
            }

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

            // لو فيه LicenseFile
            if (LicenseFile != null && LicenseFile.Length > 0)
            {
                var doc = new Document
                {
                    FilePath = await SaveFileAsync(LicenseFile),
                    UploadedOnUtc = DateTime.UtcNow,
                    DocumentType = DocumentType.License
                };
                await _documentService.AddDocumentToInstituteAsync(institute.Id, doc);
            }

            if (DocumentFile != null && DocumentFile.Length > 0)
            {
                var doc = new Document
                {
                    FilePath = await SaveFileAsync(DocumentFile),
                    UploadedOnUtc = DateTime.UtcNow,
                    DocumentType = DocumentType.Document
                };
                await _documentService.AddDocumentToInstituteAsync(institute.Id, doc);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var institute = await _institutionService.GetByIdAsync(id);
            if (institute == null) return NotFound();

            var countries = await _countryService.GetAllAsync();
            var currentCountry = countries.FirstOrDefault(c => c.Id == institute.CountryId)
                                 ?? countries.FirstOrDefault(c => c.Id == 66);

            var registration = (await _registrationService.GetAllAsync())
                                .FirstOrDefault(r => r.InstitutionId == institute.Id);

            var branches = (await _branchService.GetAllAsync())
                                .Where(b => b.InstitutionId == institute.Id)
                                .Select(b => new BranchModel
                                {
                                    BranchId = b.Id,
                                    Name = b.Name,
                                    Address = b.Address,
                                    Phone = b.Phone,
                                    Email = b.Email,
                                    CountryName = countries.FirstOrDefault(c => c.Id == b.CountryId)?.Name
                                }).ToList();

            var documents = await _documentService.GetDocumentsByInstituteIdAsync(id);

            IList<ContactModel> contacts = new List<ContactModel>();
            if (registration != null)
            {
                // نفترض عندك service للـ Contacts
                var regContacts = await _registrationService.GetContactsByRegistrationIdAsync(registration.Id);
                contacts = regContacts.Select(c => new ContactModel
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
                    CreatedOnUtc = c.CreatedOnUtc,
                    UpdatedOnUtc = c.UpdatedOnUtc,
                    NationalityCountryId = c.NationalityCountryId,
                    countries = countries.Select(cc => new CountryModel
                    {
                        Id = cc.Id,
                        Name = cc.Name
                    }).ToList()
                }).ToList();
            }

            var model = new InstituteDetailsModel
            {
                InstituteId = institute.Id,
                InstituteName = institute.Name,
                LicenseNumber = institute.LicenseNumber,
                CountryName = currentCountry?.Name ?? "",
                IsActive = institute.IsActive,
                Email = institute.Email,
                BusinessPhoneNumber = institute.BusinessPhoneNumber,
                Registration = registration == null ? null : new RegistrationModel
                {
                    InstitutionId = registration.Id,
                    LicenseNumber = registration.LicenseNumber,
                    Status = registration.Status,
                    IssueDate = registration.IssueDate,
                    ExpiryDate = registration.ExpiryDate
                },
                Branches = branches,
                Documents = documents.Select(d => new DocumentModel
                {
                    DocumentTypeId = d.DocumentTypeId,
                    DocumentType = d.DocumentType,
                    FilePath = d.FilePath,
                    UploadedOnUtc = d.UploadedOnUtc
                }).ToList(),
                Contacts = contacts
            };

            return View(model);
        }


        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            // تأكد إن الفولدر موجود، ولو مش موجود اعمله
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var fullPath = Path.Combine(uploadsPath, uniqueName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{uniqueName}";
        }

        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            var doc = await _documentService.GetByIdAsync(documentId);
            if (doc == null || string.IsNullOrEmpty(doc.FilePath))
                return NotFound();

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var contentType = "application/octet-stream";
            var fileName = Path.GetFileName(fullPath);

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, contentType, fileName);
        }

        // GET: /Admin/Institutes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var institute = await _institutionService.GetByIdAsync(id);
            if (institute == null) return NotFound();

            var model = new InstituteEditModel
            {
                Id = institute.Id,
                Name = institute.Name,
                LicenseNumber = institute.LicenseNumber,
                BusinessPhoneNumber = institute.BusinessPhoneNumber,
                Email = institute.Email,
                LicenseSectorId = institute.LicenseSectorId,
                FinancialDomainId = institute.FinancialDomainId,
                LicenseIssueDate = institute.LicenseIssueDate,
                LicenseExpiryDate = institute.LicenseExpiryDate,
                CountryId = institute.CountryId,
                Address = institute.Address,
                IsActive = institute.IsActive,

                AvailableCountries = (await _countryService.GetAllAsync())
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name,
                        Selected = (c.Id == institute.CountryId)
                    }).ToList(),

                AvailableLicenseSectors = Enum.GetValues(typeof(LicenseSector))
                    .Cast<LicenseSector>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString(),
                        Selected = (institute.LicenseSectorId == (int)e)
                    }).ToList(),

                AvailableFinancialDomains = Enum.GetValues(typeof(FinancialDomain))
                    .Cast<FinancialDomain>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString(),
                        Selected = (institute.FinancialDomainId == (int)e)
                    }).ToList()
            };

            return View(model);
        }

        // POST: /Admin/Institutes/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(InstituteEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var entity = await _institutionService.GetByIdAsync(model.Id);
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

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Institutes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _institutionService.GetByIdAsync(id);
            if (entity == null) return NotFound();

            return View(new InstituteEditModel
            {
                Id = entity.Id,
                Name = entity.Name,
                LicenseNumber = entity.LicenseNumber
            });
        }

        // POST: /Admin/Institutes/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _institutionService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UploadInstituteDocument(int instituteId, IFormFile file, DocumentType documentType = DocumentType.License)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file selected" });

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

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

            await _documentService.AddDocumentToInstituteAsync(instituteId, document);

            return Json(new
            {
                success = true,
                message = "Document uploaded successfully",
                filePath = document.FilePath,
                uploadedOn = document.UploadedOnUtc.ToString("g"),
                documentType = document.DocumentType.ToString()
            });
        }

        public async Task<IActionResult> CreateBranch(int institutionId)
        {
            var countries = await _countryService.GetAllAsync();
            var model = new BranchEditModel
            {
                InstitutionId = institutionId,
                AvailableCountries = countries.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBranch(BranchEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var branch = new Branch
            {
                InstitutionId = model.InstitutionId,
                Name = model.Name,
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email,
                CountryId = model.CountryId,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _branchService.InsertAsync(branch);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditBranch(int id)
        {
            var branch = await _branchService.GetByIdAsync(id);
            if (branch == null) return NotFound();

            var countries = await _countryService.GetAllAsync();
            var model = new BranchEditModel
            {
                Id = branch.Id,
                InstitutionId = branch.InstitutionId,
                Name = branch.Name,
                Address = branch.Address,
                Phone = branch.Phone,
                Email = branch.Email,
                CountryId = branch.CountryId,
                AvailableCountries = countries.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditBranch(BranchEditModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var branch = await _branchService.GetByIdAsync(model.Id);
            if (branch == null) return NotFound();

            branch.Name = model.Name;
            branch.Address = model.Address;
            branch.Phone = model.Phone;
            branch.Email = model.Email;
            branch.CountryId = model.CountryId;
            branch.UpdatedOnUtc = DateTime.UtcNow;

            await _branchService.UpdateAsync(branch);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            await _branchService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }

}
