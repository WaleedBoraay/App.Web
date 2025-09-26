using App.Core;
using App.Core.Builders;
using App.Core.Configuration;
using App.Core.EngineServices;
using App.Core.MappingServices;
using App.Core.Singletons;
using App.Data.EngineServices;
using App.Data.MappingServices;
using FluentMigrator;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.MigratorServices.Extensions
{
    /// <summary>
    /// FluentMigrator extensions
    /// </summary>
    public static class FluentMigratorExtensions
    {
        #region  Utils

        private const int DATE_TIME_PRECISION = 6;

        private static Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>> TypeMApping { get; } = new Dictionary<Type, Action<ICreateTableColumnAsTypeSyntax>>
        {
            [typeof(int)] = c => c.AsInt32(),
            [typeof(long)] = c => c.AsInt64(),
            [typeof(string)] = c => c.AsString(int.MaxValue).Nullable(),
            [typeof(bool)] = c => c.AsBoolean(),
            [typeof(decimal)] = c => c.AsDecimal(18, 4),
            [typeof(DateTime)] = c => c.AsNopDateTime2(),
            [typeof(byte[])] = c => c.AsBinary(int.MaxValue),
            [typeof(Guid)] = c => c.AsGuid()
        };

        private static void DefineByOwnType(string columnName, Type propType, CreateTableExpressionBuilder create, bool canBeNullable = false)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentException("The column name cannot be empty");

            if (propType == typeof(string) || propType.FindInterfaces((t, o) => t.FullName?.Equals(o.ToString(), StringComparison.InvariantCultureIgnoreCase) ?? false, "System.Collections.IEnumerable").Length > 0)
                canBeNullable = true;

            var column = create.WithColumn(columnName);

            TypeMApping[propType](column);

            if (propType == typeof(DateTime))
                create.CurrentColumn.Precision = DATE_TIME_PRECISION;

            if (canBeNullable)
                create.Nullable();
        }

        #endregion


        public static void EnsureIndexableStringColumn(this Migration migration, string table, string column, int len = 450)
        {
            migration.Execute.Sql($@"
IF COL_LENGTH('dbo.{table}', '{column}') IS NOT NULL
BEGIN
    DECLARE @t sysname, @maxlen int, @isNullable bit;
    SELECT @t = t.name, @maxlen = c.max_length, @isNullable = c.is_nullable
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID(N'[dbo].[{table}]') AND c.name = N'{column}';

    -- لو nvarchar(max)/varchar(max)/ntext/text حوله لطول آمن قبل الإندكس
    IF (@t IN (N'nvarchar', N'varchar') AND @maxlen = -1) OR @t IN (N'ntext', N'text')
    BEGIN
        DECLARE @null nvarchar(20) = CASE WHEN @isNullable = 1 THEN N'NULL' ELSE N'NOT NULL' END;
        DECLARE @sql nvarchar(max) =
            N'ALTER TABLE [dbo].[{table}] ALTER COLUMN [{column}] ' +
            CASE WHEN @t = N'varchar' THEN N'VARCHAR({len}) ' ELSE N'NVARCHAR({len}) ' END + @null;
        EXEC(@sql);
    END
END
");
        }

        /// <summary>
        /// Defines the column type as date that is combined with a time of day and a specified precision
        /// </summary>
        public static ICreateTableColumnOptionOrWithColumnSyntax AsNopDateTime2(this ICreateTableColumnAsTypeSyntax syntax)
        {
            var dataSettings = AppDataSettingsManager.LoadSettings();

            return dataSettings.DataProvider switch
            {
                DataProviderType.MySql => syntax.AsCustom($"datetime({DATE_TIME_PRECISION})"),
                DataProviderType.SqlServer => syntax.AsCustom($"datetime2({DATE_TIME_PRECISION})"),
                _ => syntax.AsDateTime2()
            };
        }

        /// <summary>
        /// Specifies a foreign key
        /// </summary>
        /// <param name="column">The foreign key column</param>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumnName">The primary tables column name</param>
        /// <param name="onDelete">Behavior for DELETEs</param>
        /// <typeparam name="TPrimary"></typeparam>
        /// <returns>Set column options or create a new column or set a foreign key cascade rule</returns>
        public static ICreateTableColumnOptionOrForeignKeyCascadeOrWithColumnSyntax ForeignKey<TPrimary>(
            this ICreateTableColumnOptionOrWithColumnSyntax column,
            string primaryTableName = null,
            string primaryColumnName = null,
            System.Data.Rule onDelete = System.Data.Rule.None) // ← كانت Cascade
            where TPrimary : BaseEntity
        {
            if (string.IsNullOrEmpty(primaryTableName))
                primaryTableName = NameCompatibilityManager.GetTableName(typeof(TPrimary));
            if (string.IsNullOrEmpty(primaryColumnName))
                primaryColumnName = nameof(BaseEntity.Id);

            return column.Indexed().ForeignKey(primaryTableName, primaryColumnName).OnDelete(onDelete);
        }

        // ❷ أثناء Alter/AddColumn
        public static IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax ForeignKey<TPrimary>(
            this IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax column,
            string primaryTableName = null,
            string primaryColumnName = null,
            System.Data.Rule onDelete = System.Data.Rule.None) // ← كانت Cascade
            where TPrimary : BaseEntity
        {
            if (string.IsNullOrEmpty(primaryTableName))
                primaryTableName = NameCompatibilityManager.GetTableName(typeof(TPrimary));
            if (string.IsNullOrEmpty(primaryColumnName))
                primaryColumnName = nameof(BaseEntity.Id);

            return column.Indexed().ForeignKey(primaryTableName, primaryColumnName).OnDelete(onDelete);
        }
        /// <summary>
        /// Specifies a foreign key
        /// </summary>
        /// <param name="column">The foreign key column</param>
        /// <param name="primaryTableName">The primary table name</param>
        /// <param name="primaryColumnName">The primary tables column name</param>
        /// <param name="onDelete">Behavior for DELETEs</param>
        /// <typeparam name="TPrimary"></typeparam>
        /// <returns>Alter/add a column with an optional foreign key</returns>
        //public static IAlterTableColumnOptionOrAddColumnOrAlterColumnOrForeignKeyCascadeSyntax ForeignKey<TPrimary>(this IAlterTableColumnOptionOrAddColumnOrAlterColumnSyntax column, string primaryTableName = null, string primaryColumnName = null, Rule onDelete = Rule.Cascade) where TPrimary : BaseEntity
        //{
        //    if (string.IsNullOrEmpty(primaryTableName))
        //        primaryTableName = NameCompatibilityManager.GetTableName(typeof(TPrimary));

        //    if (string.IsNullOrEmpty(primaryColumnName))
        //        primaryColumnName = nameof(BaseEntity.Id);

        //    return column.Indexed().ForeignKey(primaryTableName, primaryColumnName).OnDelete(onDelete);
        //}

        public static void TableFor<TEntity>(this ICreateExpressionRoot expressionRoot) where TEntity : BaseEntity
        {
            var type = typeof(TEntity);
            var tableName = NameCompatibilityManager.GetTableName(type);
            var builder = (CreateTableExpressionBuilder)expressionRoot.Table(tableName);
            builder.RetrieveTableExpressions(type);
        }

        public static void RetrieveTableExpressions(this CreateTableExpressionBuilder builder, Type type)
        {
            var typeFinder = Singleton<ITypeFinder>.Instance
                .FindClassesOfType(typeof(IEntityBuilder))
                .FirstOrDefault(t => t.BaseType?.GetGenericArguments().Contains(type) ?? false);

            if (typeFinder != null)
                (EngineContext.Current.ResolveUnregistered(typeFinder) as IEntityBuilder)?.MapEntity(builder);

            var expression = builder.Expression;
            if (!expression.Columns.Any(c => c.IsPrimaryKey))
            {
                var pk = new ColumnDefinition
                {
                    Name = nameof(BaseEntity.Id),
                    Type = DbType.Int32,
                    IsIdentity = true,
                    TableName = NameCompatibilityManager.GetTableName(type),
                    ModificationType = ColumnModificationType.Create,
                    IsPrimaryKey = true
                };
                expression.Columns.Insert(0, pk);
                builder.CurrentColumn = pk;
            }

            var propertiesToAutoMap = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
                .Where(pi => pi.DeclaringType != typeof(BaseEntity) &&
                             pi.CanWrite &&
                             !pi.HasAttribute<NotMappedAttribute>() && !pi.HasAttribute<NotColumnAttribute>() &&
                             !expression.Columns.Any(x => x.Name.Equals(NameCompatibilityManager.GetColumnName(type, pi.Name), StringComparison.OrdinalIgnoreCase)) &&
                             TypeMapping.ContainsKey(GetTypeToMap(pi.PropertyType).propType));

            foreach (var prop in propertiesToAutoMap)
            {
                var columnName = NameCompatibilityManager.GetColumnName(type, prop.Name);
                var (propType, canBeNullable) = GetTypeToMap(prop.PropertyType);
                DefineByOwnType(columnName, propType, builder, canBeNullable);
            }
        }



        // ===== helpers =====

        private static IEnumerable<Type> SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        }

        private static bool ImplementsIEntityBuilderFor(Type candidate, Type entityType)
        {
            // Supports both IEntityBuilder and IEntityBuilder<T>
            // Match generic arg equals entityType
            return candidate.GetInterfaces().Any(i =>
                i.Name is "IEntityBuilder" or "IEntityBuilder`1" &&
                (!i.IsGenericType || i.GenericTypeArguments.Any(ga => ga == entityType)))
                ||
                (candidate.BaseType?.IsGenericType == true &&
                 candidate.BaseType.GenericTypeArguments.Any(ga => ga == entityType));
        }

        public static (Type propType, bool canBeNullable) GetTypeToMap(this Type type)
        {
            if (Nullable.GetUnderlyingType(type) is Type uType)
                return (uType, true);

            return (type, false);
        }

        private static readonly Dictionary<Type, DbType> TypeMapping = new()
{
    { typeof(string), DbType.String },
    { typeof(int), DbType.Int32 },
    { typeof(long), DbType.Int64 },
    { typeof(short), DbType.Int16 },
    { typeof(byte), DbType.Byte },
    { typeof(bool), DbType.Boolean },
    { typeof(DateTime), DbType.DateTime },
    { typeof(decimal), DbType.Decimal },
    { typeof(double), DbType.Double },
    { typeof(float), DbType.Single },
    { typeof(Guid), DbType.Guid },

    // nullable support handled separately (by GetTypeToMap)
};
    }
}
