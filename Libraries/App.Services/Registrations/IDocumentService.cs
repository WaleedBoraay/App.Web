using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Services.Common;

namespace App.Services.Registrations
{
    public interface IDocumentService
    {
        Task<IList<FIDocument>> GetAllAsync();
        Task<FIDocument> GetByIdAsync(int id);
        Task<IList<FIDocument>> GetDocumentsByIdsAsync(int id);
        Task<FIDocument> InsertAsync(FIDocument document);
        Task<FIDocument> UpdateAsync(FIDocument document);
        Task DeleteAsync(int id);

        //RegistrationDocument CRUD operations
        Task<RegistrationDocument> GetRegistrationDocumentByIdAsync(int registrationId);
        Task<IList<RegistrationDocument>> GetRegistrationDocumentsByRegistrationIdAsync(int registrationId);
        Task<RegistrationDocument> InsertAsync(RegistrationDocument document);
        Task<RegistrationDocument> UpdateAsync(RegistrationDocument document);
        Task DeleteRegistrationDocumentAsync(int registrationId);

        //InstituteDocument CRUD operations
        Task<InstituteDocument> GetInstituteDocumentByIdAsync(int instituteId);
        Task<IList<FIDocument>> GetDocumentsByInstituteIdAsync(int instituteId);
        Task<InstituteDocument> InsertAsync(InstituteDocument document);
        Task<InstituteDocument> UpdateAsync(InstituteDocument document);
        Task DeleteInstituteDocumentAsync(int id);

        

        Task AddDocumentToInstituteAsync(int instituteId, FIDocument document);
        Task AddDocumentToRegistrationAsync(int registrationId, FIDocument document);

    }
}
