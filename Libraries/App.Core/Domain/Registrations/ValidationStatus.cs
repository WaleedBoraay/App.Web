using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Registrations
{
    /// <summary>
    /// Validation lifecycle as per BRD.
    /// </summary>
    public enum ValidationStatus
    {
        Draft = 0,
        SubmitForApproval = 1,
        Accepted = 2,
        Return = 3,
        Rejected = 4,
        Valid = 5
    }
}
