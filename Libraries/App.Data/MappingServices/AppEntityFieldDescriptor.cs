using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MappingServices
{
    public partial class AppEntityFieldDescriptor
    {
        public string Name { get; set; }
        public bool IsIdentity { get; set; }
        public bool? IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public int? Precision { get; set; }
        public int? Size { get; set; }
        public DbType Type { get; set; }
        public int? Scale { get; internal set; }
    }
}
