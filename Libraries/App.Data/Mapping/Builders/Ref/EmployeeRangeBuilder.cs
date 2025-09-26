using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Ref;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Ref;

/// <summary>
/// Represents a EmployeeRange entity builder
/// </summary>
public partial class EmployeeRangeBuilder : EntityBuilder<EmployeeRange>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(EmployeeRange.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(EmployeeRange.Name)).AsString(256).NotNullable()
        .WithColumn(nameof(EmployeeRange.MinValue)).AsInt32().Nullable()
        .WithColumn(nameof(EmployeeRange.MaxValue)).AsInt32().Nullable();
}