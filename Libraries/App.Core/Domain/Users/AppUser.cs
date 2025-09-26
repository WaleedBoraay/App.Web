using System;
using App.Core.Domain.Common;
using App.Core.Domain.Organization;

namespace App.Core.Domain.Users
{
    /// <summary>
    /// Represents a system user (internal or external).
    /// </summary>
    public partial class AppUser : BaseEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }

        // Passwords
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; } // legacy / PBKDF2 salt
        public int PasswordFormatId { get; set; } = (int)PasswordFormat.Hashed;
        public PasswordFormat PasswordFormat
        {
            get => (PasswordFormat)PasswordFormatId;
            set => PasswordFormatId = (int)value;
        }

        // Security
        public bool IsActive { get; set; }
        public bool IsLockedOut { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEndUtc { get; set; }

        // Audit
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
        public DateTime? LastLoginDateUtc { get; set; }
        public DateTime? DeactivatedOnUtc { get; set; }

        // Relations
        public int? InstitutionId { get; set; }
        public int? RegistrationId { get; set; }

        // Organization Hierarchy (new)
        public int? DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        public int? UnitId { get; set; }
        public virtual Unit Unit { get; set; }

        public int? SubUnitId { get; set; }
        public virtual SubUnit SubUnit { get; set; }

        // Recovery / MFA
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public string TwoFactorSecret { get; set; } // e.g., TOTP key
        public bool TwoFactorEnabled { get; set; }

    }
}
