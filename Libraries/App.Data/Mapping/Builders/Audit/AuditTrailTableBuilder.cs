using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Audit;

namespace App.Data.Mapping.Builders.Audit
{
    /// <summary>
    /// Represents a AuditTrailTable entity builder
    /// </summary>
    public partial class AuditTrailTableBuilder : EntityBuilder<AuditTrailTable>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AuditTrailTable.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(AuditTrailTable.DBName)).AsString(256).NotNullable()
                .WithColumn(nameof(AuditTrailTable.SystemName)).AsString(256).NotNullable();
        }
    }
}