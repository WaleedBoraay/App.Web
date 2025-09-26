using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Directory
{
    public class StateProvinceService : BaseService, IStateProvinceService
    {
        private readonly IRepository<StateProvince> _stateRepository;
        private readonly ILocalizationService _localizationService;

        public StateProvinceService(IRepository<StateProvince> stateRepository, ILocalizationService localizationService)
        {
            _stateRepository = stateRepository;
            _localizationService = localizationService;
        }

        public async Task<StateProvince> GetByIdAsync(int id)
            => await _stateRepository.GetByIdAsync(id);

        public async Task<IList<StateProvince>> GetByCountryIdAsync(int countryId)
        {
            var states = await _stateRepository.GetAllAsync(q => q.Where(s => s.CountryId == countryId));
            return states.OrderBy(s => s.DisplayOrder).ToList();
        }

        public async Task<ServiceResult<StateProvince>> InsertAsync(StateProvince state)
        {
            if (state == null)
                return Failed<StateProvince>(await _localizationService.GetResourceAsync("State.Insert.Null"));

            await _stateRepository.InsertAsync(state);
            return Success(state, await _localizationService.GetResourceAsync("State.Insert.Success"));
        }

        public async Task<ServiceResult<StateProvince>> UpdateAsync(StateProvince state)
        {
            if (state == null)
                return Failed<StateProvince>(await _localizationService.GetResourceAsync("State.Update.Null"));

            await _stateRepository.UpdateAsync(state);
            return Success(state, await _localizationService.GetResourceAsync("State.Update.Success"));
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var state = await _stateRepository.GetByIdAsync(id);
            if (state == null)
                return Failed(await _localizationService.GetResourceAsync("State.NotFound"));

            await _stateRepository.DeleteAsync(state);
            return Success(await _localizationService.GetResourceAsync("State.Delete.Success"));
        }
    }
}
