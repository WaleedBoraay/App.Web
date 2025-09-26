using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Directory
{
    public class CountryService : BaseService, ICountryService
    {
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateRepository;
        private readonly ILocalizationService _localizationService;

        public CountryService(
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateRepository,
            ILocalizationService localizationService)
        {
            _countryRepository = countryRepository;
            _stateRepository = stateRepository;
            _localizationService = localizationService;
        }

        public async Task<Country> GetByIdAsync(int id)
            => await _countryRepository.GetByIdAsync(id);

        public async Task<IList<Country>> GetAllAsync(bool onlyActive = true)
        {
            var list = await _countryRepository.GetAllAsync(q =>
                onlyActive ? q.Where(c => c.Published) : q);
            return list.OrderBy(c => c.DisplayOrder).ToList();
        }

        public async Task<ServiceResult<Country>> InsertAsync(Country country)
        {
            if (country == null)
                return Failed<Country>(await _localizationService.GetResourceAsync("Country.Insert.Null"));

            if (await ExistsAsync(country.TwoLetterIsoCode))
                return Failed<Country>(await _localizationService.GetResourceAsync("Country.Insert.Duplicate"));

            await _countryRepository.InsertAsync(country);
            return Success(country, await _localizationService.GetResourceAsync("Country.Insert.Success"));
        }

        public async Task<ServiceResult<Country>> UpdateAsync(Country country)
        {
            if (country == null)
                return Failed<Country>(await _localizationService.GetResourceAsync("Country.Update.Null"));

            await _countryRepository.UpdateAsync(country);
            return Success(country, await _localizationService.GetResourceAsync("Country.Update.Success"));
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var country = await _countryRepository.GetByIdAsync(id);
            if (country == null)
                return Failed(await _localizationService.GetResourceAsync("Country.NotFound"));

            await _countryRepository.DeleteAsync(country);
            return Success(await _localizationService.GetResourceAsync("Country.Delete.Success"));
        }

        public async Task<Country> GetByCodeAsync(string twoLetterIsoCode)
        {
            var countries = await _countryRepository.GetAllAsync(q => q.Where(c => c.TwoLetterIsoCode == twoLetterIsoCode));
            return countries.FirstOrDefault();
        }

        public async Task<Country> GetByNameAsync(string name)
        {
            var countries = await _countryRepository.GetAllAsync(q => q.Where(c => c.Name == name));
            return countries.FirstOrDefault();
        }

        public async Task<PagedResult<Country>> SearchAsync(string keyword = null, bool? isActive = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _countryRepository.Table;

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(c => c.Name.Contains(keyword) || c.TwoLetterIsoCode.Contains(keyword));

            if (isActive.HasValue)
                query = query.Where(c => c.Published == isActive.Value);

            var totalCount = query.Count();

            var items = query
                .OrderBy(c => c.DisplayOrder)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(new PagedResult<Country>(items, totalCount, pageIndex, pageSize));
        }

        public async Task<IList<StateProvince>> GetStatesByCountryIdAsync(int countryId)
        {
            var states = await _stateRepository.GetAllAsync(q => q.Where(s => s.CountryId == countryId));
            return states.OrderBy(s => s.DisplayOrder).ToList();
        }

        public async Task<bool> ExistsAsync(string twoLetterIsoCode)
        {
            var exists = await _countryRepository.GetAllAsync(q => q.Where(c => c.TwoLetterIsoCode == twoLetterIsoCode));
            return exists.Any();
        }
    }
}
