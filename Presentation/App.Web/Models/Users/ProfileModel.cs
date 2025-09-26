using System.ComponentModel.DataAnnotations;

namespace App.Web.Models.Users
{
    public class ProfileModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Enable Two-Factor Authentication")]
        public bool TwoFactorEnabled { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}
