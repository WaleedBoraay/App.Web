using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Organization
{
    public partial class SctoreContact : BaseEntity
    {
        public int SctoreId { get; set; }
        public int ContactId { get; set; }
	}
}
