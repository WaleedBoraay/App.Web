using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Ref;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Ref;

/// <summary>
/// Represents a BusinessScaleRange entity builder
/// </summary>
public partial class BusinessScaleRangeBuilder : EntityBuilder<BusinessScaleRange>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(BusinessScaleRange.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(BusinessScaleRange.Name)).AsString(256).Nullable()
        .WithColumn(nameof(BusinessScaleRange.MinValue)).AsInt32().Nullable()
        .WithColumn(nameof(BusinessScaleRange.MaxValue)).AsInt32().Nullable();
}