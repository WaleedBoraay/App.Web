using App.Core.Domain.Institutions;
using App.Core.Domain.Notifications;
using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;
using App.Services.Notifications;
using App.Services.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Institutions
{
    public class InstitutionService : IInstitutionService
    {
        private readonly IRepository<Institution> _institutionRepository;
        private readonly IRepository<Registration> _registrationRepository;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<AppUser> _userRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly INotificationService _notificationService;


        public InstitutionService(
            IRepository<Institution> institutionRepository,
            IRepository<Registration> registrationRepository,
            ILocalizationService localizationService,
            IRepository<AppUser> userRepository,
            IEncryptionService encryptionService,
            INotificationService notificationService)
        {
            _institutionRepository = institutionRepository;
            _registrationRepository = registrationRepository;
            _localizationService = localizationService;
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _notificationService = notificationService;

        }

        #region Utilities
        private string GeneratePassword(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region CRUD
        public async Task<Institution> CreateAsync(Institution institute, string email)
        {
            if (institute == null) throw new ArgumentNullException(nameof(institute));

            institute.IsActive = false;
            institute.CreatedOnUtc = DateTime.UtcNow;
            await _institutionRepository.InsertAsync(institute);
            var saltKey = _encryptionService.CreateSaltKey();

            // Generate user for this institution
            var password = GeneratePassword();
            var passwordHash = _encryptionService.CreatePasswordHash(password, saltKey, PasswordFormat.Hashed);

            var user = new AppUser
            {
                Username = email,
                Email = email,
                PasswordHash = password,
                PasswordSalt = saltKey,
                PasswordFormatId = (int)PasswordFormat.Hashed,
                InstitutionId = institute.Id,
                IsActive = false,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _userRepository.InsertAsync(user);

            // send email with credentials
            await _notificationService.SendAsync(
                null,
                NotificationEvent.UserCreated,
                triggeredByUserId: 0,
                recipientUserId: user.Id,
                channel: NotificationChannel.InApp,
                tokens: new Dictionary<string, string>
                {
                    {"Username", user.Username},
                    {"Password", password}
                }
            );

            //Send notification to admin
            var adminUsers = await _userRepository.GetAllAsync(q => q.Where(u => u.IsActive && u.InstitutionId == null));
            foreach (var admin in adminUsers)
            {
                await _notificationService.SendAsync(
                    null,
                    NotificationEvent.InstitutionCreated,
                    triggeredByUserId: user.Id,
                    recipientUserId: admin.Id,
                    channel: NotificationChannel.InApp,
                    tokens: new Dictionary<string, string>
                    {
                        {"InstitutionName", institute.Name},
                        {"ContactEmail", email},
                        {"Username", user.Username},
                        {"Password", password}
                    }
                );
            }

            return institute;
        }
        public async Task<Institution> GetByIdAsync(int id)
            => await _institutionRepository.GetByIdAsync(id);

        public async Task<IList<Institution>> GetAllAsync()
            => await _institutionRepository.GetAllAsync(q => q.OrderBy(i => i.Name));

        public async Task<Institution> InsertAsync(Institution institution)
        {
            if (institution == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Institution.Insert.NotFound"));

            if (await ExistsAsync(institution.Name, institution.LicenseNumber))
                throw new InvalidOperationException(await _localizationService.GetResourceAsync("Institution.Insert.Duplicate"));

            institution.CreatedOnUtc = DateTime.UtcNow;
            await _institutionRepository.InsertAsync(institution);
            await _localizationService.GetResourceAsync("Institution.Insert.Success");
            return institution;
        }

        public async Task<Institution> UpdateAsync(Institution institution)
        {
            if (institution == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Institution.Update.NotFound"));

            institution.UpdatedOnUtc = DateTime.UtcNow;
            await _institutionRepository.UpdateAsync(institution);
            await _localizationService.GetResourceAsync("Institution.Update.Success");
            return institution;
        }

        public async Task DeleteAsync(int id)
        {
            var institution = await _institutionRepository.GetByIdAsync(id);
            if (institution == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Institution.Delete.NotFound"));

            await _institutionRepository.DeleteAsync(institution);
            await _localizationService.GetResourceAsync("Institution.Delete.Success");
        }

        #endregion

        #region Checks

        public async Task<bool> ExistsAsync(string institutionName, string licenseNumber)
        {
            var institutions = await _institutionRepository.GetAllAsync(q =>
                q.Where(i => i.Name == institutionName || i.LicenseNumber == licenseNumber));
            return institutions != null && institutions.Count > 0;
        }

        public async Task<bool> IsValidForRegistrationAsync(int institutionId)
        {
            var institution = await _institutionRepository.GetByIdAsync(institutionId);
            if (institution == null) return false;

            // Example rule: must not be archived & license valid
            return institution.IsActive && institution.LicenseExpiryDate > DateTime.UtcNow;
        }

        #endregion

        #region Relations

        public async Task<IList<Institution>> GetAllInstitutionsWithRegistrationAndBranchesAsync()
        {
            return await _institutionRepository.GetAllAsync(q =>
                q.Include(i => i.Registration)
                 .ThenInclude(r => r.Branches));
        }

        public async Task<IList<Registration>> GetRegistrationsAsync(int institutionId)
            => await _registrationRepository.GetAllAsync(q => q.Where(r => r.InstitutionId == institutionId));

        public async Task<Registration> GetLatestRegistrationAsync(int institutionId)
        {
            var registrations = await _registrationRepository.GetAllAsync(q =>
                q.Where(r => r.InstitutionId == institutionId)
                 .OrderByDescending(r => r.CreatedOnUtc));
            return registrations.FirstOrDefault();
        }

        #endregion

        #region Search

        public async Task<PagedResult<Institution>> SearchAsync(
            string name = null,
            string licenseNumber = null,
            int? countryId = null,
            LicenseSector? licenseSector = null,
            FinancialDomain? financialDomain = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _institutionRepository.Table;

            if (!string.IsNullOrEmpty(name))
                query = query.Where(i => i.Name.Contains(name));

            if (!string.IsNullOrEmpty(licenseNumber))
                query = query.Where(i => i.LicenseNumber.Contains(licenseNumber));

            if (countryId.HasValue)
                query = query.Where(i => i.CountryId == countryId);

            if (licenseSector.HasValue)
                query = query.Where(i => i.LicenseSector == licenseSector);

            if (financialDomain.HasValue)
                query = query.Where(i => i.FinancialDomain == financialDomain);

            var totalCount = query.Count();

            var items = query
                .OrderBy(i => i.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(new PagedResult<Institution>(items, totalCount, pageIndex, pageSize));
        }

        public async Task<IDictionary<LicenseSector, int>> GetInstitutionCountBySectorAsync()
        {
            var institutions = await _institutionRepository.GetAllAsync(q => q, true);
            return institutions
                .GroupBy(i => i.LicenseSector)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IDictionary<int, int>> GetInstitutionCountByCountryAsync()
        {
            var institutions = await _institutionRepository.GetAllAsync(q => q, true);
            return institutions
                .GroupBy(i => i.CountryId)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        #endregion
    }
}
