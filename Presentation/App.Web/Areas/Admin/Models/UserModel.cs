using App.Core.Attributes;
using App.Core.Domain.Users;
using App.Web.Areas.Admin.Models.Organization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email")]
        [AppResourceDisplayName("Admin.Users.Fields.EmailAddress")]
        public string Email { get; set; }

        [Display(Name = "Active")]
        public bool Active { get; set; } = true;

        [Display(Name = "Locked Out")]
        public bool IsLockedOut { get; set; } = false;

        [Display(Name = "Failed Login Attempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        [AppResourceDisplayName("Admin.Users.Fields.LastLoginDateUtc")]
        public DateTime? LastLoginDateUtc { get; set; }

        [AppResourceDisplayName("Admin.Users.Fields.CreatedOnUtc")]
        public DateTime? CreatedOnUtc { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? UpdatedOnUtc { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        public string? RoleName { get; set; }
        public IList<RoleModel> Roles { get; set; } = new List<RoleModel>();
        [AppResourceDisplayName("Admin.Users.Fields.Sectore")]

        public int? DepartmentId { get; set; }
        [AppResourceDisplayName("Admin.Users.Fields.Department")]

        public int? UnitId { get; set; }
        [AppResourceDisplayName("Admin.Users.Fields.Unit")]

        public int? SubUnitId { get; set; }

        public string? DepartmentName { get; set; }
        public string? UnitName { get; set; }
        public string? SubUnitName { get; set; }

        public IEnumerable<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Units { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> SubUnits { get; set; } = new List<SelectListItem>();

    }

}