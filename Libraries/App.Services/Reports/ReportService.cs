using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Core.Domain.Reports;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Reports
{
    public class ReportService : BaseService, IReportService
    {
        private readonly IRepository<Report> _reportRepository;
        private readonly ILocalizationService _localizationService;

        public ReportService(IRepository<Report> reportRepository, ILocalizationService localizationService)
        {
            _reportRepository = reportRepository;
            _localizationService = localizationService;
        }

        public async Task<ServiceResult<Report>> GetAsync(int id)
        {
            var entity = await _reportRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult<Report>.Failed(await _localizationService.GetResourceAsync("Report.NotFound"));
            return ServiceResult<Report>.Success(entity);
        }

        public async Task<ServiceResult<IReadOnlyList<Report>>> GetAllAsync(string type = null)
        {
            var list = await _reportRepository.GetAllAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(type)) q = q.Where(x => x.ReportType == type);
                return q;
            });
            return ServiceResult<IReadOnlyList<Report>>.Success(list.ToList());
        }

        public async Task<ServiceResult<Report>> CreateAsync(Report model)
        {
            if (model == null) return ServiceResult<Report>.Failed(await _localizationService.GetResourceAsync("Errors.NullModel"));
            await _reportRepository.InsertAsync(model);
            return ServiceResult<Report>.Success(model);
        }

        public async Task<ServiceResult> UpdateAsync(int id, Report model)
        {
            var entity = await _reportRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("Report.NotFound"));
            entity.Title = model.Title;
            entity.ReportType = model.ReportType;
            entity.FilePath = model.FilePath;
            await _reportRepository.UpdateAsync(entity);
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var entity = await _reportRepository.GetByIdAsync(id);
            if (entity == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("Report.NotFound"));
            await _reportRepository.DeleteAsync(entity);
            return ServiceResult.SuccessResult();
        }
    }
}
