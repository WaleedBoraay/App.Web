using App.Core.Configuration;
using FluentMigrator.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MigratorServices.Extensions
{
    public static class MigrationRunnerBuilderExtensions
    {
        public static IMigrationRunnerBuilder SetCommandTimeout(this IMigrationRunnerBuilder builder)
        {
            var dataSettings = AppDataSettingsManager.LoadSettings();

            return dataSettings is not { SQLCommandTimeout: { } }
                ? builder
                : builder.WithGlobalCommandTimeout(TimeSpan.FromSeconds(dataSettings.SQLCommandTimeout.Value));
        }
    }
}
