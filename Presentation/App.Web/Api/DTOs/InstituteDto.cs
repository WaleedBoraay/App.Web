using App.Core.Domain.Ref;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Api.DTOs
{
    public record InstituteDto
    {
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Phone]
        public string BusinessPhoneNumber { get; set; }
        public string LicenseNumber { get; set; }
        public string Password { get; set; }
        public LicenseSector licenseSector { get; set; }
        public FinancialDomain financialDomain { get; set; }
        public int CountryId { get; set; }
        public string Address { get; set; }
        public System.DateTime? IssueDate { get; set; }
        public System.DateTime? ExpiryDate { get; set; }

        [FromForm]
        public IFormFile? LicenseFile { get; set; }

        [FromForm]
        public IFormFile? DocumentFile { get; set; }
    }
}