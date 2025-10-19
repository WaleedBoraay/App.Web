namespace App.Web.Areas.Admin.Models.Security
{
    public class UserRolesIndexModel
    {
        public List<UserRoleModel> Users { get; set; } = new();
        public List<RoleModel> AllRoles { get; set; } = new();
    }
}
