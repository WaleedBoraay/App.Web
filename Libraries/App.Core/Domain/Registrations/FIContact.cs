using App.Core.Domain.Common;

namespace App.Core.Domain.Registrations
{
    public partial class FIContact : BaseEntity
    {
        public int RegistrationId { get; set; }

        /// <summary>
        /// Type of contact (Primary, Authorized, Delegate, BusinessOwner)
        /// </summary>
        public int ContactTypeId { get; set; }
        public ContactType ContactType { get; set; }

        public string JobTitle { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public string CivilId { get; set; }
        public string PassportId { get; set; }

        public int CivilAttachmentId { get; set; }
        public int PassportAttachmentId { get; set; }

        public string ContactPhone { get; set; }
        public string BusinessPhone { get; set; }
        public string Email { get; set; }

        public int NationalityCountryId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}
