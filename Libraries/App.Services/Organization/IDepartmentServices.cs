using App.Core.Domain.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Organization
{
    public interface IDepartmentServices
    {
        // Department CRUD
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<IList<Department>> GetAllDepartmentsAsync();
        Task<Department> CreateDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(Department department);

		//Get Departments by Sector
        Task<IList<Department>> GetDepartmentsBySectorIdAsync(int sectorId);


	}
}
