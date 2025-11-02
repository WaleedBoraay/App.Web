using App.Core.Domain.Common;

namespace App.Core.Domain.Registrations
{
    public partial class Contact : BaseEntity
    {
        public int RegistrationId { get; set; }

        /// <summary>
        /// Type of contact (Primary, Authorized, Delegate, BusinessOwner)
        /// </summary>
        public int ContactTypeId { get; set; }
		public ContactType ContactTypes
		{
			get => (ContactType)ContactTypeId;
			set => ContactTypeId = (int)value;
		}

		public string JobTitle { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public string ContactPhone { get; set; }
        public string BusinessPhone { get; set; }
        public string Email { get; set; }

        public int NationalityCountryId { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}
