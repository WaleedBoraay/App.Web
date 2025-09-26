namespace App.Web.Api.DTOs
{
    public record AccountDto(
        int UpdatedByUserId,
        string AccountType,
        decimal? InitialDeposit,
        bool NotifyByEmail,
        bool NotifyBySms,
        bool NotifyInApp
    );
}
