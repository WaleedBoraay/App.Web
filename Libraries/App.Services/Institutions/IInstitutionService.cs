using App.Core.Domain.Institutions;
using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Services.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Institutions
{
    public interface IInstitutionService
    {
        // CRUD
        Task<Institution> CreateAsync(Institution institute, string email);
        Task<Institution> GetByIdAsync(int id);
        Task<IList<Institution>> GetAllAsync();
        Task<Institution> InsertAsync(Institution institution);
        Task<Institution> UpdateAsync(Institution institution);
        Task DeleteAsync(int id);

        // Checks
        Task<bool> ExistsAsync(string institutionName, string licenseNumber);
        Task<bool> IsValidForRegistrationAsync(int institutionId);

        // Relations
        Task<IList<Institution>> GetAllInstitutionsWithRegistrationAndBranchesAsync();
        Task<IList<Registration>> GetRegistrationsAsync(int institutionId);
        Task<Registration> GetLatestRegistrationAsync(int institutionId);

        // Search
        Task<PagedResult<Institution>> SearchAsync(
            string name = null,
            string licenseNumber = null,
            int? countryId = null,
            LicenseSector? licenseSector = null,
            FinancialDomain? financialDomain = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
        );

        // Reporting
        Task<IDictionary<LicenseSector, int>> GetInstitutionCountBySectorAsync();
        Task<IDictionary<int, int>> GetInstitutionCountByCountryAsync();
    }
}
