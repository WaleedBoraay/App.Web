using App.Services.Audit;
using App.Services.Users;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Areas.Admin.Models.Audit;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuditTrailController : Controller
    {
        private readonly IAuditTrailService _auditTrailService;
        private readonly IUserService _userService;

        public AuditTrailController(IAuditTrailService auditTrailService, IUserService userService)
        {
            _auditTrailService = auditTrailService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(
            string entityName,
            int? entityId,
            DateTime? fromDate,
            DateTime? toDate,
            int pageIndex = 1,
            int pageSize = 20)
        {
            var logs = await _auditTrailService.GetAllAsync();

            if (!string.IsNullOrEmpty(entityName))
                logs = logs.Where(l => l.EntityName == entityName).ToList();

          if (entityId.HasValue)
                logs = logs.Where(l => l.EntityId == entityId).ToList();

            if (fromDate.HasValue)
                logs = logs.Where(l => l.ChangedOnUtc >= fromDate.Value).ToList();

            if (toDate.HasValue)
                logs = logs.Where(l => l.ChangedOnUtc <= toDate.Value).ToList();

            var users = await _userService.GetAllAsync();
            var enrichedLogs = logs.Select(l => new AuditTrailWithUser
            {
                Id = l.Id,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                FieldName = l.FieldName,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                ChangedOnUtc = l.ChangedOnUtc,
            }).ToList();

            var totalCount = enrichedLogs.Count;
            var pagedLogs = enrichedLogs
                .OrderByDescending(x => x.ChangedOnUtc)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new AuditTrailSearchModel
            {
                EntityName = entityName,
                EntityId = entityId,
                FromDate = fromDate,
                ToDate = toDate,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                Results = pagedLogs
            };

            return View(model);
        }

    }
}
