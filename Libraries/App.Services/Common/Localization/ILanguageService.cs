using App.Core.Domain.Localization;

public interface ILanguageService
{
    // existing
    Task<Language> GetByIdAsync(int id);
    Task<IList<Language>> GetAllAsync(bool onlyPublished = true);
    Task InsertAsync(Language language);
    Task UpdateAsync(Language language);
    Task DeleteAsync(Language language);

    // new
    Task<Language> GetCurrentLanguageAsync();
    Task SetCurrentLanguageAsync(int languageId);
}
