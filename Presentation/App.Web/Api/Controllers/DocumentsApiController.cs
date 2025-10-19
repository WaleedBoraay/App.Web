using App.Core.Domain.Registrations;
using App.Services.Audit;
using App.Services.Registrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsApiController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly IAuditTrailService _auditService;

        public DocumentsApiController(
            IRegistrationService registrationService,
            IAuditTrailService auditService)
        {
            _registrationService = registrationService;
            _auditService = auditService;
        }

        /// <summary>
        /// Upload a document for a registration.
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(50_000_000)] // 50MB limit
        public async Task<IActionResult> Upload([FromForm] int registrationId, [FromForm] int documentTypeId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is required." });

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            // Generate a unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, uniqueFileName);
            var relativePath = $"/uploads/documents/{uniqueFileName}";

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var entity = new FIDocument
            {
                FilePath = filePath,
                UploadedOnUtc = System.DateTime.UtcNow
            };

            await _registrationService.AddDocumentAsync(registrationId, entity);
            await _auditService.LogCreateAsync("FIDocument", entity.Id, 0, "Document uploaded");

            return Ok(new
            {
                success = true,
                message = "Document uploaded successfully.",
                entity.Id,
                entity.FilePath,
                entity.DocumentTypeId,
                entity.UploadedOnUtc
            });
        }

        /// <summary>
        /// Get all documents by registration ID.
        /// </summary>
        [HttpGet("registration/{registrationId:int}")]
        public async Task<IActionResult> GetByRegistration(int registrationId)
        {
            var docs = await _registrationService.GetDocumentsByRegistrationIdAsync(registrationId);
            if (docs == null || docs.Count == 0)
                return NotFound(new { message = "No documents found for this registration." });

            return Ok(docs);
        }

        /// <summary>
        /// Get a specific document by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var docs = await _registrationService.GetDocumentsByRegistrationIdAsync(id); // get all (or better: use a direct GetByIdAsync if exists)
            var document = docs?.FirstOrDefault(d => d.Id == id);
            if (document == null)
                return NotFound(new { message = "Document not found." });

            return Ok(document);
        }

        /// <summary>
        /// Delete a document by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var docs = await _registrationService.GetDocumentsByRegistrationIdAsync(0);
            var document = docs?.FirstOrDefault(d => d.Id == id);
            if (document == null)
                return NotFound(new { message = "Document not found." });

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            await _registrationService.RemoveDocumentAsync(id);
            await _auditService.LogDeleteAsync("FIDocument", id, 0, "Document deleted");

            return Ok(new { success = true, message = "Document deleted successfully." });
        }
    }
}
