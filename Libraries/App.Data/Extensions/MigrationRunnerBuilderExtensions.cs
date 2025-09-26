using App.Core;
using FluentMigrator.Runner;

namespace App.Data.Extensions;

public static class MigrationRunnerBuilderExtensions
{
    public static IMigrationRunnerBuilder SetCommandTimeout(this IMigrationRunnerBuilder builder)
    {
        var dataSettings = DataSettingsManager.LoadSettings();

        return dataSettings is not { SQLCommandTimeout: { } }
            ? builder
            : builder.WithGlobalCommandTimeout(TimeSpan.FromSeconds(dataSettings.SQLCommandTimeout.Value));
    }
}
