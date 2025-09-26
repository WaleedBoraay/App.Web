using System;
using App.Core.Domain.Common;

namespace App.Core.Domain.Registrations
{
    public partial class FIDocument : BaseEntity
    {
        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType
        {
            get => (DocumentType)DocumentTypeId;
            set => DocumentTypeId = (int)value;
        }

        public string FilePath { get; set; }
        public DateTime UploadedOnUtc { get; set; }
    }
}