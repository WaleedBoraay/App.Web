using App.Core.Builders;
using App.Core.Domain.Organization;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Organization;

/// <summary>
/// Represents a SubUnit entity builder
/// </summary>
public partial class SubUnitBuilder : EntityBuilder<SubUnit>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(SubUnit.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(SubUnit.Name)).AsString(256).Nullable()
            .WithColumn(nameof(SubUnit.UnitId)).AsInt32().Nullable()
                .ForeignKey(nameof(Unit), nameof(Unit.Id));
    }
}
