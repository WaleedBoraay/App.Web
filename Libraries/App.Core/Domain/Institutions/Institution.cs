using App.Core.Domain.Organization;
using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;

namespace App.Core.Domain.Institutions
{
    /// <summary>
    /// Represents a financial institution (final approved entity after registration).
    /// </summary>
    public class Institution : BaseEntity
    {
        /// <summary>
        /// Institution official name
        /// </summary>
        public string Name { get; set; }

        public string BusinessPhoneNumber { get; set; }
        public string Email { get; set; }
        /// <summary>
        /// License number assigned to the institution
        /// </summary>
        public string LicenseNumber { get; set; }

        /// <summary>
        /// License sector (Banking / Exchange / Insurance…)
        /// </summary>
        public int LicenseSectorId { get; set; }
        public LicenseSector LicenseSector { get; set; }

        /// <summary>
        /// Financial domain (Islamic / Commercial)
        /// </summary>
        public int FinancialDomainId { get; set; }
        public FinancialDomain FinancialDomain { get; set; }

        /// <summary>
        /// License issue date
        /// </summary>
        public DateTime? LicenseIssueDate { get; set; }

        /// <summary>
        /// License expiry date
        /// </summary>
        public DateTime? LicenseExpiryDate { get; set; }

        /// <summary>
        /// Country reference
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Address of the institution
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Is the institution active
        /// </summary>
        public bool IsActive { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        // Navigation
        public virtual ICollection<Sector> Departments { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<Contract> Contracts { get; set; }
        public virtual ICollection<Duty> Duties { get; set; }
        public virtual Registration Registration { get; set; }
        public virtual ICollection<Branch> Branches { get; set; }
        public virtual ICollection<InstituteDocument> Documents { get; set; } = new List<InstituteDocument>();

    }
}
