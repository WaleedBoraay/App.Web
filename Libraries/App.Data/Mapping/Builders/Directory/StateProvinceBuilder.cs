using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Directory;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Directory;

/// <summary>
/// Represents a StateProvince entity builder
/// </summary>
public partial class StateProvinceBuilder : EntityBuilder<StateProvince>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(StateProvince.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(StateProvince.CountryId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Country), nameof(Country.Id))
        .WithColumn(nameof(StateProvince.Name)).AsString(256).NotNullable()
        .WithColumn(nameof(StateProvince.Abbreviation)).AsString(256).Nullable()
        .WithColumn(nameof(StateProvince.Published)).AsBoolean().NotNullable()
        .WithColumn(nameof(StateProvince.DisplayOrder)).AsInt32().Nullable();
}