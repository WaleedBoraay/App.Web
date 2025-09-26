using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Localization;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Localization;

/// <summary>
/// Represents a LocalizedProperty entity builder
/// </summary>
public partial class LocalizedPropertyBuilder : EntityBuilder<LocalizedProperty>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(LocalizedProperty.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(LocalizedProperty.EntityId)).AsInt32().NotNullable()
        .WithColumn(nameof(LocalizedProperty.LanguageId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Language), nameof(Language.Id))
        .WithColumn(nameof(LocalizedProperty.LocaleKeyGroup)).AsString(256).Nullable()
        .WithColumn(nameof(LocalizedProperty.LocaleKey)).AsString(256).Nullable()
        .WithColumn(nameof(LocalizedProperty.LocaleValue)).AsString(4000).Nullable();
}