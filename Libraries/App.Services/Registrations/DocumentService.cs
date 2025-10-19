using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Registrations
{
    public class DocumentService : IDocumentService
    {
        private readonly IRepository<FIDocument> _documentRepository;
        private readonly IRepository<RegistrationDocument> _registrationDocumentRepository;
        private readonly IRepository<InstituteDocument> _instituteDocumentRepository;
        private readonly ILocalizationService _localizationService;

        public DocumentService(
            IRepository<FIDocument> documentRepository,
            ILocalizationService localizationService,
            IRepository<RegistrationDocument> registrationDocumentRepository,
            IRepository<InstituteDocument> instituteDocumentRepository)
        {
            _documentRepository = documentRepository;
            _localizationService = localizationService;
            _registrationDocumentRepository = registrationDocumentRepository;
            _instituteDocumentRepository = instituteDocumentRepository;

        }

        public async Task<FIDocument> GetByIdAsync(int id)
            => await _documentRepository.GetByIdAsync(id);

        public async Task<IList<RegistrationDocument>> GetRegistrationDocumentsByRegistrationIdAsync(int registrationId)
        {
            return await _registrationDocumentRepository.GetAllAsync(query =>
             query.Where(doc => doc.RegistrationId == registrationId));
        }
        public async Task<IList<InstituteDocument>> GetByInstituteIdAsync(int instituteId)
        {
            return await _instituteDocumentRepository.GetAllAsync(query =>
             query.Where(doc => doc.InstituteId == instituteId));
        }

        public async Task<FIDocument> InsertAsync(FIDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Document.Insert.Null"));

            await _documentRepository.InsertAsync(document);
            await _localizationService.GetResourceAsync("Document.Insert.Success");
            return document;
        }

        public async Task<FIDocument> UpdateAsync(FIDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Document.Update.Null"));

            await _documentRepository.UpdateAsync(document);
            await _localizationService.GetResourceAsync("Document.Update.Success");
            return document;
        }

        public async Task DeleteAsync(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Document.NotFound"));

            await _documentRepository.DeleteAsync(document);
            await _localizationService.GetResourceAsync("Document.Delete.Success");
        }

        public async Task<IList<FIDocument>> GetAllAsync()
        {
            return await _documentRepository.GetAllAsync(query =>
                query.OrderBy(doc => doc.UploadedOnUtc)
            );
        }

        public async Task<RegistrationDocument> GetRegistrationDocumentByIdAsync(int registrationId)
        {
            if (registrationId == 0 || registrationId == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("RegistrationDocument.NotFound"));
            var regDoc = await _registrationDocumentRepository.GetAllAsync(q =>
            q.Where(rd => rd.RegistrationId == registrationId));
            return regDoc.FirstOrDefault();
        }

        public async Task<RegistrationDocument> InsertAsync(RegistrationDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("RegistrationDocument.Insert.Null"));
            await _registrationDocumentRepository.InsertAsync(document);
            return document;
        }

        public async Task<RegistrationDocument> UpdateAsync(RegistrationDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("RegistrationDocument.Update.Null"));
            await _registrationDocumentRepository.UpdateAsync(document);
            return document;

        }

        public async Task DeleteRegistrationDocumentAsync(int registrationId)
        {
            var reg = await _registrationDocumentRepository.GetAllAsync(q =>
            q.Where(r => r.RegistrationId == registrationId));
            var document = reg.FirstOrDefault();
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("RegistrationDocument.NotFound"));
            await _registrationDocumentRepository.DeleteAsync(document);
        }

        public async Task<InstituteDocument> GetInstituteDocumentByIdAsync(int instituteId)
        {
            if (instituteId == 0 || instituteId == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("InstituteDocument.NotFound"));
            return await _instituteDocumentRepository.GetAllAsync(q =>
            q.Where(r => r.InstituteId == instituteId))
                .ContinueWith(t =>
                t.Result.FirstOrDefault());
        }

        public async Task<IList<FIDocument>> GetDocumentsByInstituteIdAsync(int instituteId)
        {
            if (instituteId == 0)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("InstituteDocument.NotFound"));

            var instituteDocuments = await _instituteDocumentRepository.GetAllAsync(q =>
                q.Where(r => r.InstituteId == instituteId));

            var documentIds = instituteDocuments.Select(r => r.DocumentId).ToList();

            if (!documentIds.Any())
                return new List<FIDocument>();

            var documents = await _documentRepository.GetAllAsync(q =>
                q.Where(d => documentIds.Contains(d.Id)));

            return documents.ToList();
        }

        public async Task<InstituteDocument> InsertAsync(InstituteDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("InstituteDocument.Insert.Null"));
            await _instituteDocumentRepository.InsertAsync(document);
            await _localizationService.GetResourceAsync("InstituteDocument.Insert.Success");
            return document;
        }

        public async Task<InstituteDocument> UpdateAsync(InstituteDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("InstituteDocument.Update.Null"));
            await _instituteDocumentRepository.UpdateAsync(document);
            await _localizationService.GetResourceAsync("InstituteDocument.Update.Success");
            return document;
        }

        public async Task DeleteInstituteDocumentAsync(int id)
        {
            if (id == 0)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("InstituteDocument.NotFound"));
            var doc = await _instituteDocumentRepository.GetAllAsync(q =>
            q.Where(r => r.InstituteId == id));
            var document = doc.FirstOrDefault();
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("InstituteDocument.NotFound"));
            await _instituteDocumentRepository.DeleteAsync(document);
            await _localizationService.GetResourceAsync("InstituteDocument.Delete.Success");

        }

        public async Task AddDocumentToInstituteAsync(int instituteId, FIDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Document.Insert.Null"));

            await _documentRepository.InsertAsync(document);

            var instituteDoc = new InstituteDocument
            {
                InstituteId = instituteId,
                DocumentId = document.Id
            };
            await _instituteDocumentRepository.InsertAsync(instituteDoc);
        }

        public async Task AddDocumentToRegistrationAsync(int registrationId, FIDocument document)
        {
            if (document == null)
                throw new KeyNotFoundException(await _localizationService.GetResourceAsync("Document.Insert.Null"));

            // 1) إدخال المستند نفسه
            await _documentRepository.InsertAsync(document);

            // 2) ربطه بالريجستريشن
            var regDoc = new RegistrationDocument
            {
                RegistrationId = registrationId,
                DocumentId = document.Id
            };
            await _registrationDocumentRepository.InsertAsync(regDoc);
        }

        public async Task<IList<FIDocument>> GetDocumentsByIdsAsync(int id)
        {
            return await _documentRepository.GetAllAsync(q => q.Where(d => d.Id == id) );

		}
    }
}
