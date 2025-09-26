using App.Core.Domain.Registrations;
using App.Web.Infrastructure.Mapper.BaseModel;

namespace App.Web.Areas.Admin.Models.Registrations
{
    public record class DocumentModel : BaseAppEntityModel
    {
        public int Id { get; set; }
        public int RegistrationId { get; set; }

        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadedOnUtc { get; set; }
    }
}
