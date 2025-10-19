namespace App.Web.Areas.Admin.Models.Security
{
    public class UserRoleUpdateModel
    {
        public int UserId { get; set; }
        public List<int> SelectedRoleIds { get; set; } = new();
    }
}
