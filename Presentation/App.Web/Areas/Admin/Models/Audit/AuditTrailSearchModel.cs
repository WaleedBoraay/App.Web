using App.Core.Domain.Audit;

namespace App.Web.Areas.Admin.Models.Audit
{
    public class AuditTrailSearchModel
    {
        public string EntityName { get; set; }
        public int? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Pagination
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }

        public IList<AuditTrailWithUser> Results { get; set; } = new List<AuditTrailWithUser>();
    }
}
