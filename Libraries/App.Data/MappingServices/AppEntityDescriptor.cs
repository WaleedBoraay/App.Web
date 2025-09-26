using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MappingServices
{
    public partial class AppEntityDescriptor
    {
        public AppEntityDescriptor()
        {
            Fields = new List<AppEntityFieldDescriptor>();
        }

        public string EntityName { get; set; }
        public string SchemaName { get; set; }
        public ICollection<AppEntityFieldDescriptor> Fields { get; set; }
    }
}
