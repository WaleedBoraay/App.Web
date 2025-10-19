namespace App.Web.Areas.Admin.Models.Registrations
{
    public class RegistrationListModel
    {
        public bool IsMaker { get; set; }
        public bool IsChecker { get; set; }
        public bool IsRegulator { get; set; }
        public bool IsInspector { get; set; }

        public IList<RegistrationModel> Registrations { get; set; } = new List<RegistrationModel>();
    }
}
