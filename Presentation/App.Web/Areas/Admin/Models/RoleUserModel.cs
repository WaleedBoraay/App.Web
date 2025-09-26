namespace App.Web.Areas.Admin.Models
{
    public class RoleUserModel
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string Username { get; set; }
        public string RoleName { get; set; }
        public bool IsAssigned { get; set; }
    }
}