﻿using App.Core.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Organization
{
    public partial class Department : BaseEntity
    {
        public string Name { get; set; }
        public int SectorId { get; set; }
        public Sector Sector { get; set; }
        public ICollection<Unit> Units { get; set; }

	}
}
