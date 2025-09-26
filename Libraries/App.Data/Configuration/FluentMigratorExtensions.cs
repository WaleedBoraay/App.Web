using FluentMigrator;
using FluentMigrator.Builders.Create;
using FluentMigrator.Infrastructure.Extensions;

namespace App.Data.Configuration
{
    public static class FluentMigratorExtensions
    {
        public static void WithPrimaryKey<TEntity>(this ICreateExpressionRoot create, params string[] columns)
        {
            var tableName = typeof(TEntity).Name;

            create.PrimaryKey($"PK_{tableName}")
                  .OnTable(tableName)
                  .Columns(columns);
        }
    }
}
