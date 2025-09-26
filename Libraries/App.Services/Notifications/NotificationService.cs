using App.Core;
using App.Core.Domain.Notifications;
using App.Core.RepositoryServices;
using App.Services.Hubs;
using App.Services.Localization;
using App.Services.Security;
using App.Services.Templates;
using App.Services.Users;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IRepository<Notification> _notificationRepository;
        private readonly IRepository<NotificationLog> _logRepository;
        private readonly IRepository<NotificationReadLog> _readLogRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ITemplateService _templateService;
        private readonly IWorkContext _workContext;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IRepository<Notification> notificationRepository,
            IRepository<NotificationLog> logRepository,
            IRepository<NotificationReadLog> readLogRepository,
            ILocalizationService localizationService,
            ITemplateService templateService,
            IWorkContext workContext,
            IHubContext<NotificationHub> hubContext
            )
        {
            _notificationRepository = notificationRepository;
            _logRepository = logRepository;
            _readLogRepository = readLogRepository;
            _localizationService = localizationService;
            _templateService = templateService;
            _workContext = workContext;
            _hubContext = hubContext;
        }

        #region Sending

        public async Task SendAsync(
            int? registrationId,
            NotificationEvent eventType,
            int triggeredByUserId,
            int recipientUserId,
            NotificationChannel channel = NotificationChannel.InApp,
            IDictionary<string, string> tokens = null)
        {
            // 1) Get template
            var templates = await _templateService.GetAllAsync(channel.ToString());
            var template = templates.Data
                .FirstOrDefault(t => t.Name.Equals($"Notification.{eventType}", StringComparison.OrdinalIgnoreCase));

            // 2) Localize
            var eventName = await _localizationService.GetLocalizedEnumAsync(eventType);

            // 3) Build message
            string message;
            if (template != null)
            {
                message = template.Content;
                if (tokens != null)
                {
                    foreach (var kv in tokens)
                        message = message.Replace($"%{kv.Key}%", kv.Value);
                }
            }
            else
            {
                var defaultTemplate = await _localizationService.GetResourceAsync($"Notification.{eventType}");
                message = _localizationService.FormatMessage(defaultTemplate, registrationId, triggeredByUserId);
            }

            // 4) Determine EntityName dynamically
            string entityName = eventType switch
            {
                NotificationEvent.RegistrationSubmitted
                    or NotificationEvent.RegistrationValidated
                    or NotificationEvent.RegistrationApproved
                    or NotificationEvent.RegistrationRejected
                    or NotificationEvent.RegistrationReturnedForEdit
                    => "Registration",

                NotificationEvent.UserCreated
                    or NotificationEvent.UserUpdated
                    or NotificationEvent.UserDeleted
                    => "User",

                NotificationEvent.RoleAssigned
                    or NotificationEvent.RoleRevoked
                    => "Role",

                _ => "General"
            };

            // 5) Save notification
            var notification = new Notification
            {
                RegistrationId = registrationId,
                EventType = eventType,
                EventTypeId = (int)eventType,
                RecipientUserId = recipientUserId,
                TriggeredByUserId = triggeredByUserId,
                Message = message,
                ChannelId = (int)channel,
                Channel = channel,
                StatusId = (int)NotificationDeliveryStatus.Pending,
                Status = NotificationDeliveryStatus.Pending,
                CreatedOnUtc = DateTime.UtcNow,
                EntityName = entityName,
                EntityId = registrationId ?? 0
            };

            await _notificationRepository.InsertAsync(notification);

            // 5) Log
            var log = new NotificationLog
            {
                NotificationId = notification.Id,
                Channel = channel.ToString(),
                SentOnUtc = DateTime.UtcNow
            };

            try
            {
                switch (channel)
                {
                    case NotificationChannel.Email:
                        await SendEmailAsync(notification);
                        break;
                    case NotificationChannel.SMS:
                        await SendSmsAsync(notification);
                        break;
                    case NotificationChannel.Push:
                        await SendPushAsync(notification);
                        break;
                    case NotificationChannel.InApp:
                    default:
                        // InApp stored + broadcast via SignalR
                        await _hubContext.Clients
                            .User(notification.RecipientUserId.ToString())
                            .SendAsync("ReceiveNotification", new
                            {
                                id = notification.Id,
                                message = notification.Message,
                                createdOn = notification.CreatedOnUtc,
                                entity = notification.EntityName,
                                entityId = notification.EntityId
                            });
                        break;
                }

                notification.Status = NotificationDeliveryStatus.Sent;
                notification.StatusId = (int)NotificationDeliveryStatus.Sent;
                log.Success = true;
                log.Response = "Delivered successfully";
            }
            catch (Exception ex)
            {
                notification.Status = NotificationDeliveryStatus.Failed;
                notification.StatusId = (int)NotificationDeliveryStatus.Failed;
                log.Success = false;
                log.Response = ex.Message;
            }

            await _notificationRepository.UpdateAsync(notification);
            await _logRepository.InsertAsync(log);
        }

        #endregion

        #region Logs & ReadLogs

        public async Task<IList<NotificationLog>> GetLogsByUserAsync(int userId)
        {
            var notifications = await _notificationRepository.Table
                .Where(n => n.RecipientUserId == userId)
                .Select(n => n.Id)
                .ToListAsync();

            return await _logRepository.Table
                .Where(l => notifications.Contains(l.NotificationId))
                .OrderByDescending(l => l.SentOnUtc)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var entry = new NotificationReadLog
            {
                NotificationId = notificationId,
                UserId = userId,
                ReadOnUtc = DateTime.UtcNow
            };

            await _readLogRepository.InsertAsync(entry);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var currentUser = await _workContext.GetCurrentUserAsync();
            if (currentUser == null)
                throw new InvalidOperationException("No current user found for MarkAsRead.");

            await MarkAsReadAsync(notificationId, currentUser.Id);
        }

        #endregion

        #region Retry

        public async Task RetryFailedAsync(int maxAttempts = 3)
        {
            var failed = await _notificationRepository.Table
                .Where(n => n.Status == NotificationDeliveryStatus.Failed)
                .Take(50)
                .ToListAsync();

            foreach (var notification in failed)
            {
                await SendAsync(
                    notification.RegistrationId,
                    notification.EventType,
                    notification.TriggeredByUserId,
                    notification.RecipientUserId,
                    notification.Channel,
                    new Dictionary<string, string> { { "Message", notification.Message } }
                );
            }
        }

        #endregion

        #region Channels

        private Task SendEmailAsync(Notification notification)
        {
            Console.WriteLine($"[EMAIL] To: {notification.RecipientUserId}, Msg: {notification.Message}");
            return Task.CompletedTask;
        }

        private Task SendSmsAsync(Notification notification)
        {
            Console.WriteLine($"[SMS] To: {notification.RecipientUserId}, Msg: {notification.Message}");
            return Task.CompletedTask;
        }

        private Task SendPushAsync(Notification notification)
        {
            Console.WriteLine($"[PUSH] To: {notification.RecipientUserId}, Msg: {notification.Message}");
            return Task.CompletedTask;
        }

        public async Task<IList<(Notification Notification, bool IsRead)>> GetInboxAsync(int userId, bool onlyUnread = false)
        {
            var notifications = await _notificationRepository.GetAllAsync(q =>
                q.Where(n => n.RecipientUserId == userId)
                 .OrderByDescending(n => n.CreatedOnUtc));

            var readIds = await _readLogRepository.Table
                .Where(r => r.UserId == userId)
                .Select(r => r.NotificationId)
                .ToListAsync();

            var result = notifications
                .Select(n => (Notification: n, IsRead: readIds.Contains(n.Id)))
                .ToList();

            if (onlyUnread)
                result = result.Where(x => !x.IsRead).ToList();

            return result;
        }

        private async Task AddNotificationAsync(string resourceKey, NotificationType type)
        {
            var message = await _localizationService.GetResourceAsync(resourceKey, resourceKey);

            var notification = new Notification
            {
                Message = message,
                Type = type,
                CreatedOnUtc = DateTime.UtcNow
            };
        }

        public Task NotifySuccessAsync(string resourceKey) =>
            AddNotificationAsync(resourceKey, NotificationType.Success);

        public Task NotifyErrorAsync(string resourceKey) =>
            AddNotificationAsync(resourceKey, NotificationType.Error);

        public Task NotifyWarningAsync(string resourceKey) =>
            AddNotificationAsync(resourceKey, NotificationType.Warning);

        public Task NotifyInfoAsync(string resourceKey) =>
            AddNotificationAsync(resourceKey, NotificationType.Info);

        #endregion
    }
}

