using App.Core.Domain.Directory;
using App.Core.Domain.Institutions;
using App.Core.Domain.Localization;
using App.Core.Domain.Registrations;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using App.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace App.Services.Installation
{
    public class InstallSampleData
    {
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<AppUser> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<Institution> _institutionRepository;
        private readonly IRepository<Registration> _registrationRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<LocaleStringResource> _resourceRepository;

        public InstallSampleData(
            IRepository<Country> countryRepository,
            IRepository<Language> languageRepository,
            IRepository<AppUser> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<Institution> institutionRepository,
            IRepository<Registration> registrationRepository,
            IEncryptionService encryptionService,
            IRepository<LocaleStringResource> resourceRepository)
        {
            _countryRepository = countryRepository;
            _languageRepository = languageRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _institutionRepository = institutionRepository;
            _registrationRepository = registrationRepository;
            _encryptionService = encryptionService;
            _resourceRepository = resourceRepository;
        }

        public async Task InstallAsync()
        {
            await SeedCountriesAsync();
            await SeedLanguagesAsync();
            await SeedLocalizationResourcesAsync();
            await SeedSampleUsersAsync();
            await SeedSampleInstitutionAndRegistrationAsync();
        }

        private async Task SeedCountriesAsync()
        {
            // Minimal subset using ISO3166 helper
            foreach (var c in ISO3166.GetCollection())
            {
                var exists = (await _countryRepository.GetAllAsync(q => q.Where(x => x.ThreeLetterIsoCode == c.Alpha3))).Any();
                if (!exists)
                {
                    var country = new Country
                    {
                        Name = c.Name,
                        TwoLetterIsoCode = c.Alpha2,
                        ThreeLetterIsoCode = c.Alpha3,
                        NumericIsoCode = c.NumericCode,
                        Published = true,
                        DisplayOrder = 1
                    };
                    await _countryRepository.InsertAsync(country);
                }
            }
        }

        private async Task SeedLanguagesAsync()
        {
            if (!(await _languageRepository.GetAllAsync(q => q)).Any())
            {
                int order = 1;
                foreach (var lang in ISO639.GetCollection())
                {
                    await _languageRepository.InsertAsync(new Language
                    {
                        Name = lang.Name,
                        LanguageCulture = lang.LanguageCulture,
                        UniqueSeoCode = lang.UniqueSeoCode,
                        FlagImageFileName = lang.FlagImageFileName ?? "",
                        Rtl = lang.Rtl,
                        Published = lang.Published,
                        DisplayOrder = order++
                    });
                }
            }
        }
        private async Task SeedLocalizationResourcesAsync()
        {
            var languages = await _languageRepository.GetAllAsync(q => q);
            if (!languages.Any())
                return;

            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Localization");

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

                var filePath = Path.Combine(basePath, fileName);
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
                        await _resourceRepository.InsertAsync(new LocaleStringResource
                        {
                            LanguageId = lang.Id,
                            ResourceName = resourceName,
                            ResourceValue = resourceValue
                        });
                    }
                }
            }
        }

        private async Task SeedSampleUsersAsync()
        {
            var samples = new[]
            {
                new { Username = "maker", Role = "Maker" },
                new { Username = "checker", Role = "Checker" }
            };

            foreach (var s in samples)
            {
                if (!(await _userRepository.GetAllAsync(q => q.Where(u => u.Username == s.Username))).Any())
                {
                    var salt = _encryptionService.CreateSaltKey();
                    var hash = _encryptionService.CreatePasswordHash($"{s.Username}@123", salt, PasswordFormat.Hashed);

                    var user = new AppUser
                    {
                        Username = s.Username,
                        Email = $"{s.Username}@demo.local",
                        PasswordSalt = salt,
                        PasswordHash = hash,
                        PasswordFormatId = (int)PasswordFormat.Hashed,
                        IsActive = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await _userRepository.InsertAsync(user);

                    var role = (await _roleRepository.GetAllAsync(q => q.Where(r => r.SystemName == s.Role))).FirstOrDefault();
                    if (role != null)
                    {
                        await _userRoleRepository.InsertAsync(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        });
                    }
                }
            }
        }

        private async Task SeedSampleInstitutionAndRegistrationAsync()
        {
            if (!(await _institutionRepository.GetAllAsync(q => q)).Any())
            {
                var institution = new Institution
                {
                    Name = "Test Bank Ltd",
                    LicenseNumber = "LIC-001",
                    CreatedOnUtc = DateTime.UtcNow,
                    Address = "123 Finance St, Cairo, Egypt",
                    CountryId = (await _countryRepository.GetAllAsync(q => q.Where(c => c.TwoLetterIsoCode == "EG"))).First().Id,
                    FinancialDomainId = 1,
                    LicenseSectorId = 1,
                    LicenseIssueDate = new DateTime(DateTime.UtcNow.Year - 5, 1, 1),
                    LicenseExpiryDate = new DateTime(DateTime.UtcNow.Year + 5, 12, 31),
                    IsActive = true
                };

                await _institutionRepository.InsertAsync(institution);

                var registration = new Registration
                {
                    InstitutionId = institution.Id,
                    InstitutionName = institution.Name,
                    LicenseNumber = institution.LicenseNumber,
                    FinancialDomainId = institution.FinancialDomainId,
                    LicenseSectorId = institution.LicenseSectorId,
                    IssueDate = institution.LicenseIssueDate,
                    ExpiryDate = institution.LicenseExpiryDate,
                    CountryId = institution.CountryId,
                    Status = RegistrationStatus.Draft,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _registrationRepository.InsertAsync(registration);
            }
        }
    }
}
