using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Services.Common;

namespace App.Services.Registrations
{
    public interface IRegistrationStatusLogService
    {
        Task<FIRegistrationStatusLog> GetByIdAsync(int id);
        Task<IList<FIRegistrationStatusLog>> GetByRegistrationIdAsync(int registrationId);
        Task<ServiceResult<FIRegistrationStatusLog>> InsertAsync(FIRegistrationStatusLog log);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
