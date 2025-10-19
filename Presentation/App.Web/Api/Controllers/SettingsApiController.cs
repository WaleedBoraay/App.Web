using App.Core.Domain.Users;
using App.Services;
using App.Services.Localization;
using App.Services.Settings;
using App.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsApiController : ControllerBase
    {
        private readonly IUserSettingsService _userSettings;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;

        public SettingsApiController(
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

        // ✅ GET: All settings (system + user preferences + notifications + branding)
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var userId = (await _workContext.GetCurrentUserAsync())?.Id ?? 0;

            var allSettings = await _settingService.GetAllAsync();

            var userPref = await _userSettings.GetByUserIdAsync(userId);
            var languages = await _languageService.GetAllAsync();

            var data = new
            {
                AllSettings = allSettings,
                UserPreferences = userPref != null
                    ? userPref
                    : new UserPreference { UserId = userId },
                AvailableLanguages = languages.Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.LanguageCulture,
                    l.UniqueSeoCode,
                    l.FlagImageFileName,
                    l.Rtl,
                    l.Published,
                    l.DisplayOrder
                }),
                Branding = new
                {
                    LogoUrl = await _settingService.GetAsync("Branding.LogoUrl", "/images/default-logo.png")
                },
                SystemSettings = new
                {
                    ApplicationName = await _settingService.GetAsync("System.ApplicationName", "Supervision"),
                    DefaultTimeZone = await _settingService.GetAsync("System.DefaultTimeZone", "UTC"),
                    EnableEmail = await _settingService.GetAsync("System.EnableEmail", true),
                    EnableAuditTrail = await _settingService.GetAsync("System.EnableAuditTrail", false)
                },
                NotificationSettings = new
                {
                    EnableEmail = await _settingService.GetAsync("Notifications.EnableEmail", true),
                    EnableSms = await _settingService.GetAsync("Notifications.EnableSms", false),
                    EnableInApp = await _settingService.GetAsync("Notifications.EnableInApp", true)
                }
            };

            return Ok(data);
        }

        // ✅ GET: Retrieve single setting
        [HttpGet("{name}")]
        public async Task<IActionResult> GetSetting(string name)
        {
            var value = await _settingService.GetAsync(name, (string)null);
            if (value == null)
                return NotFound(new { message = $"Setting '{name}' not found." });

            return Ok(new { Name = name, Value = value });
        }

        // ✅ POST: Create or update a setting
        [HttpPost("set")]
        public async Task<IActionResult> SetSetting([FromBody] SettingInputModel model)
        {
            if (string.IsNullOrEmpty(model.Name))
                return BadRequest(new { message = "Setting name is required." });

            await _settingService.SetAsync(model.Name, model.Value);
            return Ok(new { success = true, message = "Setting saved successfully." });
        }

        // ✅ DELETE: Delete a setting
        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteSetting(string name)
        {
            await _settingService.DeleteAsync(name);
            return Ok(new { success = true, message = "Setting deleted successfully." });
        }

        // ✅ GET: User preferences
        [HttpGet("user-preferences/{userId:int}")]
        public async Task<IActionResult> GetUserPreferences(int userId)
        {
            var pref = await _userSettings.GetByUserIdAsync(userId);
            if (pref == null)
                return Ok(new { UserId = userId, message = "No preferences found. Returning defaults." });

            var languages = await _languageService.GetAllAsync();

            return Ok(new
            {
                pref.UserId,
                pref.LanguageId,
                pref.EnableMfa,
                pref.NotifyByEmail,
                pref.NotifyBySms,
                pref.NotifyInApp,
                AvailableLanguages = languages.Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.LanguageCulture,
                    l.FlagImageFileName
                })
            });
        }

        // ✅ POST: Update user preferences
        [HttpPost("user-preferences")]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferenceInputModel model)
        {
            if (model.UserId <= 0)
                return BadRequest(new { message = "Invalid UserId." });

            await _userSettings.SetLanguageAsync(model.UserId, model.LanguageId ?? 1);
            await _userSettings.ToggleMfaAsync(model.UserId, model.EnableMfa);
            await _userSettings.UpdateNotificationPreferencesAsync(
                model.UserId,
                model.NotifyByEmail,
                model.NotifyBySms,
                model.NotifyInApp
            );

            return Ok(new { success = true, message = "User preferences updated successfully." });
        }

        // ✅ GET: System settings
        [HttpGet("system")]
        public async Task<IActionResult> GetSystemSettings()
        {
            var model = new
            {
                ApplicationName = await _settingService.GetAsync("System.ApplicationName", "Supervision"),
                DefaultTimeZone = await _settingService.GetAsync("System.DefaultTimeZone", "UTC"),
                EnableEmail = await _settingService.GetAsync("System.EnableEmail", true),
                EnableAuditTrail = await _settingService.GetAsync("System.EnableAuditTrail", false)
            };

            return Ok(model);
        }

        // ✅ POST: Update system settings
        [HttpPost("system")]
        public async Task<IActionResult> UpdateSystemSettings([FromBody] SystemSettingsInputModel model)
        {
            await _settingService.SetAsync("System.ApplicationName", model.ApplicationName);
            await _settingService.SetAsync("System.DefaultTimeZone", model.DefaultTimeZone);
            await _settingService.SetAsync("System.EnableEmail", model.EnableEmail);
            await _settingService.SetAsync("System.EnableAuditTrail", model.EnableAuditTrail);

            return Ok(new { success = true, message = "System settings updated successfully." });
        }

        // ✅ GET: Notification settings
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotificationSettings()
        {
            var model = new
            {
                EnableEmail = await _settingService.GetAsync("Notifications.EnableEmail", true),
                EnableSms = await _settingService.GetAsync("Notifications.EnableSms", false),
                EnableInApp = await _settingService.GetAsync("Notifications.EnableInApp", true)
            };

            return Ok(model);
        }

        // ✅ POST: Update notification settings
        [HttpPost("notifications")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsInputModel model)
        {
            await _settingService.SetAsync("Notifications.EnableEmail", model.EnableEmail);
            await _settingService.SetAsync("Notifications.EnableSms", model.EnableSms);
            await _settingService.SetAsync("Notifications.EnableInApp", model.EnableInApp);

            return Ok(new { success = true, message = "Notification settings updated successfully." });
        }

        // ✅ POST: Upload branding logo
        [HttpPost("branding/logo")]
        public async Task<IActionResult> UploadBrandingLogo(IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
                return BadRequest(new { message = "Logo file is required." });

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

            return Ok(new { success = true, logoUrl = relativePath });
        }
    }

    // ✅ DTOs
    public class SettingInputModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class UserPreferenceInputModel
    {
        public int UserId { get; set; }
        public int? LanguageId { get; set; }
        public bool EnableMfa { get; set; }
        public bool NotifyByEmail { get; set; }
        public bool NotifyBySms { get; set; }
        public bool NotifyInApp { get; set; }
    }

    public class SystemSettingsInputModel
    {
        public string ApplicationName { get; set; }
        public string DefaultTimeZone { get; set; }
        public bool EnableEmail { get; set; }
        public bool EnableAuditTrail { get; set; }
    }

    public class NotificationSettingsInputModel
    {
        public bool EnableEmail { get; set; }
        public bool EnableSms { get; set; }
        public bool EnableInApp { get; set; }
    }
}
