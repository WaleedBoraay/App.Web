using System;

namespace App.Core.Domain.Registrations
{
    /// <summary>
    /// Tracks workflow transitions & status history for a registration.
    /// </summary>
    public class RegistrationStatusLog : BaseEntity
    {
        /// <summary>
        /// The main registration record
        /// </summary>
        public int RegistrationId { get; set; }

        /// <summary>
        /// The overall lifecycle status
        /// </summary>
        /// 
        public int RegistrationStatusId { get; set; }
        public RegistrationStatus RegistrationStatus { get; set; }

        /// <summary>
        /// Optional validation step result
        /// </summary>
        public ValidationStatus? ValidationStatus { get; set; }

        /// <summary>
        /// Optional approval step result
        /// </summary>
        public ApprovalStatus? ApprovalStatus { get; set; }

        /// <summary>
        /// Optional audit step result
        /// </summary>
        public AuditStatus? AuditStatus { get; set; }

        /// <summary>
        /// Who performed the action (UserId)
        /// </summary>
        public int PerformedBy { get; set; }

        /// <summary>
        /// Timestamp of the action
        /// </summary>
        public DateTime ActionDateUtc { get; set; }

        /// <summary>
        /// Optional remarks or reason for status change
        /// </summary>
        public string Remarks { get; set; }
    }
}
