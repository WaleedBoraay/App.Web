using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Organization;

namespace App.Services.Organization
{
    public interface IOrganizationsServices
    {
        // Department CRUD
        Task<Sector> GetDepartmentByIdAsync(int id);
        Task<IList<Sector>> GetAllDepartmentsAsync();
        Task<Sector> CreateDepartmentAsync(Sector department);
        Task UpdateDepartmentAsync(Sector department);
        Task DeleteDepartmentAsync(Sector department);
        // Department With Users CRUD
        Task<Sector> GetDepartmentWithUsersByIdAsync(int id);
        Task<IList<Sector>> GetAllDepartmentsWithUsersAsync();

        // Unit CRUD
        Task<Department> GetUnitByIdAsync(int id);
        Task<IList<Department>> GetAllUnitsAsync();
        Task<Department> CreateUnitAsync(Department unit);
        Task UpdateUnitAsync(Department unit);
        Task DeleteUnitAsync(Department unit);
        // Unit With Users
        Task<Department> GetUnitWithUsersByIdAsync(int id);
        Task<IList<Department>> GetUnitsByDepartmentIdAsync(int departmentId);
        Task<IList<Department>> GetUnitsWithUsersByDepartmentIdAsync(int departmentId);
        Task<IList<Unit>> GetSubUnitsByUnitIdAsync(int unitId);

        // SubUnit CRUD
        Task<Unit> GetSubUnitByIdAsync(int id);
        Task<IList<Unit>> GetAllSubUnitsAsync();
        Task<Unit> CreateSubUnitAsync(Unit subUnit);
        Task UpdateSubUnitAsync(Unit subUnit);
        Task DeleteSubUnitAsync(Unit subUnit);
        // SubUnit With Users
        Task<Unit> GetSubUnitWithUsersByIdAsync(int id);
        Task<IList<Unit>> GetSubUnitsWithUsersByUnitIdAsync(int unitId);
        Task<IList<Unit>> GetSubUnitsWithUsersByDepartmentIdAsync(int departmentId);
        Task<IList<Unit>> GetSubUnitsByDepartmentIdAsync(int departmentId);
    }
}
