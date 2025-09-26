using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Services.Common;

namespace App.Services.Directory
{
    public interface IStateProvinceService
    {
        Task<StateProvince> GetByIdAsync(int id);
        Task<IList<StateProvince>> GetByCountryIdAsync(int countryId);
        Task<ServiceResult<StateProvince>> InsertAsync(StateProvince state);
        Task<ServiceResult<StateProvince>> UpdateAsync(StateProvince state);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
