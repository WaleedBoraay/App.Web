using System.Reflection;
using App.Data.Configuration;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Data.MigratorServices.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, IConfiguration configuration, Assembly migrationsAssembly)
        {
            var conn = configuration.GetConnectionString("DefaultConnection") ?? configuration["ConnectionStrings:DefaultConnection"];
            services.AddFluentMigratorCore()
                .ConfigureRunner(rb => rb.AddSqlServer().WithGlobalConnectionString(conn).ScanIn(migrationsAssembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole());
            services.AddSingleton<IVersionTableMetaData, CustomVersionTableMetaData>();
            return services;
        }
    }
}
