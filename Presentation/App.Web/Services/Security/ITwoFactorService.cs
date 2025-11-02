using System.Threading.Tasks;

namespace App.Services.Security
{
    public interface ITwoFactorService
    {
        Task<string> GenerateTwoFactorSecretAsync(int userId, int secretByteLength = 20);
        Task<string> GetOtpAuthUriAsync(string secret, string email, string issuer = "App");
        Task<bool> ValidateTwoFactorTokenAsync(int userId, string token, int allowedDriftSteps = 1);
        Task EnableTwoFactorAsync(int userId);
        Task DisableTwoFactorAsync(int userId);
        Task<bool> IsTwoFactorEnabledAsync(int userId);
    }
}