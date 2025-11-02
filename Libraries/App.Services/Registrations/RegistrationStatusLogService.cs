using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Registrations;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Registrations
{
    public class RegistrationStatusLogService : BaseService, IRegistrationStatusLogService
    {
        private readonly IRepository<RegistrationStatusLog> _statusLogRepository;
        private readonly ILocalizationService _localizationService;

        public RegistrationStatusLogService(
            IRepository<RegistrationStatusLog> statusLogRepository,
            ILocalizationService localizationService)
        {
            _statusLogRepository = statusLogRepository;
            _localizationService = localizationService;
        }

        public async Task<RegistrationStatusLog> GetByIdAsync(int id)
            => await _statusLogRepository.GetByIdAsync(id);

        public async Task<IList<RegistrationStatusLog>> GetByRegistrationIdAsync(int registrationId)
            => await _statusLogRepository.GetAllAsync(q => q.Where(l => l.RegistrationId == registrationId)
                                                            .OrderByDescending(l => l.ActionDateUtc));

        public async Task<ServiceResult<RegistrationStatusLog>> InsertAsync(RegistrationStatusLog log)
        {
            if (log == null)
                return Failed<RegistrationStatusLog>(await _localizationService.GetResourceAsync("StatusLog.Insert.Null"));

            await _statusLogRepository.InsertAsync(log);
            return Success(log, await _localizationService.GetResourceAsync("StatusLog.Insert.Success"));
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var log = await _statusLogRepository.GetByIdAsync(id);
            if (log == null)
                return Failed(await _localizationService.GetResourceAsync("StatusLog.NotFound"));

            await _statusLogRepository.DeleteAsync(log);
            return Success(await _localizationService.GetResourceAsync("StatusLog.Delete.Success"));
        }
    }
}
