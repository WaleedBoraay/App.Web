using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Services.Common;

namespace App.Services.Registrations
{
    public interface IRegistrationWorkflowService
    {
        Task SubmitAsync(int registrationId, int performedByUserId, int recipientUserId);
        Task ValidateAsync(int registrationId, int performedByUserId, int recipientUserId, ValidationStatus status);
        Task ApproveAsync(int registrationId, int performedByUserId, int recipientUserId, ApprovalStatus status);
        Task AuditAsync(int registrationId, int performedByUserId, int recipientUserId, AuditStatus status);
        Task ReturnForEditAsync(int registrationId, int performedByUserId, int recipientUserId);
        Task RejectAsync(int registrationId, int performedByUserId, int recipientUserId);
        Task ArchiveAsync(int registrationId, int performedByUserId, int recipientUserId);
        Task FinalSubmissionAsync(int registrationId, int performedByUserId, int recipientUserId);
    }
}
