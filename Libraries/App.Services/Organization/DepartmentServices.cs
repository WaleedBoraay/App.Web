using App.Core.Domain.Organization;
using App.Core.RepositoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Organization
{
    public class DepartmentServices : IDepartmentServices
	{
		#region Fields
		protected readonly IRepository<Department> _departmentRepository;
		#endregion

		#region Ctor
		public DepartmentServices(IRepository<Department> departmentRepository)
		{
			_departmentRepository = departmentRepository;
		}
		#endregion

		#region Methods
		public async Task<Department> CreateDepartmentAsync(Department department)
		{
			await _departmentRepository.InsertAsync(department);
			return department;
		}
		public async Task DeleteDepartmentAsync(Department department)
		{
			await _departmentRepository.DeleteAsync(department);
		}

        public async Task<IList<Department>> GetAllDepartmentsAsync()
        {
			return await _departmentRepository.GetAllAsync( query =>
			{
				return query.OrderBy(d => d.Name);
			});
		}

        public Task<Department> GetDepartmentByIdAsync(int id)
        { 
			return _departmentRepository.GetByIdAsync(id);
		}

        public Task<IList<Department>> GetDepartmentsBySectorIdAsync(int sectorId)
        {
			return _departmentRepository.GetAllAsync(query =>
			query.Where(d => d.SectorId == sectorId)
			);

		}

        public Task UpdateDepartmentAsync(Department department)
        {
			return _departmentRepository.UpdateAsync(department);
		}
		#endregion
	}
}
