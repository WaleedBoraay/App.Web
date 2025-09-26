using App.Core.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Organization
{
    public class SubUnit : BaseEntity
    {
        public string Name { get; set; }
        public int UnitId { get; set; }
        public Unit Unit { get; set; }
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}
