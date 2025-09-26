using App.Core.Domain.Organization;
using App.Core.RepositoryServices;
using App.Services.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Organization
{
    public class OrganizationsServices : IOrganizationsServices
    {
        private readonly IRepository<Department> _departments;
        private readonly IRepository<Unit> _units;
        private readonly IRepository<SubUnit> _subUnits;
        private readonly IUserService _userService;
        public OrganizationsServices(
            IRepository<Department> departments,
            IRepository<Unit> units,
            IRepository<SubUnit> subUnits,
            IUserService userService)
        {
            _departments = departments;
            _units = units;
            _subUnits = subUnits;
            _userService = userService;
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            await _departments.InsertAsync(department);
            return department;
        }

        public async Task<SubUnit> CreateSubUnitAsync(SubUnit subUnit)
        {
            await _subUnits.InsertAsync(subUnit);
            return subUnit;
        }

        public async Task<Unit> CreateUnitAsync(Unit unit)
        {
            await _units.InsertAsync(unit);
            return unit;
        }

        public async Task DeleteDepartmentAsync(Department department)
        {
            await _departments.DeleteAsync(department);

        }

        public async Task DeleteSubUnitAsync(SubUnit subUnit)
        {
            await _subUnits.DeleteAsync(subUnit);
        }

        public async Task DeleteUnitAsync(Unit unit)
        {
            await _units.DeleteAsync(unit);
        }

        public async Task<IList<Department>> GetAllDepartmentsAsync()
        {
            return await _departments.GetAllAsync(query =>
            query.OrderBy(d => d.Name));
        }

        public async Task<IList<Department>> GetAllDepartmentsWithUsersAsync()
        {
            return await _departments.GetAllAsync(query =>
            query.OrderBy(d => d.Name));
        }

        public async Task<IList<SubUnit>> GetAllSubUnitsAsync()
        {
            return await _subUnits.GetAllAsync(query
                => query.OrderBy(sb => sb.Name));
        }

        public Task<IList<Unit>> GetAllUnitsAsync()
        {
            return _units.GetAllAsync(query
                => query.OrderBy(u => u.Name));
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            return await _departments.GetByIdAsync(id);
        }

        public async Task<Department> GetDepartmentWithUsersByIdAsync(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            var department = await _departments.GetByIdAsync(user.DepartmentId ?? 0);
            return department;

        }

        public Task<SubUnit> GetSubUnitByIdAsync(int id)
        {
            return _subUnits.GetByIdAsync(id);
        }

        public async Task<IList<SubUnit>> GetSubUnitsByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.Unit.DepartmentId == departmentId)
                .OrderBy(su => su.Name));
        }

        public async Task<IList<SubUnit>> GetSubUnitsByUnitIdAsync(int unitId)
        {
            var unit = await _units.GetByIdAsync(unitId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.UnitId == unitId)
                .OrderBy(su => su.Name));
        }

        public async Task<IList<SubUnit>> GetSubUnitsWithUsersByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.Unit.DepartmentId == departmentId)
                .OrderBy(su => su.Name));
        }

        public async Task<IList<SubUnit>> GetSubUnitsWithUsersByUnitIdAsync(int unitId)
        {
            var unit = await _units.GetByIdAsync(unitId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.UnitId == unitId)
                .OrderBy(su => su.Name));
        }

        public async Task<SubUnit> GetSubUnitWithUsersByIdAsync(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return await _subUnits.GetByIdAsync(user.SubUnitId ?? 0);
        }

        public async Task<Unit> GetUnitByIdAsync(int id)
        {
            return await _units.GetByIdAsync(id);
        }

        public async Task<IList<Unit>> GetUnitsByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _units.GetAllAsync(query
                => query.Where(u => u.DepartmentId == departmentId)
                .OrderBy(u => u.Name));
        }

        public async Task<IList<Unit>> GetUnitsWithUsersByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _units.GetAllAsync(query
                => query.Where(u => u.DepartmentId == departmentId)
                .OrderBy(u => u.Name));
        }

        public async Task<Unit> GetUnitWithUsersByIdAsync(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return await _units.GetByIdAsync(user.UnitId ?? 0);
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            await _departments.UpdateAsync(department);
        }

        public async Task UpdateSubUnitAsync(SubUnit subUnit)
        {
            await _subUnits.UpdateAsync(subUnit);
        }

        public async Task UpdateUnitAsync(Unit unit)
        {
            await _units.UpdateAsync(unit);
        }
    }
}
