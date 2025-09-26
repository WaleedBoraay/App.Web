using System;
using App.Core.Domain.Ref;
using App.Core.Domain.Institutions;

namespace App.Core.Domain.Registrations
{
    /// <summary>
    /// Represents a registration record (snapshot + lifecycle).
    /// </summary>
    public partial class Registration : BaseEntity
    {
        /// <summary>
        /// Institution reference
        /// </summary>
        public int InstitutionId { get; set; }
        public virtual Institution Institution { get; set; }

        /// <summary>
        /// Institution name snapshot (copied from Institution at submission time)
        /// </summary>
        public string InstitutionName { get; set; }

        /// <summary>
        /// License number snapshot
        /// </summary>
        public string LicenseNumber { get; set; }

        /// <summary>
        /// License sector (Banking / Exchange)
        /// </summary>
        public int LicenseSectorId { get; set; }
        public LicenseSector LicenseSector { get; set; }

        /// <summary>
        /// License type (Islamic, Commercial, FinTech…)
        /// </summary>
        public int LicenseTypeId { get; set; }
        public LicenseType LicenseType { get; set; }

        /// <summary>
        /// Financial domain (Islamic / Commercial)
        /// </summary>
        public int FinancialDomainId { get; set; }
        public FinancialDomain FinancialDomain { get; set; }

        /// <summary>
        /// License issue date
        /// </summary>
        public DateTime? IssueDate { get; set; }

        /// <summary>
        /// License expiry date
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Country of the institution
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Address of the institution
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Main lifecycle status of registration
        /// </summary>
        /// 
        public int StatusId { get; set; }
        public RegistrationStatus Status { get; set; }

        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public int? SubmittedToUserId { get; set; }

        public DateTime? CreatedOnUtc { get; set; }
        public DateTime? SubmittedDateUtc { get; set; }
        public DateTime? ApprovedDateUtc { get; set; }
        public DateTime? AuditedDateUtc { get; set; }

        public int? BusinessScaleRangeId { get; set; }
        public int? EmployeeRangeId { get; set; }

        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    }
}
