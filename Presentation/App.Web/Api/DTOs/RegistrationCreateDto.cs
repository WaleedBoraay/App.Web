using App.Core.Domain.Ref;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Api.DTOs
{
    public record RegistrationCreateDto(
        [property: Required] string ApplicationName,
        [property: Required] string InstitutionName,
        [property: Required] string LicenseNumber,
        [property: Required] LicenseSector LicenseSector,
        [property: Required] string FinancialType,
        [property: Required] DateOnly LicenseIssueDate,
        [property: Required] DateOnly LicenseExpiryDate,
        [property: Required] string Country,
        string? ParentCountry,
        string? ParentName,
        string? ParentLicenseId,
        int? ParentEmployees,
        List<AttachmentDto>? Attachments
    );
}
