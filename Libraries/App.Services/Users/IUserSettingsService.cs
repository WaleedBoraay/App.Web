using App.Core.Domain.Users;
using System.Threading.Tasks;

namespace App.Services.Users
{
    public interface IUserSettingsService
    {
        Task<UserPreference> GetByUserIdAsync(int userId);

        Task SetLanguageAsync(int userId, int languageId);

        Task ToggleMfaAsync(int userId, bool enable);

        Task UpdateNotificationPreferencesAsync(
            int userId,
            bool notifyByEmail,
            bool notifyBySms,
            bool notifyInApp);
    }
}
