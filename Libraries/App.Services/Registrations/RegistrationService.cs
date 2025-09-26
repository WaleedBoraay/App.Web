using App.Core.Domain.Institutions;
using App.Core.Domain.Notifications;
using App.Core.Domain.Registrations;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;
using App.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRepository<Registration> _registrationRepository;
        private readonly IRepository<FIContact> _fiContactRepository;
        private readonly IDocumentService _documentService;
        private readonly ILocalizationService _localizationService;
        private readonly IRegistrationStatusLogService _statusLogService;
        private readonly INotificationService _notificationService;
        private readonly IRepository<Institution> _institutionRepository;
        private readonly IRepository<AppUser> _userRepository;

        public RegistrationService(
            IRepository<Registration> registrationRepository,
            IRepository<FIContact> fiContactRepository,
            IDocumentService documentService,
            ILocalizationService localizationService,
            IRegistrationStatusLogService statusLogService,
            INotificationService notificationService,
            IRepository<Institution> institutionRepository,
            IRepository<AppUser> userRepository
            )
        {
            _registrationRepository = registrationRepository;
            _fiContactRepository = fiContactRepository;
            _documentService = documentService;
            _localizationService = localizationService;
            _statusLogService = statusLogService;
            _notificationService = notificationService;
            _institutionRepository = institutionRepository;
            _userRepository = userRepository;
        }

        #region CRUD

        public async Task<Registration> GetByIdAsync(int id)
            => await _registrationRepository.GetByIdAsync(id);

        public async Task<IList<Registration>> GetAllAsync()
            => await _registrationRepository.GetAllAsync(q => q.OrderBy(r => r.CreatedOnUtc));

        public async Task<Registration> InsertAsync(Registration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration), await _localizationService.GetResourceAsync("Registration.Insert.Null"));

            registration.CreatedOnUtc = DateTime.UtcNow;
            await _registrationRepository.InsertAsync(registration);

            // Localized log
            var logTemplate = await _localizationService.GetResourceAsync("Registration.Created.Log");
            var logMessage = _localizationService.FormatMessage(logTemplate, registration.CreatedByUserId);

            var log = new FIRegistrationStatusLog
            {
                RegistrationId = registration.Id,
                RegistrationStatusId = (int)registration.Status,
                PerformedBy = registration.CreatedByUserId,
                ActionDateUtc = DateTime.UtcNow,
                Remarks = logMessage
            };
            await _statusLogService.InsertAsync(log);
            await _localizationService.GetResourceAsync("Registration.Insert.Success");
            return registration;
        }

        public async Task<Registration> UpdateAsync(Registration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration), await _localizationService.GetResourceAsync("Registration.Update.Null"));

            await _registrationRepository.UpdateAsync(registration);
            await _localizationService.GetResourceAsync("Registration.Update.Success");
            return registration;
        }

        public async Task DeleteAsync(int id)
        {
            var registration = await _registrationRepository.GetByIdAsync(id);
            if (registration == null)
                throw new ArgumentNullException(nameof(registration), await _localizationService.GetResourceAsync("Registration.NotFound"));

            await _registrationRepository.DeleteAsync(registration);

            await _localizationService.GetResourceAsync("Registration.Delete.Success");
        }

        #endregion

        #region Workflow

        private async Task AddStatusLog(
    Registration registration,
    int performedByUserId,
    string remarks,
    ValidationStatus? validationStatus = null,
    ApprovalStatus? approvalStatus = null,
    AuditStatus? auditStatus = null)
        {
            var log = new FIRegistrationStatusLog
            {
                RegistrationId = registration.Id,
                RegistrationStatus = registration.Status,
                RegistrationStatusId = registration.StatusId,
                ValidationStatus = validationStatus,
                ApprovalStatus = approvalStatus,
                AuditStatus = auditStatus,
                PerformedBy = performedByUserId,
                ActionDateUtc = DateTime.UtcNow,
                Remarks = remarks ?? $"Status changed to {registration.Status}"
            };

            await _statusLogService.InsertAsync(log);
        }

        public async Task SubmitAsync(int registrationId, int performedByUserId, string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            registration.Status = RegistrationStatus.Submitted;
            registration.StatusId = (int)RegistrationStatus.Submitted;
            registration.SubmittedDateUtc = DateTime.UtcNow;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            await AddStatusLog(registration, performedByUserId, remarks);
        }

        public async Task ValidateAsync(
    int registrationId,
    int performedByUserId,
    ValidationStatus status,
    string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            // Update registration status
            registration.Status = RegistrationStatus.Approved; // Validator بيوافق
            registration.StatusId = (int)RegistrationStatus.Approved;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            // Load again after update
            var reg = await _registrationRepository.GetByIdAsync(registrationId);
            if (reg == null)
                throw new Exception("Registration not found after update");

            // ✅ Activate Institution
            var institution = await _institutionRepository.GetByIdAsync(reg.InstitutionId);
            if (institution != null && !institution.IsActive)
            {
                institution.IsActive = true;
                await _institutionRepository.UpdateAsync(institution);
            }

            var user = (await _userRepository.GetAllAsync(q =>
                q.Where(u => u.InstitutionId == reg.InstitutionId))).FirstOrDefault();

            if (user != null && !user.IsActive)
            {
                user.IsActive = true;
                user.RegistrationId = reg.Id; // ربط اليوزر بالريجستراشن
                await _userRepository.UpdateAsync(user);
            }

            // ✅ Log + Notifications
            await _notificationService.SendAsync(
                registrationId,
                NotificationEvent.RegistrationApproved, // عشان ده من الـ Validator
                performedByUserId,
                user?.Id ?? 0,
                NotificationChannel.InApp
            );

            await AddStatusLog(reg, performedByUserId, remarks ?? "Approved by Validator");
        }



        public async Task ApproveAsync(int registrationId, int performedByUserId, ApprovalStatus status, string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            registration.Status = RegistrationStatus.Approved;
            registration.StatusId = (int)RegistrationStatus.Approved;
            registration.ApprovedDateUtc = DateTime.UtcNow;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            await AddStatusLog(registration, performedByUserId, remarks, approvalStatus: status);
        }

        public async Task RejectAsync(int registrationId, int performedByUserId, string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            registration.Status = RegistrationStatus.Rejected;
            registration.StatusId = (int)RegistrationStatus.Rejected;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            await AddStatusLog(registration, performedByUserId, remarks);
        }

        public async Task ReturnForEditAsync(int registrationId, int performedByUserId, string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            registration.Status = RegistrationStatus.ReturnedForEdit;
            registration.StatusId = (int)RegistrationStatus.ReturnedForEdit;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            await AddStatusLog(registration, performedByUserId, remarks);
        }

        public async Task ArchiveAsync(int registrationId, int performedByUserId, string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            registration.Status = RegistrationStatus.Archived;
            registration.StatusId = (int)RegistrationStatus.Archived;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            await AddStatusLog(registration, performedByUserId, remarks);
        }

        public async Task FinalSubmissionAsync(int registrationId, int performedByUserId, string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            registration.Status = RegistrationStatus.FinalSubmission;
            registration.StatusId = (int)RegistrationStatus.FinalSubmission;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            await AddStatusLog(registration, performedByUserId, remarks);
        }

        public async Task AuditAsync(int registrationId, int performedByUserId, AuditStatus status, string remarks = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new Exception("Registration not found");

            registration.Status = RegistrationStatus.UnderReview; // أو تعمل Status جديد مثلاً RegistrationStatus.Audited
            registration.StatusId = (int)RegistrationStatus.UnderReview;
            registration.AuditedDateUtc = DateTime.UtcNow;
            registration.UpdatedByUserId = performedByUserId;

            await _registrationRepository.UpdateAsync(registration);

            await AddStatusLog(registration, performedByUserId, remarks, auditStatus: status);
        }
        private async Task ChangeStatusAsync(
            int registrationId,
            RegistrationStatus newStatus,
            int performedByUserId,
            string remarks,
            ValidationStatus? validationStatus = null,
            ApprovalStatus? approvalStatus = null,
            AuditStatus? auditStatus = null)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
                throw new ArgumentNullException(nameof(registration), await _localizationService.GetResourceAsync("Registration.NotFound"));

            var oldStatus = registration.Status;

            // ✅ تحقق من صحة الانتقال
            if (!IsValidTransition(oldStatus, newStatus))
                throw new InvalidOperationException($"Invalid transition from {oldStatus} to {newStatus}");

            registration.Status = newStatus;
            registration.StatusId = (int)newStatus;
            registration.UpdatedByUserId = performedByUserId;

            switch (newStatus)
            {
                case RegistrationStatus.Submitted:
                    registration.SubmittedDateUtc = DateTime.UtcNow;
                    registration.SubmittedToUserId = performedByUserId;
                    break;

                case RegistrationStatus.Approved:
                    registration.ApprovedDateUtc = DateTime.UtcNow;
                    break;

                case RegistrationStatus.UnderReview:
                    registration.AuditedDateUtc = DateTime.UtcNow;
                    break;
            }

            await _registrationRepository.UpdateAsync(registration);

            // سجل Log
            var oldStatusName = await _localizationService.GetLocalizedEnumAsync(oldStatus);
            var newStatusName = await _localizationService.GetLocalizedEnumAsync(newStatus);

            var logTemplate = await _localizationService.GetResourceAsync("Registration.Status.Changed");
            var logMessage = _localizationService.FormatMessage(logTemplate, oldStatusName, newStatusName);

            var log = new FIRegistrationStatusLog
            {
                RegistrationId = registration.Id,
                RegistrationStatus = newStatus,
                RegistrationStatusId = (int)newStatus,
                ValidationStatus = validationStatus,
                ApprovalStatus = approvalStatus,
                AuditStatus = auditStatus,
                PerformedBy = performedByUserId,
                ActionDateUtc = DateTime.UtcNow,
                Remarks = remarks ?? logMessage
            };
            await _statusLogService.InsertAsync(log);
        }
        private bool IsValidTransition(RegistrationStatus oldStatus, RegistrationStatus newStatus)
        {
            return oldStatus switch
            {
                RegistrationStatus.Draft => newStatus == RegistrationStatus.Submitted || newStatus == RegistrationStatus.Archived,
                RegistrationStatus.Submitted => newStatus == RegistrationStatus.UnderReview || newStatus == RegistrationStatus.ReturnedForEdit,
                RegistrationStatus.UnderReview => newStatus == RegistrationStatus.Approved || newStatus == RegistrationStatus.Rejected || newStatus == RegistrationStatus.ReturnedForEdit,
                RegistrationStatus.Approved => newStatus == RegistrationStatus.FinalSubmission || newStatus == RegistrationStatus.Archived,
                RegistrationStatus.Rejected => newStatus == RegistrationStatus.ReturnedForEdit,
                RegistrationStatus.ReturnedForEdit => newStatus == RegistrationStatus.Submitted,
                _ => false
            };
        }

        #endregion

        #region Contacts & Documents

        public async Task<IList<FIContact>> GetContactsByRegistrationIdAsync(int registrationId)
            => await _fiContactRepository.GetAllAsync(q => q.Where(c => c.RegistrationId == registrationId));

        public async Task<IList<FIDocument>> GetDocumentsByRegistrationIdAsync(int registrationId)
        {
            if (registrationId <= 0)
                throw new ArgumentException(await _localizationService.GetResourceAsync("Registration.Id.Invalid"));
            var regDoc = await _documentService.GetRegistrationDocumentsByRegistrationIdAsync(registrationId);
            var documents = new List<FIDocument>();
            foreach (var rd in regDoc)
            {
                var doc = await _documentService.GetByIdAsync(rd.DocumentId);
                if (doc != null)
                    documents.Add(doc);
            }
            return documents;
        }

        public async Task AddContactAsync(int registrationId, FIContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact), await _localizationService.GetResourceAsync("Registration.Contact.Null"));

            contact.RegistrationId = registrationId;
            await _fiContactRepository.InsertAsync(contact);

        }
        public async Task UpdateContactAsync(FIContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException(nameof(contact));

            contact.UpdatedOnUtc = DateTime.UtcNow;
            await _fiContactRepository.UpdateAsync(contact);
        }

        public async Task AddDocumentAsync(int registrationId, FIDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document), await _localizationService.GetResourceAsync("Registration.Document.Null"));

            var doc = await _documentService.InsertAsync(document);
            await _documentService.InsertAsync(new RegistrationDocument
            {
                RegistrationId = registrationId,
                DocumentId = doc.Id
            });
        }

        public async Task<bool> CheckDuplicateAsync(string institutionName, string licenseNumber)
        {
            var duplicates = await _registrationRepository.GetAllAsync(q =>
                q.Where(s => s.InstitutionName == institutionName || s.LicenseNumber == licenseNumber));
            return duplicates != null && duplicates.Count > 0;
        }

        #endregion

        #region Status & Audit (New)

        public async Task<IList<FIRegistrationStatusLog>> GetStatusHistoryAsync(int registrationId)
        {
            return await _statusLogService.GetByRegistrationIdAsync(registrationId);
        }

        public async Task NotifyAsync(int registrationId, NotificationEvent eventType, int triggeredByUserId, int recipientUserId, NotificationChannel channel = NotificationChannel.InApp, string customMessage = null)
        {
            var tokens = new Dictionary<string, string>
{
    { "CustomMessage", customMessage ?? string.Empty }
};
            await _notificationService.SendAsync(
                registrationId,
                eventType,
                triggeredByUserId,
                recipientUserId,
                channel,
                tokens
            );
        }

        public async Task RemoveContactAsync(int contactId)
        {
            var contact = await _fiContactRepository.GetByIdAsync(contactId);
            if (contact == null)
                throw new ArgumentNullException(nameof(contact), await _localizationService.GetResourceAsync("Registration.Contact.NotFound"));
            await _fiContactRepository.DeleteAsync(contact);
            await _localizationService.GetResourceAsync("Registration.Contact.Removed");
        }

        public async Task RemoveDocumentAsync(int documentId)
        {
            var document = await _documentService.GetByIdAsync(documentId);
            if (document == null)
                throw new ArgumentNullException(nameof(document), await _localizationService.GetResourceAsync("Registration.Document.NotFound"));
            // Assuming there's a method to delete the document from the repository
            await _documentService.DeleteAsync(document.Id);
            await _localizationService.GetResourceAsync("Registration.Document.Removed");
        }


        #endregion
    }
}
