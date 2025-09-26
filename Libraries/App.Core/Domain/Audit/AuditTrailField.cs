namespace App.Core.Domain.Audit
{
    /// <summary>
    /// Lookup for audited fields within a table.
    /// </summary>
    public class AuditTrailField : BaseEntity
    {
        public int AuditTrailTableId { get; set; }
        public string DBFieldName { get; set; }
        public string SystemName { get; set; }
    }
}
