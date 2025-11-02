using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Registrations
{
    public class ContactService : IContactService
    {
        private readonly IRepository<Contact> _contactRepository;
        private readonly ILocalizationService _localizationService;

        public ContactService(
            IRepository<Contact> contactRepository,
            ILocalizationService localizationService)
        {
            _contactRepository = contactRepository;
            _localizationService = localizationService;
        }

        public async Task<Contact> GetByIdAsync(int id)
            => await _contactRepository.GetByIdAsync(id);

        public async Task<IList<Contact>> GetByRegistrationIdAsync(int registrationId)
            => await _contactRepository.GetAllAsync(q => q.Where(c => c.RegistrationId == registrationId));

        public async Task<Contact> InsertAsync(Contact contact)
        {
            if (contact == null)
                throw new System.ArgumentNullException(nameof(contact), await _localizationService.GetResourceAsync("Contact.Insert.Null"));

            await _contactRepository.InsertAsync(contact);
            await _localizationService.GetResourceAsync("Contact.Insert.Success");
            return contact;
        }

        public async Task<Contact> UpdateAsync(Contact contact)
        {
            if (contact == null)
                throw new System.ArgumentNullException(nameof(contact), await _localizationService.GetResourceAsync("Contact.Update.Null"));
            await _contactRepository.UpdateAsync(contact);
            await _localizationService.GetResourceAsync("Contact.Update.Success");
            return contact;
        }

        public async Task DeleteAsync(int id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            if (contact == null)
                throw new System.ArgumentNullException(nameof(contact), await _localizationService.GetResourceAsync("Contact.NotFound"));

            await _contactRepository.DeleteAsync(contact);
            throw new System.ArgumentNullException(nameof(contact), await _localizationService.GetResourceAsync("Contact.Delete.Success"));
        }

        public async Task<IList<Contact>> GetAllAsync()
        {
            return await _contactRepository.GetAllAsync(query =>
                query.OrderBy(c => c.LastName).ThenBy(c => c.FirstName));
		}
    }
}
