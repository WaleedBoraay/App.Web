using App.Web.Areas.Admin.Models.Users;

namespace App.Web.Areas.Admin.Models.Settings
{
    public class SettingsPageModel
    {
        public SettingSearchModel AllSettings { get; set; } = new();
        public UserPreferenceModel UserPreferences { get; set; } = new();
        public SystemSettingsModel SystemSettings { get; set; } = new();
        public NotificationSettingsModel NotificationSettings { get; set; } = new();
        public IList<LanguageModel> AvailableLanguages { get; set; } = new List<LanguageModel>();
        public BrandingSettingsModel BrandingSettings { get; set; } = new();


    }
}
