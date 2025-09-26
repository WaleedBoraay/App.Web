using App.Core.Domain.Registrations;
using App.Services.Registrations;
using App.Services.Audit;
using App.Web.Areas.Admin.Models.Registrations;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DocumentsController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly IAuditTrailService _auditService;

        public DocumentsController(
            IRegistrationService registrationService,
            IAuditTrailService auditService)
        {
            _registrationService = registrationService;
            _auditService = auditService;
        }

        // GET: /Admin/Documents/Upload
        public IActionResult Upload(int registrationId)
        {
            var model = new DocumentModel { RegistrationId = registrationId };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(DocumentModel model, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "File is required.");
                return View(model);
            }

            var filePath = $"/uploads/{file.FileName}";

            var entity = new FIDocument
            {
                DocumentTypeId = (int)model.DocumentType,
                FilePath = filePath,
                UploadedOnUtc = System.DateTime.UtcNow
            };

            await _registrationService.AddDocumentAsync(model.RegistrationId, entity);
            await _auditService.LogCreateAsync("FIDocument", entity.Id, 0, "Document uploaded");

            return RedirectToAction("Details", "Registrations", new { id = model.RegistrationId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int registrationId)
        {
            await _registrationService.RemoveDocumentAsync(id);
            await _auditService.LogDeleteAsync("FIDocument", id, 0, "Document deleted");

            return RedirectToAction("Details", "Registrations", new { id = registrationId });
        }
    }
}
