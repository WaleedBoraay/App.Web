using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Core.RepositoryServices;
using App.Services.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public class RegistrationSearchService : IRegistrationSearchService
    {
        private readonly IRepository<Registration> _registrationRepository;

        public RegistrationSearchService(IRepository<Registration> registrationRepository)
        {
            _registrationRepository = registrationRepository;
        }

        public async Task<Common.PagedResult<Registration>> SearchAsync(
            string institutionName = null,
            string licenseNumber = null,
            int? countryId = null,
            RegistrationStatus? status = null,
            LicenseType? licenseType = null,
            LicenseSector? licenseSector = null,
            FinancialDomain? financialDomain = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _registrationRepository.Table;

            if (!string.IsNullOrEmpty(institutionName))
                query = query.Where(r => r.InstitutionName.Contains(institutionName));

            if (!string.IsNullOrEmpty(licenseNumber))
                query = query.Where(r => r.LicenseNumber.Contains(licenseNumber));

            if (countryId.HasValue)
                query = query.Where(r => r.CountryId == countryId);

            if (status.HasValue)
                query = query.Where(r => r.Status == status);

            if (licenseType.HasValue)
                query = query.Where(r => r.LicenseType == licenseType);

            if (licenseSector.HasValue)
                query = query.Where(r => r.LicenseSector == licenseSector);

            if (financialDomain.HasValue)
                query = query.Where(r => r.FinancialDomain == financialDomain);

            if (createdFromUtc.HasValue)
                query = query.Where(r => r.CreatedOnUtc >= createdFromUtc.Value);

            if (createdToUtc.HasValue)
                query = query.Where(r => r.CreatedOnUtc <= createdToUtc.Value);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(r => r.CreatedOnUtc)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(new Common.PagedResult<Registration>(items, totalCount, pageIndex, pageSize));
        }
    }
}
