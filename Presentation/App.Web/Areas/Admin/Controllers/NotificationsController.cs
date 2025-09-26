//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using App.Services.Notifications;
//using App.Web.Areas.Admin.Models;

//namespace App.Web.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    public class NotificationsController : Controller
//    {
//        private readonly INotificationService _notifications;

//        public NotificationsController(INotificationService notifications)
//        {
//            _notifications = notifications;
//        }

//        [HttpGet]
//        public IActionResult Compose() => View(new NotificationComposeModel());

//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> Compose(NotificationComposeModel m)
//        {
//            if (!ModelState.IsValid) return View(m);
//            if (m.RegistrationId.HasValue)
//            {
//                var res = await _notifications.NotifyRegistrationEventAsync(m.RegistrationId.Value, m.EventType);
//                if (!res.Success) { TempData["Error"] = res.Error; return View(m); }
//            }
//            else
//            {
//                var res = await _notifications.NotifyAsync(m.EventType, m.RecipientUserId, m.Message ?? string.Empty, m.Channel ?? "InApp");
//                if (!res.Success) { TempData["Error"] = res.Error; return View(m); }
//            }
//            TempData["Success"] = "Notification sent.";
//            return RedirectToAction(nameof(Compose));
//        }
//    }
//}
