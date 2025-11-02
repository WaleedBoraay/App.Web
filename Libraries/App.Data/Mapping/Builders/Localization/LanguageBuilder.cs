using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Localization;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Localization;

/// <summary>
/// Represents a Language entity builder
/// </summary>
public partial class LanguageBuilder : EntityBuilder<Language>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(Language.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(Language.Name)).AsString(256).Nullable()
        .WithColumn(nameof(Language.LanguageCulture)).AsString(256).Nullable()
        .WithColumn(nameof(Language.UniqueSeoCode)).AsString(64).Nullable()
        .WithColumn(nameof(Language.FlagImageFileName)).AsString(256).Nullable()
        .WithColumn(nameof(Language.Rtl)).AsBoolean().Nullable()
        .WithColumn(nameof(Language.Published)).AsBoolean().Nullable()
        .WithColumn(nameof(Language.DisplayOrder)).AsInt32().Nullable();
}