using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Services.Common;

namespace App.Services.Registrations
{
    public interface IRegistrationStatusLogService
    {
        Task<RegistrationStatusLog> GetByIdAsync(int id);
        Task<IList<RegistrationStatusLog>> GetByRegistrationIdAsync(int registrationId);
        Task<ServiceResult<RegistrationStatusLog>> InsertAsync(RegistrationStatusLog log);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
