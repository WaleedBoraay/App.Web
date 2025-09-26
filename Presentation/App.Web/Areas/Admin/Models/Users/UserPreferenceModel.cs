namespace App.Web.Areas.Admin.Models.Users
{
    public class UserPreferenceModel
    {
        public int UserId { get; set; }
        public int? LanguageId { get; set; }
        public bool EnableMfa { get; set; }
        public bool NotifyByEmail { get; set; }
        public bool NotifyBySms { get; set; }
        public bool NotifyInApp { get; set; }
        public IList<LanguageModel> AvailableLanguages { get; set; } = new List<LanguageModel>();

    }
}
