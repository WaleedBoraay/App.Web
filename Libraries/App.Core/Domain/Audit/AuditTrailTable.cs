namespace App.Core.Domain.Audit
{
    /// <summary>
    /// Lookup for audited database tables.
    /// </summary>
    public class AuditTrailTable : BaseEntity
    {
        public string DBName { get; set; }
        public string SystemName { get; set; } // used as key for localization
    }
}
