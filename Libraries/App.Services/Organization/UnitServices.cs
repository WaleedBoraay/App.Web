using App.Core.Domain.Organization;
using App.Core.RepositoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Organization
{
    public class UnitServices : IUnitServices
    {
		#region Fields
		protected readonly IRepository<Unit> _unitRepository;
		#endregion

		#region Ctor
		public UnitServices(
            IRepository<Unit> unitRepository
			)
        {
            _unitRepository = unitRepository;
		}
		#endregion

		#region Methods
		public async Task<Unit> CreateUnitAsync(Unit unit)
		{
			await _unitRepository.InsertAsync(unit);
			return unit;
		}
		public async Task DeleteUnitAsync(Unit unit)
		{
			await _unitRepository.DeleteAsync(unit);
		}
		public async Task<IList<Unit>> GetAllUnitsAsync()
		{
			return await _unitRepository.GetAllAsync(query
				=> query.OrderBy(u => u.Name));
		}
		public async Task<Unit> GetUnitByIdAsync(int id)
		{
			return await _unitRepository.GetByIdAsync(id);
		}

        public Task<IList<Unit>> GetUnitsByDepartmentAsync(Department department)
        {
			return _unitRepository.GetAllAsync(query
				=> query.Where(u => u.DepartmentId == department.Id)
						 .OrderBy(u => u.Name));

		}

        public Task<IList<Unit>> GetUnitsByDepartmentIdAsync(int departmentId)
        {
			return _unitRepository.GetAllAsync(query
				=> query.Where(u => u.DepartmentId == departmentId)
						 .OrderBy(u => u.Name));
		}

        public async Task UpdateUnitAsync(Unit unit)
		{
			await _unitRepository.UpdateAsync(unit);
		}
		#endregion
	}
}
