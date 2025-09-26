using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Core.Domain.Common;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Templates
{
    public class TemplateService : BaseService, ITemplateService
    {
        private readonly IRepository<Template> _templateRepository;
        private readonly ILocalizationService _localizationService;

        public TemplateService(IRepository<Template> templateRepository, ILocalizationService localizationService)
        {
            _templateRepository = templateRepository;
            _localizationService = localizationService;
        }

        public async Task<ServiceResult<Template>> GetAsync(int id)
        {
            var entity = await _templateRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult<Template>.Failed(await _localizationService.GetResourceAsync("Template.NotFound"));
            return ServiceResult<Template>.Success(entity);
        }

        public async Task<ServiceResult<IReadOnlyList<Template>>> GetAllAsync(string type = null)
        {
            var list = await _templateRepository.GetAllAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.TemplateType == type);
                return q;
            });
            return ServiceResult<IReadOnlyList<Template>>.Success(list.ToList());
        }

        public async Task<ServiceResult<Template>> CreateAsync(Template model)
        {
            if (model == null) return ServiceResult<Template>.Failed(await _localizationService.GetResourceAsync("Errors.NullModel"));
            await _templateRepository.InsertAsync(model);
            return ServiceResult<Template>.Success(model);
        }

        public async Task<ServiceResult> UpdateAsync(int id, Template model)
        {
            var entity = await _templateRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("Template.NotFound"));
            entity.Name = model.Name;
            entity.TemplateType = model.TemplateType;
            entity.Content = model.Content;
            await _templateRepository.UpdateAsync(entity);
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var entity = await _templateRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("Template.NotFound"));
            await _templateRepository.DeleteAsync(entity);
            return ServiceResult.SuccessResult();
        }
    }
}
