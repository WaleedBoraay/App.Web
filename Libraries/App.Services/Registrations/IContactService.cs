using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Services.Common;

namespace App.Services.Registrations
{
    public interface IContactService
    {
        Task<Contact> GetByIdAsync(int id);
        Task<IList<Contact>> GetAllAsync();
		Task<IList<Contact>> GetByRegistrationIdAsync(int registrationId);
        Task<Contact> InsertAsync(Contact contact);
        Task<Contact> UpdateAsync(Contact contact);
        Task DeleteAsync(int id);
    }
}
