using App.Core.Builders;
using App.Core.Domain.Common;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Common
{
    public partial class TemplateBuilder : EntityBuilder<Template>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Template.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Template.Name)).AsString(200).Nullable()
                .WithColumn(nameof(Template.TemplateType)).AsString(50).Nullable()
                .WithColumn(nameof(Template.Content)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(Template.CreatedByUserId)).AsInt32().Nullable()
                .WithColumn(nameof(Template.CreatedOnUtc)).AsDateTime2().Nullable();
        }
    }
}