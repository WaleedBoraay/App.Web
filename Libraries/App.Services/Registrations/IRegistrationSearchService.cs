using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Services.Common;
using System;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public interface IRegistrationSearchService
    {
        Task<PagedResult<Registration>> SearchAsync(
            string institutionName = null,
            string licenseNumber = null,
            int? countryId = null,
            RegistrationStatus? status = null,
            LicenseType? licenseType = null,
            LicenseSector? licenseSector = null,
            FinancialDomain? financialDomain = null,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
        );
    }
}
