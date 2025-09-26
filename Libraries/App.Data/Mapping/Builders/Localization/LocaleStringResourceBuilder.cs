using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Localization;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Localization;

/// <summary>
/// Represents a LocaleStringResource entity builder
/// </summary>
public partial class LocaleStringResourceBuilder : EntityBuilder<LocaleStringResource>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(LocaleStringResource.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(LocaleStringResource.LanguageId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Language), nameof(Language.Id))
        .WithColumn(nameof(LocaleStringResource.ResourceName)).AsString(256).NotNullable()
        .WithColumn(nameof(LocaleStringResource.ResourceValue)).AsString(512).Nullable();
}