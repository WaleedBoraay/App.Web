using FluentMigrator.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MigratorServices.Extensions
{
    public static partial class MigrationInfoExtension
    {
        /// <summary>
        /// Gets the flag which indicate whether the migration should be applied into DB on the debug mode
        /// </summary>
        public static bool IsNeedToApplyInDbOnDebugMode(this IMigrationInfo info)
        {
            var applyInDbOnDebugMode = info.Migration.GetType().GetCustomAttribute<AppMigrationAttribute>()?.ApplyInDbOnDebugMode;

            return applyInDbOnDebugMode ?? true;
        }
    }
}
