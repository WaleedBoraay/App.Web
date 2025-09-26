using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models
{
    public class UserSecuritySettingsModel
    {
        [Range(1, 20)]
        public int MaxFailedAccessAttempts { get; set; } = 5;
        [Range(1, 1440)]
        public int DefaultLockoutMinutes { get; set; } = 15;
        [Range(6, 128)]
        public int PasswordMinLength { get; set; } = 8;
        public bool RequireDigit { get; set; } = true;
        public bool RequireUppercase { get; set; } = false;
        public bool RequireLowercase { get; set; } = false;
        public bool RequireNonAlphanumeric { get; set; } = false;
    }
}
