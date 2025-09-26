namespace App.Web.Areas.Admin.Models.Settings
{
    public class SettingSearchModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<SettingModel> Results { get; set; } = new();
    }
}
