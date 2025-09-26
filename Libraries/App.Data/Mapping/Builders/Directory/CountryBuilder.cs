using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Directory;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Directory;

/// <summary>
/// Represents a Country entity builder
/// </summary>
public partial class CountryBuilder : EntityBuilder<Country>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(Country.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Country.Name)).AsString(256).NotNullable()
        .WithColumn(nameof(Country.TwoLetterIsoCode)).AsString(64).Nullable()
        .WithColumn(nameof(Country.ThreeLetterIsoCode)).AsString(64).Nullable()
        .WithColumn(nameof(Country.NumericIsoCode)).AsInt32().Nullable()
        .WithColumn(nameof(Country.Published)).AsBoolean().NotNullable()
        .WithColumn(nameof(Country.DisplayOrder)).AsInt32().Nullable();
}