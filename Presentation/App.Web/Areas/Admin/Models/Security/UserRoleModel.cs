namespace App.Web.Areas.Admin.Models.Security
{
    public class UserRoleModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
