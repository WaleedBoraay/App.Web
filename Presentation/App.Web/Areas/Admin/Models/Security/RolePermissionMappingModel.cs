namespace App.Web.Areas.Admin.Models.Security
{
    public class RolePermissionMappingModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public IList<string> GrantedPermissions { get; set; } = new List<string>();
        public IList<PermissionModel> AllPermissions { get; set; } = new List<PermissionModel>();
    }
}
