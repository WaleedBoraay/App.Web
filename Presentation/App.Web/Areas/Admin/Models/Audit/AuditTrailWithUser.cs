namespace App.Web.Areas.Admin.Models.Audit
{
    public class AuditTrailWithUser
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedOnUtc { get; set; }
        public int ChangedByUserId { get; set; }
        public string ChangedByUsername { get; set; }
    }
}
