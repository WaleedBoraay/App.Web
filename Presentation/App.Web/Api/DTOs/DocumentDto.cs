using App.Core.Domain.Registrations;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Api.DTOs
{
    public class DocumentDto
    {
        public DocumentType[] Type { get; set; }

        [FromForm]
        public IFormFile? DocumentFile { get; set; }
        public IFormFile? LicenseFile { get; set; }
    }
}
