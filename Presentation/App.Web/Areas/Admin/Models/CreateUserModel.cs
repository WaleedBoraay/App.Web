using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models
{
    public class CreateUserModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool Active { get; set; } = true;
        public List<int> RoleIds { get; set; } = new();
    }
}
