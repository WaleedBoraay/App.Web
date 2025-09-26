using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Notifications
{
    public enum NotificationDeliveryStatus
    {
        Pending = 0,
        Sent = 1,
        Failed = 2,
        Delivered = 3,
        Read = 4
    }
}
