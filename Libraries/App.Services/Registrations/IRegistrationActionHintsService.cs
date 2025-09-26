using App.Core.Domain.Registrations;
using App.Services.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    /// <summary>
    /// Provides context-aware action hints for Registration workflow
    /// depending on user role and current status.
    /// </summary>
    public interface IRegistrationActionHintsService
    {
        /// <summary>
        /// Returns available workflow actions for a given registration and user.
        /// </summary>
        Task<IList<RegistrationAction>> GetAvailableActionsAsync(
            Registration registration,
            int currentUserId,
            string currentUserRole);

        /// <summary>
        /// Returns next possible statuses based on current registration status.
        /// </summary>
        Task<IList<RegistrationStatus>> GetNextStatusesAsync(
            RegistrationStatus currentStatus,
            string currentUserRole);

        /// <summary>
        /// Returns validation hints (e.g., missing docs/contacts) before submission.
        /// </summary>
        Task<ServiceResult<IList<string>>> GetValidationHintsAsync(int registrationId);

        /// <summary>
        /// Returns notification recipients for actions (who should be notified).
        /// </summary>
        Task<IList<int>> GetNotificationRecipientsAsync(
            Registration registration,
            RegistrationAction action,
            int performedByUserId);
    }
}
