using System;
using System.Threading.Tasks;
using App.Core.Domain.Audit;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Audit
{
    public class AuditTrailService : IAuditTrailService
    {
        private readonly IRepository<AuditTrail> _auditRepository;
        private readonly IRepository<UserAuditTrail> _userAuditRepository;
        private readonly ILocalizationService _localizationService;

        public AuditTrailService(IRepository<AuditTrail> auditRepository, ILocalizationService localizationService, IRepository<UserAuditTrail> userAuditRepository)
        {
            _auditRepository = auditRepository;
            _localizationService = localizationService;
            _userAuditRepository = userAuditRepository;

        }
        public async Task<IList<AuditTrail>> GetAllAsync()
        {
            return await _auditRepository.GetAllAsync(q => q.OrderByDescending(x => x.ChangedOnUtc));
        }

        public async Task LogAsync(AuditTrail entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (entry.ChangedOnUtc == default)
                entry.ChangedOnUtc = DateTime.UtcNow;

            await _auditRepository.InsertAsync(entry);
            await _localizationService.GetResourceAsync("Audit.Log.Success");
        }

        public async Task LogCreateAsync(string entityName, int entityId, int userId, string comment = null)
        {
            var entry = new AuditTrail
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = AuditActionType.Create,
                ActionId = (int)AuditActionType.Create,
                ChangedOnUtc = DateTime.UtcNow,
                Comment = comment
            };

            await LogAsync(entry);
        }
        public async Task LogCreateAsync(string entityName, int entityId)
        {
            var entry = new AuditTrail
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = AuditActionType.Create,
                ActionId = (int)AuditActionType.Create,
                ChangedOnUtc = DateTime.UtcNow,
            };

            await LogAsync(entry);
        }

        public async Task LogUpdateAsync(string entityName, int entityId, int userId, string field = null, string oldValue = null, string newValue = null, string comment = null)
        {
            var entry = new AuditTrail
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = AuditActionType.Update,
                ActionId = (int)AuditActionType.Update,
                ChangedOnUtc = DateTime.UtcNow,
                FieldName = field,
                OldValue = oldValue,
                NewValue = newValue,
                Comment = comment
            };

            await LogAsync(entry);
        }

        public async Task LogDeleteAsync(string entityName, int entityId, int userId, string comment = null)
        {
            var entry = new AuditTrail
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = AuditActionType.Delete,
                ActionId = (int)AuditActionType.Delete,
                ChangedOnUtc = DateTime.UtcNow,
                Comment = comment
            };

            await LogAsync(entry);
        }

        public async Task<PagedResult<AuditTrail>> SearchAsync(
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            int? changedByUserId = null,
            string entityName = null,
            int? entityId = null,
            string fieldName = null,
            AuditActionType? action = null,
            int pageIndex = 0,
            int pageSize = 50)
        {
            var query = _auditRepository.Table;

            var userAuditQuery = _userAuditRepository.Table;
            if (changedByUserId.HasValue)
                userAuditQuery = userAuditQuery.Where(x => x.UserId == changedByUserId.Value);

            if (fromUtc.HasValue)
                query = query.Where(x => x.ChangedOnUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                query = query.Where(x => x.ChangedOnUtc <= toUtc.Value);

            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(x => x.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(x => x.EntityId == entityId.Value);

            if (!string.IsNullOrWhiteSpace(fieldName))
                query = query.Where(x => x.FieldName == fieldName);

            if (action.HasValue)
                query = query.Where(x => x.ActionId == (int)action.Value);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.ChangedOnUtc)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(new PagedResult<AuditTrail>(items, totalCount, pageIndex, pageSize));
        }
        public async Task LogChangeAsync(string entityName, int entityId, string fieldName, string oldValue, string newValue, int userId)
        {
            var entry = new AuditTrail
            {
                EntityName = entityName,
                EntityId = entityId,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ChangedOnUtc = DateTime.UtcNow,
            };

            await _auditRepository.InsertAsync(entry);
        }

        public async Task<IList<UserAuditTrail>> GetAllUserAuditTrailsAsync()
        {
            var list = await _userAuditRepository.GetAllAsync(q => q.OrderBy(x => x.AuditTrailId));
            return list;
        }

        public async Task<UserAuditTrail> GetUserAuditTrailByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            return await _userAuditRepository.GetAllAsync(q => q.Where(x => x.AuditTrailId == id)).ContinueWith(t => t.Result.FirstOrDefault());
        }

        public async Task InsertUserAuditTrailAsync(UserAuditTrail entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            await _userAuditRepository.InsertAsync(entry);
            await _localizationService.GetResourceAsync("UserAudit.Insert.Success");
        }

        public async Task UpdateUserAuditTrailAsync(UserAuditTrail entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));
            await _userAuditRepository.UpdateAsync(entry);
            await _localizationService.GetResourceAsync("UserAudit.Update.Success");
        }

        public Task DeleteUserAuditTrailAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            var model = _userAuditRepository.GetByIdAsync(id).Result;
            return _userAuditRepository.DeleteAsync(model);
        }

        public async Task<AuditTrail> InsertAuditTrailAsync(AuditTrail auditTrail)
        {
            if (auditTrail == null)
                throw new ArgumentNullException(nameof(auditTrail));
            await _auditRepository.InsertAsync(auditTrail);
            await _localizationService.GetResourceAsync("Audit.Insert.Success");
            return auditTrail;
        }
    }
}
