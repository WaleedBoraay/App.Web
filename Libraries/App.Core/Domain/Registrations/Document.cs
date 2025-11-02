using System;
using App.Core.Domain.Common;

namespace App.Core.Domain.Registrations
{
    public partial class Document : BaseEntity
    {
        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType
        {
            get => (DocumentType)DocumentTypeId;
            set => DocumentTypeId = (int)value;
        }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedOnUtc { get; set; }
        public int ContactId { get; set; }
    }
}