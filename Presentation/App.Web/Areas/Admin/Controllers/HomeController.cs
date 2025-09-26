using App.Services.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISettingService _settingsService;

        public HomeController(ISettingService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetAllAsync();

            if (settings.TryGetValue("System.ApplicationName", out var applicationName))
            {
                ViewData["Title"] = applicationName;
            }

            return View();
        }
    }
}
