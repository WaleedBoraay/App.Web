using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models
{
    public class EditUserModel
    {
        public int Id { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        public bool Active { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
}
