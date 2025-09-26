namespace App.Web.Areas.Admin.Models.Security
{
    public class PermissionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
