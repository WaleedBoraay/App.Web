using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Registrations
{
    /// <summary>
    /// Audit outcomes as per BRD.
    /// </summary>
    public enum AuditStatus
    {
        Accepted = 0,
        Return = 1,
        Rejected = 2
    }
}
