using App.Core.Domain.Registrations;

namespace App.Web.Api.DTOs
{
    public record TrackDto(
        RegistrationStatus Status
    );
}
