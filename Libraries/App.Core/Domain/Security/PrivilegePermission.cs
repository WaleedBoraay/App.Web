using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Security
{
    public class PrivilegePermission : BaseEntity
    {
        public int PrivilegeId { get; set; }
        public int PermissionId { get; set; }
    }
}
