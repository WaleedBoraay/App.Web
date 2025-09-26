using App.Core.Domain.Organization;
using App.Core.Domain.Users;
using App.Services.Organization;
using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Organization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrganizationController : Controller
    {
        private readonly IOrganizationsServices _orgService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public OrganizationController(IOrganizationsServices orgService, IUserService userService, IRoleService roleService)
        {
            _orgService = orgService;
            _userService = userService;
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var departments = await _orgService.GetAllDepartmentsWithUsersAsync();
            var units = await _orgService.GetAllUnitsAsync();
            var subUnits = await _orgService.GetAllSubUnitsAsync();
            var allUsers = await _userService.GetAllAsync();
            var allRoles = await _roleService.GetAllAsync();


            var model = new OrganizationPageViewModel
            {
                Departments = departments.Select(d => new OrganizationModel
                {
                    DepartmentId = d.Id,
                    DepartmentName = d.Name,
                    Units = units.Where(u => u.DepartmentId == d.Id)
                        .Select(u => new UnitModel
                        {
                            UnitId = u.Id,
                            UnitName = u.Name,
                            SubUnits = subUnits.Where(su => su.UnitId == u.Id)
                                .Select(su => new SubUnitModel
                                {
                                    SubUnitId = su.Id,
                                    SubUnitName = su.Name,
                                    SubUnitUsers = allUsers
                                        .Where(x => x.SubUnitId == su.Id)
                                        .Select(x => new UserModel
                                        {
                                            Id = x.Id,
                                            UserName = x.Username,
                                            Email = x.Email,
                                            Roles = _roleService.GetRolesByUserIdAsync(x.Id).Result
                                                .Select(r => new RoleModel { Id = r.Id, Name = r.Name })
                                                .ToList()
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                }).ToList(),

                AllUsers = allUsers.Select(u => new UserModel
                {
                    Id = u.Id,
                    UserName = u.Username,
                    Email = u.Email
                }).ToList(),

                AllRoles = allRoles.Select(r => new RoleModel
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };

            return View(model);
        }





        // Department Actions
        [HttpPost]
        public async Task<IActionResult> CreateDepartment(string name)
        {
            if (string.IsNullOrEmpty(name))
                return RedirectToAction(nameof(Index));

            await _orgService.CreateDepartmentAsync(new App.Core.Domain.Organization.Department { Name = name });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var dept = await _orgService.GetDepartmentByIdAsync(id);
            if (dept != null)
                await _orgService.DeleteDepartmentAsync(dept);

            return RedirectToAction(nameof(Index));
        }

        // Unit Actions
        [HttpPost]
        public async Task<IActionResult> CreateUnit(int departmentId, string name)
        {
            await _orgService.CreateUnitAsync(new App.Core.Domain.Organization.Unit
            {
                DepartmentId = departmentId,
                Name = name
            });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var unit = await _orgService.GetUnitByIdAsync(id);
            if (unit != null)
                await _orgService.DeleteUnitAsync(unit);

            return RedirectToAction(nameof(Index));
        }

        // SubUnit Actions 
        [HttpPost]
        public async Task<IActionResult> CreateSubUnit(int unitId, string name)
        {
            await _orgService.CreateSubUnitAsync(new App.Core.Domain.Organization.SubUnit
            {
                UnitId = unitId,
                Name = name
            });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubUnit(int id)
        {
            var subUnit = await _orgService.GetSubUnitByIdAsync(id);
            if (subUnit != null)
                await _orgService.DeleteSubUnitAsync(subUnit);

            return RedirectToAction(nameof(Index));
        }

        // User Assignment 
        [HttpPost]
        public async Task<IActionResult> AssignUser(int userId, int roleId, int? departmentId, int? unitId, int? subUnitId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return RedirectToAction(nameof(Index));

            // تحديث مكان اليوزر
            user.DepartmentId = departmentId;
            user.UnitId = unitId;
            user.SubUnitId = subUnitId;
            await _userService.UpdateAsync(user);

            // اساين رول لليوزر
            await _roleService.AddUserToRoleAsync(userId, roleId);

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> RemoveUserRole(int userId, int roleId)
        {
            await _roleService.RemoveUserFromRoleAsync(userId, roleId);
            return RedirectToAction(nameof(Index));
        }
    }
}
