using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Registrations
{
    public class RegistrationDocument : BaseEntity
    {
        public int RegistrationId { get; set; }
        public int DocumentId { get; set; }
    }
}
