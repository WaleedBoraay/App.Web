using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Organization;

namespace App.Services.Organization
{
    public interface IOrganizationsServices
    {
        // Department CRUD
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<IList<Department>> GetAllDepartmentsAsync();
        Task<Department> CreateDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(Department department);
        Task DeleteDepartmentAsync(Department department);
        // Department With Users CRUD
        Task<Department> GetDepartmentWithUsersByIdAsync(int id);
        Task<IList<Department>> GetAllDepartmentsWithUsersAsync();

        // Unit CRUD
        Task<Unit> GetUnitByIdAsync(int id);
        Task<IList<Unit>> GetAllUnitsAsync();
        Task<Unit> CreateUnitAsync(Unit unit);
        Task UpdateUnitAsync(Unit unit);
        Task DeleteUnitAsync(Unit unit);
        // Unit With Users
        Task<Unit> GetUnitWithUsersByIdAsync(int id);
        Task<IList<Unit>> GetUnitsByDepartmentIdAsync(int departmentId);
        Task<IList<Unit>> GetUnitsWithUsersByDepartmentIdAsync(int departmentId);
        Task<IList<SubUnit>> GetSubUnitsByUnitIdAsync(int unitId);

        // SubUnit CRUD
        Task<SubUnit> GetSubUnitByIdAsync(int id);
        Task<IList<SubUnit>> GetAllSubUnitsAsync();
        Task<SubUnit> CreateSubUnitAsync(SubUnit subUnit);
        Task UpdateSubUnitAsync(SubUnit subUnit);
        Task DeleteSubUnitAsync(SubUnit subUnit);
        // SubUnit With Users
        Task<SubUnit> GetSubUnitWithUsersByIdAsync(int id);
        Task<IList<SubUnit>> GetSubUnitsWithUsersByUnitIdAsync(int unitId);
        Task<IList<SubUnit>> GetSubUnitsWithUsersByDepartmentIdAsync(int departmentId);
        Task<IList<SubUnit>> GetSubUnitsByDepartmentIdAsync(int departmentId);
    }
}
