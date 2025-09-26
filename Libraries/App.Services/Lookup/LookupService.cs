using App.Core.Caching;
using App.Core.Domain.Ref;
using App.Core.RepositoryServices;
using App.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Lookup
{
    public class LookupService : ILookupService
    {
        private readonly IRepository<BusinessScaleRange> _businessScaleRepo;
        private readonly IRepository<EmployeeRange> _employeeRangeRepo;
        private readonly IStaticCacheManager _cache;

        // CacheKey definitions
        private static readonly CacheKey BusinessScaleCacheKey = new("Lookup.BusinessScaleRanges");
        private static readonly CacheKey EmployeeRangeCacheKey = new("Lookup.EmployeeRanges");

        public LookupService(
            IRepository<BusinessScaleRange> businessScaleRepo,
            IRepository<EmployeeRange> employeeRangeRepo,
            IStaticCacheManager cache)
        {
            _businessScaleRepo = businessScaleRepo;
            _employeeRangeRepo = employeeRangeRepo;
            _cache = cache;
        }

        #region BusinessScaleRange

        public async Task<IList<BusinessScaleRange>> GetBusinessScaleRangesAsync()
        {
            return await _cache.GetAsync(BusinessScaleCacheKey, async () =>
            {
                return await _businessScaleRepo.Table
                    .OrderBy(x => x.MinValue)
                    .ToListAsync();
            });
        }

        public async Task<BusinessScaleRange> GetBusinessScaleRangeByIdAsync(int id)
        {
            var list = await GetBusinessScaleRangesAsync();
            return list.FirstOrDefault(x => x.Id == id);
        }

        public async Task<BusinessScaleRange> FindBusinessScaleRangeAsync(int employeesCount)
        {
            var list = await GetBusinessScaleRangesAsync();
            return list.FirstOrDefault(x => employeesCount >= x.MinValue && employeesCount <= x.MaxValue);
        }

        #endregion

        #region EmployeeRange

        public async Task<IList<EmployeeRange>> GetEmployeeRangesAsync()
        {
            return await _cache.GetAsync(EmployeeRangeCacheKey, async () =>
            {
                return await _employeeRangeRepo.Table
                    .OrderBy(x => x.MinValue)
                    .ToListAsync();
            });
        }

        public async Task<EmployeeRange> GetEmployeeRangeByIdAsync(int id)
        {
            var list = await GetEmployeeRangesAsync();
            return list.FirstOrDefault(x => x.Id == id);
        }

        public async Task<EmployeeRange> FindEmployeeRangeAsync(int employeesCount)
        {
            var list = await GetEmployeeRangesAsync();
            return list.FirstOrDefault(x => employeesCount >= x.MinValue && employeesCount <= x.MaxValue);
        }

        #endregion

        #region Enums

        public Task<IList<string>> GetLicenseTypesAsync()
        {
            var values = System.Enum.GetNames(typeof(LicenseType)).ToList();
            return Task.FromResult<IList<string>>(values);
        }

        public Task<IList<string>> GetLicenseSectorsAsync()
        {
            var values = System.Enum.GetNames(typeof(LicenseSector)).ToList();
            return Task.FromResult<IList<string>>(values);
        }

        public Task<IList<string>> GetFinancialDomainsAsync()
        {
            var values = System.Enum.GetNames(typeof(FinancialDomain)).ToList();
            return Task.FromResult<IList<string>>(values);
        }

        #endregion

        #region Cache Management

        public async Task ClearLookupCacheAsync()
        {
            await _cache.RemoveByPrefixAsync("Lookup");
        }

        #endregion
    }
}
