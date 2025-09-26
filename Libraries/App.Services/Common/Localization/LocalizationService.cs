using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Localization;
using App.Core.RepositoryServices;

namespace App.Services.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IRepository<LocaleStringResource> _resourceRepository;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;

        public LocalizationService(
            IRepository<LocaleStringResource> resourceRepository,
            ILanguageService languageService,
            IWorkContext workContext)
        {
            _resourceRepository = resourceRepository;
            _languageService = languageService;
            _workContext = workContext;

        }

        public async Task<string> GetResourceAsync(string key, string defaultValue = null)
        {
            var language = await _workContext.GetWorkingLanguageAsync();
            var resource = await _resourceRepository.GetAllAsync(q =>
                q.Where(r => r.ResourceName == key && r.LanguageId == language.Id));

            var value = resource.FirstOrDefault()?.ResourceValue;
            return string.IsNullOrEmpty(value) ? defaultValue ?? key : value;
        }

        public async Task<string> GetResourceAsync(string key, int languageId, string defaultValue = null)
        {
            var resource = await _resourceRepository.GetAllAsync(q =>
                q.Where(r => r.ResourceName == key && r.LanguageId == languageId));

            var value = resource.FirstOrDefault()?.ResourceValue;
            return string.IsNullOrEmpty(value) ? defaultValue ?? key : value;
        }

        public async Task<LocaleStringResource> GetByIdAsync(int id)
            => await _resourceRepository.GetByIdAsync(id);

        public async Task<LocaleStringResource> GetByNameAsync(string resourceName, int languageId)
        {
            var list = await _resourceRepository.GetAllAsync(q =>
                q.Where(r => r.ResourceName == resourceName && r.LanguageId == languageId));
            return list.FirstOrDefault();
        }

        public async Task<IList<LocaleStringResource>> GetAllResourcesAsync(int languageId)
        {
            return await _resourceRepository.GetAllAsync(q =>
                q.Where(r => r.LanguageId == languageId));
        }

        public async Task InsertResourceAsync(LocaleStringResource resource)
            => await _resourceRepository.InsertAsync(resource);

        public async Task UpdateResourceAsync(LocaleStringResource resource)
            => await _resourceRepository.UpdateAsync(resource);

        public async Task DeleteResourceAsync(LocaleStringResource resource)
            => await _resourceRepository.DeleteAsync(resource);

        // New: Localize Enums
        public async Task<string> GetLocalizedEnumAsync<TEnum>(TEnum enumValue) where TEnum : struct
        {
            var enumName = enumValue.ToString();
            var key = $"Enums.{typeof(TEnum).Name}.{enumName}";
            // e.g., Enums.RegistrationStatus.Approved

            var result = await GetResourceAsync(key, enumName);
            return result ?? enumName;
        }

        // New: Format message with placeholders
        public string FormatMessage(string template, params object[] args)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            return string.Format(template, args);
        }
    }
}
