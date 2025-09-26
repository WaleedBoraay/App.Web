using App.Core.Domain.Registrations;
using App.Services.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public class RegistrationActionHintsService : IRegistrationActionHintsService
    {
        private readonly IContactService _contactService;
        private readonly IDocumentService _documentService;
        private readonly IRegistrationStatusLogService _statusLogService;

        public RegistrationActionHintsService(
            IContactService contactService,
            IDocumentService documentService,
            IRegistrationStatusLogService statusLogService)
        {
            _contactService = contactService;
            _documentService = documentService;
            _statusLogService = statusLogService;
        }

        #region Available Actions
        public Task<IList<RegistrationAction>> GetAvailableActionsAsync(
            Registration registration,
            int currentUserId,
            string currentUserRole)
        {
            var actions = new List<RegistrationAction>();

            switch (currentUserRole)
            {
                case "Maker":
                    if (registration.Status == RegistrationStatus.Draft ||
                        registration.Status == RegistrationStatus.ReturnedForEdit)
                        actions.Add(RegistrationAction.Submit);
                    break;

                case "Checker":
                    if (registration.Status == RegistrationStatus.Submitted)
                    {
                        actions.Add(RegistrationAction.Validate);
                        actions.Add(RegistrationAction.ReturnForEdit);
                    }
                    break;

                case "Regulator":
                    if (registration.Status == RegistrationStatus.UnderReview)
                    {
                        actions.Add(RegistrationAction.Approve);
                        actions.Add(RegistrationAction.Reject);
                        actions.Add(RegistrationAction.ReturnForEdit);
                    }
                    break;

                case "Admin":
                    actions.AddRange(new[]
                    {
                        RegistrationAction.Submit,
                        RegistrationAction.Validate,
                        RegistrationAction.Approve,
                        RegistrationAction.Reject,
                        RegistrationAction.ReturnForEdit,
                        RegistrationAction.Audit,
                        RegistrationAction.Archive,
                        RegistrationAction.FinalSubmission
                    });
                    break;
            }

            return Task.FromResult<IList<RegistrationAction>>(actions);
        }
        #endregion

        #region Next Statuses
        public Task<IList<RegistrationStatus>> GetNextStatusesAsync(
            RegistrationStatus currentStatus,
            string currentUserRole)
        {
            var statuses = new List<RegistrationStatus>();

            if (currentUserRole == "Maker" && currentStatus == RegistrationStatus.Draft)
                statuses.Add(RegistrationStatus.Submitted);

            if (currentUserRole == "Checker" && currentStatus == RegistrationStatus.Submitted)
                statuses.Add(RegistrationStatus.UnderReview);

            if (currentUserRole == "Regulator" && currentStatus == RegistrationStatus.UnderReview)
            {
                statuses.Add(RegistrationStatus.Approved);
                statuses.Add(RegistrationStatus.Rejected);
                statuses.Add(RegistrationStatus.ReturnedForEdit);
            }

            if (currentUserRole == "Admin")
            {
                statuses.AddRange(new[]
                {
                    RegistrationStatus.Approved,
                    RegistrationStatus.Rejected,
                    RegistrationStatus.Archived,
                    RegistrationStatus.FinalSubmission
                });
            }

            return Task.FromResult<IList<RegistrationStatus>>(statuses);
        }
        #endregion

        #region Validation Hints
        public async Task<ServiceResult<IList<string>>> GetValidationHintsAsync(int registrationId)
        {
            var hints = new List<string>();

            var contacts = await _contactService.GetByRegistrationIdAsync(registrationId);
            if (contacts == null || contacts.Count == 0)
                hints.Add("At least one contact is required.");

            var documents = await _documentService.GetRegistrationDocumentByIdAsync(registrationId);
            if (documents == null)
                hints.Add("At least one supporting document is required.");

            return ServiceResult<IList<string>>.Success(hints);
        }
        #endregion

        #region Notification Recipients
        public Task<IList<int>> GetNotificationRecipientsAsync(
            Registration registration,
            RegistrationAction action,
            int performedByUserId)
        {
            var recipients = new List<int>();

            switch (action)
            {
                case RegistrationAction.Submit:
                    // TODO: Use user service to get Checkers/Regulators
                    break;

                case RegistrationAction.Approve:
                case RegistrationAction.Reject:
                case RegistrationAction.ReturnForEdit:
                    if (registration.CreatedByUserId != performedByUserId)
                        recipients.Add(registration.CreatedByUserId);
                    break;

                case RegistrationAction.FinalSubmission:
                    // TODO: Notify Admin users (via IUserService)
                    break;
            }

            return Task.FromResult<IList<int>>(recipients);
        }
        #endregion
    }
}
