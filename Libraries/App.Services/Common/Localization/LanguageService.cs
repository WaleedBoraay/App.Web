using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Localization;
using App.Core.RepositoryServices;

namespace App.Services.Localization
{
    public class LanguageService : ILanguageService
    {
        private readonly IRepository<Language> _languageRepository;
        private static int _currentLanguageId = 1; // default = Arabic/English as per seed

        public LanguageService(IRepository<Language> languageRepository)
        {
            _languageRepository = languageRepository;
        }

        public async Task<Language> GetByIdAsync(int id)
            => await _languageRepository.GetByIdAsync(id);

        public async Task<IList<Language>> GetAllAsync(bool onlyPublished = true)
        {
            return await _languageRepository.GetAllAsync(q =>
            {
                if (onlyPublished)
                    q = q.Where(l => l.Published);
                return q.OrderBy(l => l.DisplayOrder);
            });
        }

        public async Task InsertAsync(Language language)
            => await _languageRepository.InsertAsync(language);

        public async Task UpdateAsync(Language language)
            => await _languageRepository.UpdateAsync(language);

        public async Task DeleteAsync(Language language)
            => await _languageRepository.DeleteAsync(language);

        // new
        public async Task<Language> GetCurrentLanguageAsync()
            => await GetByIdAsync(_currentLanguageId);

        public async Task SetCurrentLanguageAsync(int languageId)
        {
            _currentLanguageId = languageId;
            await Task.CompletedTask;
        }
    }
}
