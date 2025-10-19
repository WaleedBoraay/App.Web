using App.Core.Builders;
using App.Core.Domain.Organization;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Organization;

/// <summary>
/// Represents a Department entity builder
/// </summary>
public partial class DepartmentBuilder : EntityBuilder<Sector>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(Sector.Id)).AsInt32().PrimaryKey().Identity()
            .WithColumn(nameof(Sector.Name)).AsString(256).Nullable()
            .WithColumn(nameof(Sector.ParentDepartmentId)).AsInt32().Nullable();
    }
}
