using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;

namespace App.Web.Api.DTOs
{
    public record ContactDto(
        ContactType ContactType,
        string JobTitle,
        string FirstName,
        string MiddleName,
        string LastName,
        string Email,
        string Phone,

        string CivilId,
        string PassportId
    );
}
