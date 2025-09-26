using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Audit;

namespace App.Data.Mapping.Builders.Audit;

/// <summary>
/// Represents a AuditTrailField entity builder
/// </summary>
public partial class AuditTrailFieldBuilder : EntityBuilder<AuditTrailField>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(AuditTrailField.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(AuditTrailField.AuditTrailTableId)).AsInt32().Nullable()
            .ForeignKey(nameof(AuditTrailTable), nameof(AuditTrailTable.Id))
        .WithColumn(nameof(AuditTrailField.DBFieldName)).AsString(256).Nullable()
        .WithColumn(nameof(AuditTrailField.SystemName)).AsString(256).Nullable();
}