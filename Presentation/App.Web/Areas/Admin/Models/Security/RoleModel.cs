using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Security
{
    public class RoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsSystemRole { get; set; }
    }
}
