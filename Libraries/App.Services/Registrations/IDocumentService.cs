using App.Core.Domain.Registrations;
using App.Services.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public interface IDocumentService
    {
        Task<IList<Document>> GetAllAsync();
        Task<Document> GetByIdAsync(int id);
        Task<IList<Document>> GetDocumentsByIdsAsync(int id);
        Task<Document> InsertAsync(Document document);
        Task<Document> UpdateAsync(Document document);
        Task DeleteAsync(int id);

        //RegistrationDocument CRUD operations
        Task<RegistrationDocument> GetRegistrationDocumentByIdAsync(int registrationId);
        Task<IList<RegistrationDocument>> GetRegistrationDocumentsByRegistrationIdAsync(int registrationId);
        Task<RegistrationDocument> InsertAsync(RegistrationDocument document);
        Task<RegistrationDocument> UpdateAsync(RegistrationDocument document);
        Task DeleteRegistrationDocumentAsync(int registrationId);

        //InstituteDocument CRUD operations
        Task<InstituteDocument> GetInstituteDocumentByIdAsync(int instituteId);
        Task<IList<Document>> GetDocumentsByInstituteIdAsync(int instituteId);
        Task<InstituteDocument> InsertAsync(InstituteDocument document);
        Task<InstituteDocument> UpdateAsync(InstituteDocument document);
        Task DeleteInstituteDocumentAsync(int id);    

        Task AddDocumentToInstituteAsync(int instituteId, Document document);
        Task AddDocumentToRegistrationAsync(int registrationId, Document document);

		//upload document
        Task<Document> UploadDocumentAsync(Document document, IFormFile file);

	}
}
