using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using App.Core.Domain.Registrations;

namespace App.Web.Api.DTOs
{
    public record ContactPersonDto(
        [property: Required]
    string JobTitle,
        [property: Required]
    string FirstName,
        string? MiddleName,
        [property: Required]
    string LastName,
        [property: Required]
    string Nationality,
        string? CivilId,
        string? PassportId,
        [property: Phone] string? ContactPhone,
        [property: Phone] string? BusinessPhone,
        [property: EmailAddress] string Email,
        List<AttachmentDto>? Attachments
    );
}
