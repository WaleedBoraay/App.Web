using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Services.Audit;
using App.Core.Domain.Admin.Models.Audit;
using App.Core.Domain.Audit;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminAuditController : Controller
    {
        private readonly IAuditTrailService _auditTrailService;

        public AdminAuditController(IAuditTrailService auditTrailService)
        {
            _auditTrailService = auditTrailService;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] AuditLogQuery query)
        {
            var result = await _auditTrailService.SearchAsync(
                query.FromUtc,
                query.ToUtc,
                query.UserId,
                query.EntityName,
                query.EntityId,
                query.FieldName,
                query.Action,
                query.Page,
                query.PageSize
            );

            var model = new AuditLogModel
            {
                Items = result.Items.Select(e => new AuditLogEntryModel
                {
                    Id = e.Id,
                    EntityName = e.EntityName,
                    EntityId = e.EntityId,
                    FieldName = e.FieldName,
                    OldValue = e.OldValue,
                    NewValue = e.NewValue,
                    ChangedOnUtc = e.ChangedOnUtc,
                    ActionId = e.ActionId,
                    Action = e.Action,
                    ClientIp = e.ClientIp,
                    Comment = e.Comment
                }).ToList(),
                TotalCount = result.TotalCount
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] AuditLogQuery query, string format = "json")
        {
            var result = await _auditTrailService.SearchAsync(
                query.FromUtc,
                query.ToUtc,
                query.UserId,
                query.EntityName,
                query.EntityId,
                query.FieldName,
                query.Action,
                0,
                int.MaxValue
            );

            var rows = result.Items.Select(e => new AuditLogEntryModel
            {
                Id = e.Id,
                EntityName = e.EntityName,
                EntityId = e.EntityId,
                FieldName = e.FieldName,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                ChangedOnUtc = e.ChangedOnUtc,
                ActionId = e.ActionId,
                Action = e.Action,
                ClientIp = e.ClientIp,
                Comment = e.Comment
            }).ToList();

            return Json(new { ok = true, count = rows.Count, data = rows });
        }
    }
}
