using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MigratorServices
{
    /// <summary>
    /// Represents default values related to migration process
    /// </summary>
    public static partial class AppMigrationDefaults
    {
        /// <summary>
        /// Gets the supported datetime formats
        /// </summary>
        public static string[] DateFormats { get; } = ["yyyy-MM-dd HH:mm:ss", "yyyy.MM.dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss:fffffff", "yyyy.MM.dd HH:mm:ss:fffffff", "yyyy/MM/dd HH:mm:ss:fffffff"];

        /// <summary>
        /// Gets the format string to create the description of update migration
        /// <remarks>
        /// 0 - nopCommerce version
        /// 1 - update migration type
        /// </remarks>
        /// </summary>
        public static string UpdateMigrationDescription => "App version {0}. Update {1}";
    }
}
