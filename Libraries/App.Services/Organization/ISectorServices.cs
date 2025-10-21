using App.Core.Domain.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Organization
{
    public interface ISectorServices
    {
        // Sector CRUD
        Task<Sector> GetSectorByIdAsync(int id);
        Task<IList<Sector>> GetAllSectorsAsync();
        Task<Sector> CreateSectorAsync(Sector sector);
        Task UpdateSectorAsync(Sector sector);
        Task DeleteSectorAsync(Sector sector);

		//Get Departments by Sector Id
        Task<IList<Department>> GetDepartmentsBySectorIdAsync(int sectorId);
	}
}
