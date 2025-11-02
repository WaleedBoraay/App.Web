using App.Core.Domain.Registrations;
using App.Web.Infrastructure.Mapper.BaseModel;

namespace App.Web.Api.Models
{
	public record class DocumentModel : BaseAppEntityModel
	{
		public int RegistrationId { get; set; }

		public int DocumentTypeId { get; set; }
		public DocumentType DocumentType { get; set; }

		public string FileName { get; set; }
		public string FilePath { get; set; }

		public DateTime UploadedOnUtc { get; set; }
		public int ContactId { get; set; }

		public IFormFile LicenseFile { get; set; }
		public IFormFile OtherDocument { get; set; }
		public IFormFile PassportDocument { get; set; }
		public IFormFile CivilIdDocument { get; set; }
	}
}
