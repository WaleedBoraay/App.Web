namespace App.Web.Areas.Admin.Models.Security
{
    public class UserPermissionOverrideModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }

        public IList<string> Granted { get; set; } = new List<string>();
        public IList<string> Denied { get; set; } = new List<string>();

        public IList<PermissionModel> AllPermissions { get; set; } = new List<PermissionModel>();
    }
}
