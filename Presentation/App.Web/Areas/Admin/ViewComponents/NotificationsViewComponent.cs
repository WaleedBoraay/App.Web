using App.Core;
using App.Services;
using App.Services.Common;
using App.Services.Notifications;
using App.Web.Areas.Admin.Models.Notifications;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.ViewComponents
{
    public class NotificationsViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly IWorkContext _workContext;

        public NotificationsViewComponent(INotificationService notificationService, IWorkContext workContext)
        {
            _notificationService = notificationService;
            _workContext = workContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null)
                return View(new List<NotificationModel>());

            var inbox = await _notificationService.GetInboxAsync(currentUser.Id, onlyUnread: true);

            var items = inbox.Select(x => new NotificationModel
            {
                Id = x.Notification.Id,
                Message = x.Notification.Message,
                CreatedOnUtc = x.Notification.CreatedOnUtc,
                IsRead = x.IsRead,
                TargetUrl = GenerateTargetUrl(x.Notification.EntityName, x.Notification.EntityId)
            }).Take(5).ToList();

            return View(items);
        }

        private string GenerateTargetUrl(string entityName, int entityId)
        {
            return entityName switch
            {
                "Registration" => $"/Admin/Registration/Details/{entityId}",
                "User" => $"/Admin/User/Edit/{entityId}",
                "Role" => $"/Admin/Role/Edit/{entityId}",
                _ => "/Admin/Inbox/Index"
            };
        }
    }
}
