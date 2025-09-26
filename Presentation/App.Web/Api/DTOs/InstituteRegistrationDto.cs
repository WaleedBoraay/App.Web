using App.Web.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

public class InstituteRegistrationDto
{
    public string InstitutionName { get; set; }
    public string LicenseNumber { get; set; }
    public int LicenseTypeId { get; set; }
    public int LicenseSectorId { get; set; }
    public int FinancialDomainId { get; set; }
    public int CountryId { get; set; }
    public string Address { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int CreatedByUserId { get; set; }
    public int? InstitutionId { get; set; }

    // ?? ?? ???? ?????
    [FromForm] public IFormFile? LicenseFile { get; set; }
    [FromForm] public IFormFile? CivilIdFile { get; set; }
    [FromForm] public IFormFile? PassportFile { get; set; }
    [FromForm] public IFormFile? DocumentFile { get; set; }

    // ???? ???????? ?? ?? ??
    [FromForm] public List<ContactPersonDto> Contacts { get; set; } = new();
    [FromForm] public AccountDto Account { get; set; }
    [FromForm] public TrackDto Track { get; set; }
    [FromForm] public NotificationDto Notification { get; set; }
}
