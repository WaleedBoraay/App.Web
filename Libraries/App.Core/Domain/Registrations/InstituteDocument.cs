using App.Core.Domain.Institutions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Registrations
{
    public class InstituteDocument : BaseEntity
    {
        public int InstituteId { get; set; }
        public virtual Institution Institution { get; set; }

        public int DocumentId { get; set; }
        public virtual Document Document { get; set; }
    }
}
