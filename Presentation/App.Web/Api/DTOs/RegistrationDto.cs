namespace App.Web.Api.DTOs;

public record RegistrationDto(
    int InstitutionId,
    string LicenseNumber,
    int LicenseTypeId,
    int LicenseSectorId,
    int StatusId
);
