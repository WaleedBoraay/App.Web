using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Localization;

namespace App.Services.Localization
{
    public interface ILocalizationService
    {
        // Resources
        Task<string> GetResourceAsync(string key, string defaultValue = null);
        Task<LocaleStringResource> GetByIdAsync(int id);
        Task<LocaleStringResource> GetByNameAsync(string resourceName, int languageId);
        Task<IList<LocaleStringResource>> GetAllResourcesAsync(int languageId);
        Task InsertResourceAsync(LocaleStringResource resource);
        Task UpdateResourceAsync(LocaleStringResource resource);
        Task DeleteResourceAsync(LocaleStringResource resource);

        // Helpers
        Task<string> GetResourceAsync(string key, int languageId, string defaultValue = null);
        Task<string> GetLocalizedEnumAsync<TEnum>(TEnum enumValue) where TEnum : struct;
        string FormatMessage(string template, params object[] args);
    }
}
