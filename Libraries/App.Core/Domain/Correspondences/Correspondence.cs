using System;
using App.Core.Domain.Common;

namespace App.Core.Domain.Correspondences
{
    public partial class Correspondence : BaseEntity
    {
        public int SenderUserId { get; set; }
        public int RecipientUserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime SentOnUtc { get; set; }
        public string Status { get; set; } // Sent, Delivered, Read, Archived
    }
}