using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Services.Common;

namespace App.Services.Directory
{
    public interface ICountryService
    {
        // CRUD
        Task<Country> GetByIdAsync(int id);
        Task<IList<Country>> GetAllAsync(bool onlyActive = true);
        Task<ServiceResult<Country>> InsertAsync(Country country);
        Task<ServiceResult<Country>> UpdateAsync(Country country);
        Task<ServiceResult> DeleteAsync(int id);

        // Lookup
        Task<Country> GetByCodeAsync(string twoLetterIsoCode);
        Task<Country> GetByNameAsync(string name);

        // Search & Paging
        Task<PagedResult<Country>> SearchAsync(
            string keyword = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        // Hierarchy
        Task<IList<StateProvince>> GetStatesByCountryIdAsync(int countryId);

        // Utility
        Task<bool> ExistsAsync(string twoLetterIsoCode);
    }
}
