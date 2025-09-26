using App.Core.Domain.Directory;
using App.Core.Domain.Institutions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Registrations
{
    public partial class Branch : BaseEntity
    {
        public int InstitutionId { get; set; }
        public virtual Institution Institution { get; set; }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public int CountryId { get; set; }
        public virtual Country Country { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedOnUtc { get; set; }

    }
}
