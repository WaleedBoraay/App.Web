using System;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models
{
    public class ManageAccountModel
    {
        public int Id { get; set; }

        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public bool Active { get; set; }

        public DateTime? LastLoginDateUtc { get; set; } = DateTime.UtcNow;
    }
}
