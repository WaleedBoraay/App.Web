using App.Core.Domain.Notifications;
using App.Core.Domain.Registrations;
using App.Services.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public interface IRegistrationService
    {
        #region CRUD

        Task<Registration> GetByIdAsync(int id);
        Task<IList<Registration>> GetAllAsync();
        Task<Registration> InsertAsync(Registration registration);
        Task<Registration> UpdateAsync(Registration registration);
        Task DeleteAsync(int id);

        #endregion

        #region Workflow
        Task ApproveRegistrationAsync(int registrationId, int approvedByUserId);
        Task RejectRegistrationAsync(int registrationId, int rejectedByUserId, string comment = null);
        Task SubmitAsync(int registrationId, int performedByUserId, string remarks = null);
        Task ValidateAsync(int registrationId, int performedByUserId, ValidationStatus status, string remarks = null);
        Task ApproveAsync(int registrationId, int performedByUserId, ApprovalStatus status, string remarks = null);
        Task AuditAsync(int registrationId, int performedByUserId, AuditStatus status, string remarks = null);
        Task ReturnForEditAsync(int registrationId, int performedByUserId, string remarks = null);
        Task RejectAsync(int registrationId, int performedByUserId, string remarks = null);
        Task ArchiveAsync(int registrationId, int performedByUserId, string remarks = null);
        Task FinalSubmissionAsync(int registrationId, int performedByUserId, string remarks = null);

        #endregion

        #region Contacts & Documents

        Task<IList<Contact>> GetContactsByRegistrationIdAsync(int registrationId);
        Task<IList<Document>> GetDocumentsByRegistrationIdAsync(int registrationId);
        Task AddContactAsync(int registrationId, Contact contact);
        Task RemoveContactAsync(int contactId);
        Task UpdateContactAsync(Contact contact);
        Task AddDocumentAsync(int registrationId, Document document);
        Task RemoveDocumentAsync(int documentId);

        #endregion

        #region Validation (New)
        /// <summary>
        /// Checks whether a registration already exists with the same institution name or license number.
        /// </summary>
        Task<bool> CheckDuplicateAsync(string institutionName, string licenseNumber);
        #endregion

        #region Status & Audit (New)
        /// <summary>
        /// Returns the full status change history (audit log) for a given registration.
        /// </summary>
        Task<IList<RegistrationStatusLog>> GetStatusHistoryAsync(int registrationId);
        #endregion

        #region Notifications (New)
        /// <summary>
        /// Sends notification for workflow events (submit, approve, reject, return, etc.).
        /// </summary>
        Task NotifyAsync(int registrationId, NotificationEvent eventType, int triggeredByUserId, int recipientUserId, NotificationChannel channel = NotificationChannel.InApp, string customMessage = null);
        #endregion
    }
}
