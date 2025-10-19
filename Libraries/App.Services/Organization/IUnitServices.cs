using App.Core.Domain.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Organization
{
    public interface IUnitServices
    {
        // Unit CRUD
        Task<Unit> GetUnitByIdAsync(int id);
        Task<IList<Unit>> GetAllUnitsAsync();
        Task<Unit> CreateUnitAsync(Unit unit);
        Task UpdateUnitAsync(Unit unit);
        Task DeleteUnitAsync(Unit unit);

		//Get Unets by Department
        Task<IList<Unit>> GetUnitsByDepartmentAsync(Department department);

		//Get Units by Department
		Task<IList<Unit>> GetUnitsByDepartmentIdAsync(int departmentId);
	}
}
