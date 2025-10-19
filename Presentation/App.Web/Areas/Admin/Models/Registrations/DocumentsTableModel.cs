namespace App.Web.Areas.Admin.Models.Registrations
{
    public class DocumentsTableModel
    {
        public int RegistrationId { get; set; }
        public IList<DocumentModel> Documents { get; set; } = new List<DocumentModel>();
    }
}
