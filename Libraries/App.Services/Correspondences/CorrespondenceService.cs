using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Core.Domain.Correspondences;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Correspondences
{
    public class CorrespondenceService : BaseService, ICorrespondenceService
    {
        private readonly IRepository<Correspondence> _correspondenceRepository;
        private readonly ILocalizationService _localizationService;

        public CorrespondenceService(IRepository<Correspondence> correspondenceRepository, ILocalizationService localizationService)
        {
            _correspondenceRepository = correspondenceRepository;
            _localizationService = localizationService;
        }

        public async Task<ServiceResult<Correspondence>> GetAsync(int id)
        {
            var entity = await _correspondenceRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult<Correspondence>.Failed(await _localizationService.GetResourceAsync("Correspondence.NotFound"));
            return ServiceResult<Correspondence>.Success(entity);
        }

        public async Task<ServiceResult<IReadOnlyList<Correspondence>>> InboxAsync(int userId)
        {
            var list = await _correspondenceRepository.GetAllAsync(q => q.Where(x => x.RecipientUserId == userId));
            return ServiceResult<IReadOnlyList<Correspondence>>.Success(list.ToList());
        }

        public async Task<ServiceResult<IReadOnlyList<Correspondence>>> OutboxAsync(int userId)
        {
            var list = await _correspondenceRepository.GetAllAsync(q => q.Where(x => x.SenderUserId == userId));
            return ServiceResult<IReadOnlyList<Correspondence>>.Success(list.ToList());
        }

        public async Task<ServiceResult<Correspondence>> SendAsync(Correspondence model)
        {
            if (model == null) return ServiceResult<Correspondence>.Failed(await _localizationService.GetResourceAsync("Errors.NullModel"));
            await _correspondenceRepository.InsertAsync(model);
            return ServiceResult<Correspondence>.Success(model);
        }

        public async Task<ServiceResult> MarkAsAsync(int id, string status)
        {
            var entity = await _correspondenceRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("Correspondence.NotFound"));
            entity.Status = status;
            await _correspondenceRepository.UpdateAsync(entity);
            return ServiceResult.SuccessResult();
        }
    }
}
