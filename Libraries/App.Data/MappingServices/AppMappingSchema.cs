using App.Core.Infrastructure;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Expressions;
using LinqToDB.DataProvider;
using LinqToDB.Mapping;
using App.Data.MappingServices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core.Singletons;
using App.Data.MigratorServices;
using App.Core;
using App.Core.MigratorServices;
using App.Data.MigratorServices.Extensions;

namespace App.Data.MappingServices
{
    /// <summary>
    /// Provides an access to entity mapping information
    /// </summary>
    public static class AppMappingSchema
    {
        #region Fields

        private static ConcurrentDictionary<Type, AppEntityDescriptor> EntityDescriptors { get; } = new();

        #endregion

        /// <summary>
        /// Returns mapped entity descriptor
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <returns>Mapped entity descriptor</returns>
        public static AppEntityDescriptor GetEntityDescriptor(Type entityType)
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType))
                return null;

            return EntityDescriptors.GetOrAdd(entityType, t =>
            {
                var tableName = NameCompatibilityManager.GetTableName(t);
                var expression = new CreateTableExpression { TableName = tableName };
                var builder = new CreateTableExpressionBuilder(expression, new NullMigrationContext());
                builder.RetrieveTableExpressions(t);

                return new AppEntityDescriptor
                {
                    EntityName = tableName,
                    SchemaName = builder.Expression.SchemaName,
                    Fields = builder.Expression.Columns.Select(column => new AppEntityFieldDescriptor
                    {
                        Name = column.Name,
                        IsPrimaryKey = column.IsPrimaryKey,
                        IsNullable = column.IsNullable,
                        Size = column.Size,
                        Precision = column.Precision,
                        IsIdentity = column.IsIdentity,
                        Type = column.Type ?? System.Data.DbType.String
                    }).ToList()
                };
            });
        }

        /// <summary>
        /// Get or create mapping schema with specified configuration name
        /// </summary>
        public static MappingSchema GetMappingSchema(string configurationName, IDataProvider mappings)
        {

            if (Singleton<MappingSchema>.Instance is null)
            {
                Singleton<MappingSchema>.Instance = new MappingSchema(configurationName, mappings.MappingSchema);
                Singleton<MappingSchema>.Instance.AddMetadataReader(new FluentMigratorMetadataReader());
            }

            return Singleton<MappingSchema>.Instance;
        }
    }
}
