using App.Core.Domain.Registrations;
using App.Web.Infrastructure.Mapper.BaseModel;

namespace App.Web.Areas.Admin.Models.Registrations
{
    public record class StatusLogModel : BaseAppEntityModel
    {
        public int Id { get; set; }

        public int RegistrationId { get; set; }

        public RegistrationStatus RegistrationStatus { get; set; }

        public ValidationStatus? ValidationStatus { get; set; }

        public ApprovalStatus? ApprovalStatus { get; set; }

        public AuditStatus? AuditStatus { get; set; }

        public int PerformedBy { get; set; }

        public DateTime ActionDateUtc { get; set; }

        public string Remarks { get; set; }
    }
}
