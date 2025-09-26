using App.Core.Domain.Registrations;
using App.Web.Infrastructure.Mapper.BaseModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Registrations
{
    /// <summary>
    /// Admin ViewModel for FI Registration entity
    /// </summary>
    public record class RegistrationModel : BaseAppEntityModel
    {
        public int Id { get; set; }

        public int RegistrationId { get; set; }

        // Institution Snapshot
        [Display(Name = "Institution")]
        public int InstitutionId { get; set; }
        public string? InstitutionName { get; set; }

        // License Data
        [Required]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

        [Display(Name = "License Sector")]
        public int LicenseSectorId { get; set; }

        [Display(Name = "Financial Domain")]
        public int FinancialDomainId { get; set; }


        [Display(Name = "Issue Date")]
        public DateTime? IssueDate { get; set; }

        [Display(Name = "Expiry Date")]
        public DateTime? ExpiryDate { get; set; }

        // Workflow & Status
        [Display(Name = "Registration Status")]
        public int StatusId { get; set; }
        public RegistrationStatus Status { get; set; }

        [Display(Name = "Approval Status")]
        public ApprovalStatus ApprovalStatus { get; set; }

        [Display(Name = "Validation Status")]
        public ValidationStatus ValidationStatus { get; set; }

        // System Info
        public DateTime CreatedOnUtc { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? UpdatedByUserName { get; set; }

        // Linked Data (simplified, can expand later)
        public List<ContactModel> Contacts { get; set; } = new();
        public List<DocumentModel> Documents { get; set; } = new();
        public List<StatusLogModel> StatusLogs { get; set; } = new();


        public int CountryId { get; set; }
        public string? CountryName { get; set; }
        public bool IsActive { get; set; }

        public IEnumerable<SelectListItem> AvailableInstitutions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AvailableCountries { get; set; } = new List<SelectListItem>();
    }
}
