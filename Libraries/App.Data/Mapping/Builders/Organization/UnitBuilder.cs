using App.Core.Builders;
using App.Core.Domain.Organization;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Organization;

/// <summary>
/// Represents a Unit entity builder
/// </summary>
public partial class UnitBuilder : EntityBuilder<Department>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Department.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(Department.Name)).AsString(256).Nullable()
            .WithColumn(nameof(Department.SectorId)).AsInt32().Nullable()
                .ForeignKey(nameof(Sector), nameof(Sector.Id));
    }
}
