using App.Core.Domain.Registrations;

namespace App.Web.Api.DTOs
{
    public class AttachmentDto
    {
        public DocumentType Type { get; set; }
        public string FileUrl { get; set; }
        public IFormFile File { get; set; }
    }

}
