using App.Core.AppSettingsConfig;
using App.Core.Configuration;
using App.Core.EngineServices;
using App.Core.MigratorServices;
using App.Core.RepositoryServices;
using App.Core.Singletons;
using App.Core.StartupServices;
using App.Data.Configuration;
using App.Data.DataProviders;
using App.Data.EngineServices;
using App.Data.Extensions;
using App.Data.MigratorServices;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Data.Startup;

/// <summary>
/// Represents object for the configuring DB context on application startup
/// </summary>
public partial class AppDbStartup : IAppStartup
{
    private static void ApplyMigrationsAtStartup(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();

        // Apply all update migrations across discovered assemblies (pending only)
        var typeFinder = EngineContext.Current.Resolve<ITypeFinder>();

        var migrationAssemblies = typeFinder.FindClassesOfType<FluentMigrator.MigrationBase>()
            .Select(t => t.Assembly)
            .Where(a => a?.FullName?.Contains("FluentMigrator.Runner") == false)
            .Distinct()
            .ToArray();

        foreach (var asm in migrationAssemblies)
            migrationManager.ApplyUpMigrations(asm, MigrationProcessType.Update, true);
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var typeFinder = Singleton<ITypeFinder>.Instance;

        var migrationAssemblies = typeFinder.FindClassesOfType<MigrationBase>()
            .Select(t => t.Assembly)
            .Where(a => a?.FullName?.Contains("FluentMigrator.Runner") == false)
            .Distinct()
            .ToArray();

        services
            .AddFluentMigratorCore()
            .AddScoped<IProcessorAccessor, AppProcessorAccessor>()
            .AddScoped<IConnectionStringAccessor>(sp =>
            {
                var settings = Singleton<DataConfig>.Instance ?? AppDataSettingsManager.LoadSettings();
                return settings;
            })
            .AddTransient<IMigrationManager, MigrationManager>()
            .AddScoped<IConventionSet, AppConventionSet>()
            .ConfigureRunner(rb =>
                rb.WithVersionTable(new MigrationVersionInfo())
                  .AddSqlServer()
                  .AddMySql5()
                  .AddPostgres()
                  .ScanIn(migrationAssemblies).For.Migrations()
                  .SetCommandTimeout());

        services.AddTransient(p => new Lazy<IVersionLoader>(p.GetRequiredService<IVersionLoader>()));
        services.AddSingleton<IVersionTableMetaData, CustomVersionTableMetaData>();

        // data layer
        services.AddTransient<IDataProviderManager, DataProviderManager>();
        services.AddTransient(sp => sp.GetRequiredService<IDataProviderManager>().DataProvider);

        // repositories
        services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));

        // If DB isn't installed yet, stop here. The Install flow will handle initial schema + seed.
        if (!AppDataSettingsManager.IsDatabaseInstalled())
            return;

        // Build provider once and reuse it below
        var sp = services.BuildServiceProvider();

        //try
        //{
        //    ApplyMigrationsAtStartup(sp);
        //}
        //catch (Exception ex)
        //{
        //    var logger = sp.GetService<ILoggerFactory>()?.CreateLogger("StartupMigrations");
        //    logger?.LogError(ex, "Error while applying startup migrations");
        //    throw; // Fail fast on schema drift
        //}
    }

    public void Configure(IApplicationBuilder application)
    {
        var config = Singleton<AppSettings>.Instance;
        //LinqToDB.Common.Configuration.Linq.DisableQueryCache = config.LinqDisableQueryCache;
    }

    public int Order => 10;
}
