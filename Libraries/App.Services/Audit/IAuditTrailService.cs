using System.Threading.Tasks;
using App.Core.Domain.Audit;
using App.Services.Common;

namespace App.Services.Audit
{
    public interface IAuditTrailService
    {

        Task<IList<AuditTrail>> GetAllAsync();
        Task LogChangeAsync(string entityName, int entityId, string fieldName, string oldValue, string newValue, int userId);

        Task LogAsync(AuditTrail entry);

        Task LogCreateAsync(string entityName, int entityId, int userId, string comment = null);
        Task LogCreateAsync(string entityName, int entityId);

        Task<AuditTrail> InsertAuditTrailAsync(AuditTrail auditTrail);


        Task LogUpdateAsync(
            string entityName,
            int entityId,
            int userId,
            string field = null,
            string oldValue = null,
            string newValue = null,
            string comment = null);

        Task LogDeleteAsync(string entityName, int entityId, int userId, string comment = null);

        Task<PagedResult<AuditTrail>> SearchAsync(
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            int? changedByUserId = null,
            string entityName = null,
            int? entityId = null,
            string fieldName = null,
            AuditActionType? action = null,
            int pageIndex = 0,
            int pageSize = 50);

        //Add UserAuditTrail CRUD operations
        Task<IList<UserAuditTrail>> GetAllUserAuditTrailsAsync();
        Task<UserAuditTrail> GetUserAuditTrailByIdAsync(int id);
        Task InsertUserAuditTrailAsync(UserAuditTrail entry);
        Task UpdateUserAuditTrailAsync(UserAuditTrail entry);
        Task DeleteUserAuditTrailAsync(int id);

		//Get by registration id
        Task<IList<AuditTrail>> GetUserAuditTrailsByRegistrationIdAsync(int registrationId);
	}
}
