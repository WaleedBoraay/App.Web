using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Ref;

namespace App.Services.Lookup
{
    public interface ILookupService
    {
        // Business Scale Ranges
        Task<IList<BusinessScaleRange>> GetBusinessScaleRangesAsync();
        Task<BusinessScaleRange> GetBusinessScaleRangeByIdAsync(int id);
        Task<BusinessScaleRange> FindBusinessScaleRangeAsync(int employeesCount);

        // Employee Ranges
        Task<IList<EmployeeRange>> GetEmployeeRangesAsync();
        Task<EmployeeRange> GetEmployeeRangeByIdAsync(int id);
        Task<EmployeeRange> FindEmployeeRangeAsync(int employeesCount);

        // Enums
        Task<IList<string>> GetLicenseTypesAsync();
        Task<IList<string>> GetLicenseSectorsAsync();
        Task<IList<string>> GetFinancialDomainsAsync();

        // Invalidate cache (for Admin actions)
        Task ClearLookupCacheAsync();
    }
}
