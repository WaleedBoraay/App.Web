using System;

namespace App.Core.Domain.Common
{
    /// <summary>
    /// Represents a file attachment (used for documents, IDs, etc.).
    /// </summary>
    public class Attachment : BaseEntity
    {
        public string FileName { get; set; }
        public string StoragePath { get; set; }
        public string MimeType { get; set; }
        public string Extension { get; set; }
        public long FileSize { get; set; }

        public int UploadedByUserId { get; set; }
        public DateTime UploadedOnUtc { get; set; }
    }
}
