using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Services.Common;

namespace App.Services.Registrations
{
    public interface IContactService
    {
        Task<FIContact> GetByIdAsync(int id);
        Task<IList<FIContact>> GetByRegistrationIdAsync(int registrationId);
        Task<FIContact> InsertAsync(FIContact contact);
        Task<FIContact> UpdateAsync(FIContact contact);
        Task DeleteAsync(int id);
    }
}
