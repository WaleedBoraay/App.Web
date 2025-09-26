namespace App.Core.Domain.Users
{
    /// <summary>
    /// Strongly-typed settings related to user management
    /// </summary>
    public class UserSettings
    {
        // Password policy
        public int PasswordMinLength { get; set; } = 6;
        public bool RequireDigit { get; set; } = true;
        public bool RequireUppercase { get; set; } = false;
        public bool RequireLowercase { get; set; } = false;
        public bool RequireNonAlphanumeric { get; set; } = false;

        // Registration / login
        public bool RequireUniqueEmail { get; set; } = true;
        public bool RequireConfirmedEmail { get; set; } = false;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int DefaultLockoutMinutes { get; set; } = 15;

        // Profile
        public bool AllowProfileEditing { get; set; } = true;
        public string DefaultAvatarUrl { get; set; } = null;
    }
}
