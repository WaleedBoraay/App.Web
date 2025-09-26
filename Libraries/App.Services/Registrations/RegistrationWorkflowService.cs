using App.Core.Domain.Notifications;
using App.Core.Domain.Registrations;
using App.Services.Common;
using App.Services.Localization;
using App.Services.Notifications;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public class RegistrationWorkflowService : IRegistrationWorkflowService
    {
        private readonly IRegistrationService _registrationService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        public RegistrationWorkflowService(
            IRegistrationService registrationService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {
            _registrationService = registrationService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        public async Task SubmitAsync(int registrationId, int performedByUserId, int recipientUserId)
        {
            await _registrationService.SubmitAsync(registrationId, performedByUserId);
            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationSubmitted, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.Submit.Success");
        }

        public async Task ValidateAsync(int registrationId, int performedByUserId, int recipientUserId, ValidationStatus status)
        {
            await _registrationService.ValidateAsync(registrationId, performedByUserId, status);

            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationValidated, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.Validate.Success");
        }

        public async Task ApproveAsync(int registrationId, int performedByUserId, int recipientUserId, ApprovalStatus status)
        {
            await _registrationService.ApproveAsync(registrationId, performedByUserId, status);

            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationApproved, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.Approve.Success");
        }

        public async Task AuditAsync(int registrationId, int performedByUserId, int recipientUserId, AuditStatus status)
        {
            await _registrationService.AuditAsync(registrationId, performedByUserId, status);

            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationAudited, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.Audit.Success");
        }

        public async Task ReturnForEditAsync(int registrationId, int performedByUserId, int recipientUserId)
        {
            await _registrationService.ReturnForEditAsync(registrationId, performedByUserId);

            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationReturnedForEdit, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.ReturnForEdit.Success");
        }

        public async Task RejectAsync(int registrationId, int performedByUserId, int recipientUserId)
        {
            await _registrationService.RejectAsync(registrationId, performedByUserId);

            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationRejected, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.Reject.Success");
        }

        public async Task ArchiveAsync(int registrationId, int performedByUserId, int recipientUserId)
        {
            await _registrationService.ArchiveAsync(registrationId, performedByUserId);

            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationArchived, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.Archive.Success");
        }

        public async Task FinalSubmissionAsync(int registrationId, int performedByUserId, int recipientUserId)
        {
            await _registrationService.FinalSubmissionAsync(registrationId, performedByUserId);

            await _notificationService.SendAsync(registrationId, NotificationEvent.RegistrationFinalSubmission, performedByUserId, recipientUserId);
            await _localizationService.GetResourceAsync("Workflow.FinalSubmission.Success");
        }
    }
}
