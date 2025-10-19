using App.Core.Domain.Organization;
using App.Core.RepositoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Organization
{
    public class SectorServices : ISectorServices
	{
		#region Fields
		protected readonly IRepository<Sector> _sectorRepository;
		#endregion

		#region Ctor
		public SectorServices(IRepository<Sector> sectorRepository)
		{
			_sectorRepository = sectorRepository;
		}
		#endregion
		#region Methods
		public async Task<Sector> CreateSectorAsync(Sector sector)
		{
			await _sectorRepository.InsertAsync(sector);
			return sector;
		}
		public async Task DeleteSectorAsync(Sector sector)
		{
			await _sectorRepository.DeleteAsync(sector);
		}

        public Task<IList<Sector>> GetAllSectorsAsync()
        {
			return _sectorRepository.GetAllAsync(query =>
				query.OrderBy(s => s.Name));
		}

        public async Task<Sector> GetSectorByIdAsync(int id)
		{
			return await _sectorRepository.GetByIdAsync(id);
		}
		public async Task UpdateSectorAsync(Sector sector)
		{
			await _sectorRepository.UpdateAsync(sector);
		}
		#endregion
	}
}
