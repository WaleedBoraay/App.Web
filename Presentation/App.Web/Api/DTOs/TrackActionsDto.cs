using System.ComponentModel.DataAnnotations;

namespace App.Web.Api.DTOs
{
    public record NotificationPrefs(
        bool Email,
        bool Sms,
        bool Marketing
    );

    public record TrackActionsDto(
        [property: Required] string Validation,
        [property: Required] string Approval,
        [property: Required] string Audit,
        [property: Range(0, 100)] int Progress,
        string AccountType,
        decimal? InitialDeposit,
        NotificationPrefs Notifications
    );
}
