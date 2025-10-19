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
        private readonly IRepository<Sector> _departments;
        private readonly IRepository<Department> _units;
        private readonly IRepository<Unit> _subUnits;
        private readonly IUserService _userService;
        public OrganizationsServices(
            IRepository<Sector> departments,
            IRepository<Department> units,
            IRepository<Unit> subUnits,
            IUserService userService)
        {
            _departments = departments;
            _units = units;
            _subUnits = subUnits;
            _userService = userService;
        }

        public async Task<Sector> CreateDepartmentAsync(Sector department)
        {
            await _departments.InsertAsync(department);
            return department;
        }

        public async Task<Unit> CreateSubUnitAsync(Unit subUnit)
        {
            await _subUnits.InsertAsync(subUnit);
            return subUnit;
        }

        public async Task<Department> CreateUnitAsync(Department unit)
        {
            await _units.InsertAsync(unit);
            return unit;
        }

        public async Task DeleteDepartmentAsync(Sector department)
        {
            await _departments.DeleteAsync(department);

        }

        public async Task DeleteSubUnitAsync(Unit subUnit)
        {
            await _subUnits.DeleteAsync(subUnit);
        }

        public async Task DeleteUnitAsync(Department unit)
        {
            await _units.DeleteAsync(unit);
        }

        public async Task<IList<Sector>> GetAllDepartmentsAsync()
        {
            return await _departments.GetAllAsync(query =>
            query.OrderBy(d => d.Name));
        }

        public async Task<IList<Sector>> GetAllDepartmentsWithUsersAsync()
        {
            return await _departments.GetAllAsync(query =>
            query.OrderBy(d => d.Name));
        }

        public async Task<IList<Unit>> GetAllSubUnitsAsync()
        {
            return await _subUnits.GetAllAsync(query
                => query.OrderBy(sb => sb.Name));
        }

        public Task<IList<Department>> GetAllUnitsAsync()
        {
            return _units.GetAllAsync(query
                => query.OrderBy(u => u.Name));
        }

        public async Task<Sector> GetDepartmentByIdAsync(int id)
        {
            return await _departments.GetByIdAsync(id);
        }

        public async Task<Sector> GetDepartmentWithUsersByIdAsync(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            var department = await _departments.GetByIdAsync(user.DepartmentId ?? 0);
            return department;

        }

        public Task<Unit> GetSubUnitByIdAsync(int id)
        {
            return _subUnits.GetByIdAsync(id);
        }

        public async Task<IList<Unit>> GetSubUnitsByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.Department.SectorId == departmentId)
                .OrderBy(su => su.Name));
        }

        public async Task<IList<Unit>> GetSubUnitsByUnitIdAsync(int unitId)
        {
            var unit = await _units.GetByIdAsync(unitId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.DepartmentId == unitId)
                .OrderBy(su => su.Name));
        }

        public async Task<IList<Unit>> GetSubUnitsWithUsersByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.Department.SectorId == departmentId)
                .OrderBy(su => su.Name));
        }

        public async Task<IList<Unit>> GetSubUnitsWithUsersByUnitIdAsync(int unitId)
        {
            var unit = await _units.GetByIdAsync(unitId);
            return await _subUnits.GetAllAsync(query
                => query.Where(su => su.DepartmentId == unitId)
                .OrderBy(su => su.Name));
        }

        public async Task<Unit> GetSubUnitWithUsersByIdAsync(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return await _subUnits.GetByIdAsync(user.UnitId ?? 0);
        }

        public async Task<Department> GetUnitByIdAsync(int id)
        {
            return await _units.GetByIdAsync(id);
        }

        public async Task<IList<Department>> GetUnitsByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _units.GetAllAsync(query
                => query.Where(u => u.SectorId == departmentId)
                .OrderBy(u => u.Name));
        }

        public async Task<IList<Department>> GetUnitsWithUsersByDepartmentIdAsync(int departmentId)
        {
            var department = await _departments.GetByIdAsync(departmentId);
            return await _units.GetAllAsync(query
                => query.Where(u => u.SectorId == departmentId)
                .OrderBy(u => u.Name));
        }

        public async Task<Department> GetUnitWithUsersByIdAsync(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return await _units.GetByIdAsync(user.UnitId ?? 0);
        }

        public async Task UpdateDepartmentAsync(Sector department)
        {
            await _departments.UpdateAsync(department);
        }

        public async Task UpdateSubUnitAsync(Unit subUnit)
        {
            await _subUnits.UpdateAsync(subUnit);
        }

        public async Task UpdateUnitAsync(Department unit)
        {
            await _units.UpdateAsync(unit);
        }
    }
}
