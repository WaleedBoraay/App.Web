using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using App.Services.Registrations;
using App.Services.Notifications;
using App.Services.Audit;

namespace App.Web.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private readonly IRegistrationService _registrationService;
        private readonly INotificationService _notificationService;
        private readonly IAuditTrailService _auditTrailService;

        public AdminController(
            IRegistrationService registrationService,
            INotificationService notificationService,
            IAuditTrailService auditTrailService)
        {
            _registrationService = registrationService;
            _notificationService = notificationService;
            _auditTrailService = auditTrailService;
        }

        public async Task<IActionResult> Index()
        {
            var registrations = await _registrationService.GetAllAsync();
            var notifications = await _notificationService.GetLogsByUserAsync(0); // demo userId=0
            var auditsPaged = await _auditTrailService.SearchAsync();

            // Counts using LINQ
            ViewBag.TotalRegs = registrations.Count;
            ViewBag.Drafts = registrations.Where(r => r.StatusId == 0).Count();
            ViewBag.Submitted = registrations.Where(r => r.StatusId == 1).Count();
            ViewBag.Approved = registrations.Where(r => r.StatusId == 2).Count();
            ViewBag.Rejected = registrations.Where(r => r.StatusId == 3).Count();

            // Pending
            ViewBag.PendingRegs = registrations.Where(r => r.StatusId == 1).Take(5).ToList();

            // Notifications
            ViewBag.Notifications = notifications.Take(5).ToList();

            // Audits (PagedResult -> Items)
            var audits = auditsPaged.Items;
            ViewBag.Audits = audits.Take(10).ToList();

            return View();
        }
    }
}
