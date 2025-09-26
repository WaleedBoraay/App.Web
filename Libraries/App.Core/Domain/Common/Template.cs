using System;
using App.Core.Domain.Common;

namespace App.Core.Domain.Common
{
    public partial class Template : BaseEntity
    {
        public string Name { get; set; }
        public string TemplateType { get; set; } // Email, SMS, Document
        public string Content { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}