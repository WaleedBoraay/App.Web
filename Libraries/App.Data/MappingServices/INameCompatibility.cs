using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MappingServices
{
    /// <summary>
    /// Backward compatibility of table naming
    /// </summary>
    public partial interface INameCompatibility
    {
        /// <summary>
        /// Gets table name for mApping with the type
        /// </summary>
        Dictionary<Type, string> TableNames { get; }

        /// <summary>
        ///  Gets column name for mApping with the entity's property and type
        /// </summary>
        Dictionary<(Type, string), string> ColumnName { get; }
    }
}
