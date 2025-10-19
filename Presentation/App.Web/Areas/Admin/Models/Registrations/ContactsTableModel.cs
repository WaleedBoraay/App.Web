namespace App.Web.Areas.Admin.Models.Registrations
{
    public class ContactsTableModel
    {
        public int RegistrationId { get; set; }
        public IList<ContactModel> Contacts { get; set; } = new List<ContactModel>();
    }
}
