using App.Core.Builders;
using App.Core.Domain.Organization;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Organization;

/// <summary>
/// Represents a Unit entity builder
/// </summary>
public partial class UnitBuilder : EntityBuilder<Unit>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Unit.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(Unit.Name)).AsString(256).Nullable()
            .WithColumn(nameof(Unit.DepartmentId)).AsInt32().Nullable()
                .ForeignKey(nameof(Department), nameof(Department.Id));
    }
}
