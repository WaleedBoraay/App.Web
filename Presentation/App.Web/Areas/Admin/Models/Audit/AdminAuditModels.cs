using System;
using System.Collections.Generic;
using App.Core.Domain.Audit;

namespace App.Core.Domain.Admin.Models.Audit
{
    public class AuditLogQuery
    {
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public int? UserId { get; set; }
        public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string FieldName { get; set; }
        public AuditActionType? Action { get; set; }
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 50;
    }

    public class AuditLogModel
    {
        public IList<AuditLogEntryModel> Items { get; set; } = new List<AuditLogEntryModel>();
        public int TotalCount { get; set; }
    }

    public class AuditLogEntryModel
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int ChangedByUserId { get; set; }
        public DateTime ChangedOnUtc { get; set; }
        public int ActionId { get; set; }
        public AuditActionType Action { get; set; }
        public string ClientIp { get; set; }
        public string Comment { get; set; }
    }
}
