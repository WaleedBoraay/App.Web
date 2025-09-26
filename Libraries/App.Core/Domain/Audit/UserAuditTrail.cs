using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Audit
{
    public class UserAuditTrail : BaseEntity
    {
        public int AuditTrailId { get; set; }
        public int UserId { get; set; }
    }
}
