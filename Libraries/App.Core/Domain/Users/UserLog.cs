using System;

namespace App.Core.Domain.Users
{
    /// <summary>
    /// Logs user-related actions (activation, deactivation, login, password reset).
    /// </summary>
    public class UserLog : BaseEntity
    {
        public int UserId { get; set; }
        public string Action { get; set; } // e.g., "Activated", "Deactivated", "PasswordReset"
        public DateTime CreatedOnUtc { get; set; }
        public string ClientIp { get; set; }
    }
}
