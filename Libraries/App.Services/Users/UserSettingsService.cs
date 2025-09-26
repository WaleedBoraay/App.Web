using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace App.Services.Users
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IRepository<UserPreference> _preferenceRepo;
        private readonly ILanguageService _languageService;

        public UserSettingsService(
            IRepository<UserPreference> preferenceRepo,
            ILanguageService languageService)
        {
            _preferenceRepo = preferenceRepo;
            _languageService = languageService;
        }

        public async Task<UserPreference> GetByUserIdAsync(int userId)
        {
            return await _preferenceRepo.Table
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task SetLanguageAsync(int userId, int languageId)
        {
            // Ensure language exists
            var lang = await _languageService.GetByIdAsync(languageId);
            if (lang == null)
                throw new ArgumentException("Invalid languageId");

            var pref = await EnsurePreferenceAsync(userId);
            pref.LanguageId = languageId;
            pref.UpdatedOnUtc = DateTime.UtcNow;
            await _preferenceRepo.UpdateAsync(pref);
        }

        public async Task ToggleMfaAsync(int userId, bool enable)
        {
            var pref = await EnsurePreferenceAsync(userId);
            pref.EnableMfa = enable;
            pref.UpdatedOnUtc = DateTime.UtcNow;
            await _preferenceRepo.UpdateAsync(pref);
        }

        public async Task UpdateNotificationPreferencesAsync(
            int userId,
            bool notifyByEmail,
            bool notifyBySms,
            bool notifyInApp)
        {
            var pref = await EnsurePreferenceAsync(userId);
            pref.NotifyByEmail = notifyByEmail;
            pref.NotifyBySms = notifyBySms;
            pref.NotifyInApp = notifyInApp;
            pref.UpdatedOnUtc = DateTime.UtcNow;
            await _preferenceRepo.UpdateAsync(pref);
        }

        private async Task<UserPreference> EnsurePreferenceAsync(int userId)
        {
            var pref = await _preferenceRepo.Table.FirstOrDefaultAsync(p => p.UserId == userId);
            if (pref == null)
            {
                pref = new UserPreference
                {
                    UserId = userId,
                    LanguageId = null,
                    EnableMfa = false,
                    NotifyByEmail = true,
                    NotifyBySms = false,
                    NotifyInApp = true,
                    UpdatedOnUtc = DateTime.UtcNow
                };
                await _preferenceRepo.InsertAsync(pref);
            }
            return pref;
        }
    }
}
