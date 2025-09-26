using System.Collections.Generic;
using App.Web.Areas.Admin.Models;

namespace App.Web.Areas.Admin.Models.Organization
{
    public class OrganizationModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        public List<UnitModel> Units { get; set; } = new();
        public List<UserModel> DepartmentUsers { get; set; } = new();
    }

    public class UnitModel
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }

        public List<SubUnitModel> SubUnits { get; set; } = new();
        public List<UserModel> UnitUsers { get; set; } = new();
    }

    public class SubUnitModel
    {
        public int SubUnitId { get; set; }
        public string SubUnitName { get; set; }

        public List<UserModel> SubUnitUsers { get; set; } = new();
    }

    public class RoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OrganizationPageViewModel
    {
        public IEnumerable<OrganizationModel> Departments { get; set; } = new List<OrganizationModel>();
        public IEnumerable<UserModel> AllUsers { get; set; } = new List<UserModel>();
        public IEnumerable<RoleModel> AllRoles { get; set; } = new List<RoleModel>();
    }
}
