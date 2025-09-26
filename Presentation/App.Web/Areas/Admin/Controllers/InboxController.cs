using App.Core;
using App.Services;
using App.Services.Common;
using App.Services.Notifications;
using App.Web.Areas.Admin.Models.Notifications;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InboxController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;

        public InboxController(INotificationService notificationService, IWorkContext workContext)
        {
            _notificationService = notificationService;
            _workContext = workContext;
        }

        public async Task<IActionResult> Index(bool onlyUnread = false)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null)
                return Unauthorized();

            // Get Domain notifications + read status
            var inbox = await _notificationService.GetInboxAsync(currentUser.Id, onlyUnread);

            // Map to ViewModel
            var items = inbox.Select(x => new NotificationModel
            {
                Id = x.Notification.Id,
                Message = x.Notification.Message,
                CreatedOnUtc = x.Notification.CreatedOnUtc,
                IsRead = x.IsRead,
                TargetUrl = GenerateTargetUrl(x.Notification.EntityName, x.Notification.EntityId)
            }).ToList();

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private string GenerateTargetUrl(string entityName, int entityId)
        {
            return entityName switch
            {
                "Registration" => $"/Admin/Registrations/Details/{entityId}",
                "User" => $"/Admin/Users/Edit/{entityId}",
                "Role" => $"/Admin/Role/Edit/{entityId}",
                _ => "/Admin/Inbox/Index"
            };
        }
    }
}
