using FluentMigrator.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MigratorServices
{
    /// <summary>
    /// Represents a migration manager
    /// </summary>
    public partial interface IMigrationManager
    {
        /// <summary>
        /// Executes an Up for all found unapplied migrations
        /// </summary>
        /// <param name="assembly">Assembly to find migrations</param>
        /// <param name="migrationProcessType">Type of migration process</param>
        /// <param name="commitVersionOnly">Commit only version information</param>
        void ApplyUpMigrations(Assembly assembly, MigrationProcessType migrationProcessType = MigrationProcessType.Installation, bool commitVersionOnly = false);

        /// <summary>
        /// Executes an Up for schema unapplied migrations
        /// </summary>
        /// <param name="assembly">Assembly to find migrations</param>
        void ApplyUpSchemaMigrations(Assembly assembly);

        /// <summary>
        /// Executes a Down for all found (and applied) migrations
        /// </summary>
        /// <param name="assembly">Assembly to find the migration</param>
        void ApplyDownMigrations(Assembly assembly);
        void ApplyUpUpdateMigrations(Assembly assembly);

        /// <summary>
        /// Executes down expressions for the passed migration
        /// </summary>
        /// <param name="migration">Migration to rollback</param>
        void ApplyDownMigration(IMigrationInfo migration);

        /// <summary>
        /// Executes up expressions for the passed migration
        /// </summary>
        /// <param name="migration">Migration to apply</param>
        /// <param name="commitVersionOnly">Commit only version information</param>
        void ApplyUpMigration(IMigrationInfo migration, bool commitVersionOnly = false);

        /// <summary>
        /// Checks if there are any pending migrations.
        /// </summary>
        /// <returns>True if there are pending migrations, otherwise false.</returns>
        bool HasPendingMigrations();
    }
}
