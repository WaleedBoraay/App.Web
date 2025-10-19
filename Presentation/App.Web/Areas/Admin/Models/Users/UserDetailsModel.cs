using App.Core.Domain.Organization;
using App.Web.Areas.Admin.Models.Organization;

namespace App.Web.Areas.Admin.Models.Users
{
    public class UserDetailsModel
    {
        public UserModel User { get; set; }
        public IEnumerable<RoleModel> AllRoles { get; set; }
        public IEnumerable<Sector> Sectors { get; set; }
        public IEnumerable<Department> Departments { get; set; }
        public IEnumerable<Unit> Units { get; set; }
    }

}
