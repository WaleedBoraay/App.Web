namespace App.Web.Api.DTOs
{
    public record InstitutionDto(
        string Name,
        int CountryId,
        string Address
    );
}
