using App.Services;
using App.Services.Localization;
using App.Services.Settings;
using App.Services.Users;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Settings;
using App.Web.Areas.Admin.Models.Users;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly IUserSettingsService _userSettings;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;

        public SettingsController(
            IUserSettingsService userSettings,
            ISettingService settingService,
            ILanguageService languageService,
            IWorkContext workContext)
        {
            _userSettings = userSettings;
            _settingService = settingService;
            _languageService = languageService;
            _workContext = workContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;

            var allSettingsList = (await _settingService.GetAllAsync() ?? new Dictionary<string, string>())
                .Select(s => new SettingModel
                {
                    Name = s.Key,
                    Value = s.Value
                }).ToList();

            var allSettings = new SettingSearchModel
            {
                Results = allSettingsList
            };

            var pref = await _userSettings.GetByUserIdAsync(userId);
            var userPref = pref == null
                ? new UserPreferenceModel { UserId = userId }
                : new UserPreferenceModel
                {
                    UserId = userId,
                    LanguageId = pref.LanguageId,
                    EnableMfa = pref.EnableMfa,
                    NotifyByEmail = pref.NotifyByEmail,
                    NotifyBySms = pref.NotifyBySms,
                    NotifyInApp = pref.NotifyInApp
                };

            var systemSettings = new SystemSettingsModel
            {
                ApplicationName = await _settingService.GetAsync("System.ApplicationName", "Supervision") ?? "Supervision",
                DefaultTimeZone = await _settingService.GetAsync("System.DefaultTimeZone", "UTC") ?? "UTC",
                EnableEmail = await _settingService.GetAsync("System.EnableEmail", true),
                EnableAuditTrail = await _settingService.GetAsync("System.EnableAuditTrail", false)
            };

            var notificationSettings = new NotificationSettingsModel
            {
                EnableEmail = await _settingService.GetAsync("Notifications.EnableEmail", true),
                EnableSms = await _settingService.GetAsync("Notifications.EnableSms", false),
                EnableInApp = await _settingService.GetAsync("Notifications.EnableInApp", true)
            };

            var languages = (await _languageService.GetAllAsync() ?? new List<App.Core.Domain.Localization.Language>())
                .Select(l => new LanguageModel
                {
                    Id = l.Id,
                    Name = l.Name,
                    LanguageCulture = l.LanguageCulture,
                    UniqueSeoCode = l.UniqueSeoCode,
                    FlagImageFileName = l.FlagImageFileName,
                    Rtl = l.Rtl,
                    Published = l.Published,
                    DisplayOrder = l.DisplayOrder
                }).ToList();

            var branding = new BrandingSettingsModel
            {
                LogoUrl = await _settingService.GetAsync("Branding.LogoUrl", "/images/default-logo.png")
                          ?? "/images/default-logo.png"
            };

            var model = new SettingsPageModel
            {
                AllSettings = allSettings,
                UserPreferences = userPref,
                SystemSettings = systemSettings,
                NotificationSettings = notificationSettings,
                AvailableLanguages = languages,
                BrandingSettings = branding
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AllSettings(string name, string value)
        {
            var getall = await _settingService.GetAllAsync();
            var settings = await _settingService.GetAsync(name, value);
            var model = new SettingSearchModel
            {
                Name = name,
                Value = value,
                Results = settings.Select(s => new SettingModel
                {
                    Name = name,
                    Value = s.ToString()
                }).ToList()
            };
            return View(getall);
        }

        [HttpPost]
        public async Task<IActionResult> EditSetting(string name, string value)
        {
            await _settingService.SetAsync(name, value);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSetting(string name)
        {
            await _settingService.DeleteAsync(name);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> UserPreferences(int userId)
        {
            var pref = await _userSettings.GetByUserIdAsync(userId);


            var model = pref == null
                ? new UserPreferenceModel { UserId = userId }
                : new UserPreferenceModel
                {
                    UserId = userId,
                    LanguageId = pref.LanguageId,
                    EnableMfa = pref.EnableMfa,
                    NotifyByEmail = pref.NotifyByEmail,
                    NotifyBySms = pref.NotifyBySms,
                    NotifyInApp = pref.NotifyInApp
                };

            var languages = (await _languageService.GetAllAsync() ?? new List<App.Core.Domain.Localization.Language>())
    .Select(l => new LanguageModel
    {
        Id = l.Id,
        Name = l.Name,
        LanguageCulture = l.LanguageCulture,
        UniqueSeoCode = l.UniqueSeoCode,
        FlagImageFileName = l.FlagImageFileName,
        Rtl = l.Rtl,
        Published = l.Published,
        DisplayOrder = l.DisplayOrder
    }).ToList();
            model.AvailableLanguages = languages;


            // هنمرر الـ Languages كـ ViewData للبارشيال أو مباشرة من Index
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserPreferences(UserPreferenceModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _userSettings.SetLanguageAsync(model.UserId, model.LanguageId ?? 1);
            await _userSettings.ToggleMfaAsync(model.UserId, model.EnableMfa);
            await _userSettings.UpdateNotificationPreferencesAsync(
                model.UserId,
                model.NotifyByEmail,
                model.NotifyBySms,
                model.NotifyInApp
            );

            TempData["SuccessMessage"] = "User preferences updated successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> System()
        {
            var model = new SystemSettingsModel
            {
                ApplicationName = await _settingService.GetAsync("System.ApplicationName", "Supervision"),
                DefaultTimeZone = await _settingService.GetAsync("System.DefaultTimeZone", "UTC"),
                EnableEmail = await _settingService.GetAsync("System.EnableEmail", true),
                EnableAuditTrail = await _settingService.GetAsync("System.EnableAuditTrail", false)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> System(SystemSettingsModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _settingService.SetAsync("System.ApplicationName", model.ApplicationName);
            await _settingService.SetAsync("System.DefaultTimeZone", model.DefaultTimeZone);
            await _settingService.SetAsync("System.EnableEmail", model.EnableEmail);
            await _settingService.SetAsync("System.EnableAuditTrail", model.EnableAuditTrail);

            TempData["SuccessMessage"] = "System settings updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var model = new NotificationSettingsModel
            {
                EnableEmail = await _settingService.GetAsync("Notifications.EnableEmail", true),
                EnableSms = await _settingService.GetAsync("Notifications.EnableSms", false),
                EnableInApp = await _settingService.GetAsync("Notifications.EnableInApp", true)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Notifications(NotificationSettingsModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _settingService.SetAsync("Notifications.EnableEmail", model.EnableEmail);
            await _settingService.SetAsync("Notifications.EnableSms", model.EnableSms);
            await _settingService.SetAsync("Notifications.EnableInApp", model.EnableInApp);

            TempData["SuccessMessage"] = "Notification settings updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBranding(IFormFile logo)
        {
            if (logo != null && logo.Length > 0)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "branding");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = "logo.png";
                var fullPath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }

                var relativePath = "/uploads/branding/logo.png";
                await _settingService.SetAsync("Branding.LogoUrl", relativePath);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
