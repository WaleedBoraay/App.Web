using App.Services.Security;
using App.Services.Users;
using App.Web.Areas.Admin.Models.Security;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
public class UserPermissionOverridesController : Controller
{
    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;

    public UserPermissionOverridesController(IUserService userService, IPermissionService permissionService)
    {
        _userService = userService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        var allPerms = await _permissionService.GetAllAsync();

        var model = new List<UserPermissionOverrideModel>();

        foreach (var user in users)
        {
            var overrides = await _permissionService.GetUserOverridesAsync(user.Id);

            var granted = overrides.Where(o => o.EndsWith("Allow"))
                                   .Select(o => o.Split(':')[0])
                                   .ToList();

            var denied = overrides.Where(o => o.EndsWith("Deny"))
                                  .Select(o => o.Split(':')[0])
                                  .ToList();

            model.Add(new UserPermissionOverrideModel
            {
                UserId = user.Id,
                Username = user.Username,
                Granted = granted,
                Denied = denied,
                AllPermissions = allPerms.Select(p => new PermissionModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SystemName = p.SystemName,
                    Category = p.Category,
                    Description = p.Description,
                    IsActive = p.IsActive
                }).ToList()
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Grant(int userId, string permissionSystemName)
    {
        await _permissionService.SetUserOverrideAsync(userId, permissionSystemName, true);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deny(int userId, string permissionSystemName)
    {
        await _permissionService.SetUserOverrideAsync(userId, permissionSystemName, false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int userId, string permissionSystemName)
    {
        await _permissionService.RemoveUserOverrideAsync(userId, permissionSystemName);
        return RedirectToAction(nameof(Index));
    }
}
