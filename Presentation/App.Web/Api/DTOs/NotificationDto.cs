using System.Collections.Generic;
using App.Core.Domain.Notifications;
using App.Core.Domain.Registrations;

namespace App.Web.Api.DTOs
{
    public record NotificationDto(
        NotificationEvent EventType,
        int TriggeredByUserId,
        int RecipientUserId,
        NotificationChannel Channel = NotificationChannel.InApp,
        IDictionary<string, string> Tokens = null
    );
}
