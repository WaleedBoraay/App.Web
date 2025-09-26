using System.IO;
using System.Xml.Linq;
using App.Core.Domain.Localization;
using App.Core.RepositoryServices;

namespace App.Services.Installation
{
    public class LocalizationSeeder
    {
        private readonly IRepository<LocaleStringResource> _resourceRepository;
        private readonly IRepository<Language> _languageRepository;

        private readonly string _localizationPath;

        public LocalizationSeeder(
            IRepository<LocaleStringResource> resourceRepository,
            IRepository<Language> languageRepository)
        {
            _resourceRepository = resourceRepository;
            _languageRepository = languageRepository;

            _localizationPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"App_Data\Localization");
        }

        public async Task SeedAsync()
        {
            var languages = await _languageRepository.GetAllAsync(q => q);
            if (!languages.Any())
                return;

            foreach (var lang in languages)
            {
                string? fileName = lang.LanguageCulture switch
                {
                    "en-US" => "defaultResources.res.en.xml",
                    "ar-SA" => "defaultResources.res.ar-SA.xml",
                    "ar-EG" => "defaultResources.res.ar-EG.xml",
                    _ => null
                };

                if (fileName == null)
                    continue;

                var filePath = Path.Combine(_localizationPath, fileName);
                if (!File.Exists(filePath))
                    continue;

                var doc = XDocument.Load(filePath);
                foreach (var element in doc.Descendants("LocaleResource"))
                {
                    var resourceName = element.Attribute("Name")?.Value;
                    var resourceValue = element.Element("Value")?.Value ?? string.Empty;

                    if (string.IsNullOrEmpty(resourceName))
                        continue;

                    var exists = (await _resourceRepository.GetAllAsync(q =>
                        q.Where(r => r.ResourceName == resourceName && r.LanguageId == lang.Id))).Any();

                    if (!exists)
                    {
                        var resource = new LocaleStringResource
                        {
                            LanguageId = lang.Id,
                            ResourceName = resourceName,
                            ResourceValue = resourceValue
                        };

                        await _resourceRepository.InsertAsync(resource);
                    }
                }
            }
        }
    }
}
