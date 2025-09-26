using App.Core.Domain.Common;
using App.Core.Domain.Security;

namespace App.Core.Domain.Registrations
{
    public partial class WorkflowStep : BaseEntity
    {
        public string Name { get; set; }
        public int? NextStepId { get; set; }

        public int RoleAllowedId { get; set; }
        public Role RoleAllowed { get; set; }
    }
}