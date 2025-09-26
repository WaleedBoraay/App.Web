using App.Core.Builders;
using App.Core.Domain.Common;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Common
{
    /// <summary>
    /// Represents a GenericAttribute entity builder
    /// </summary>
    public partial class GenericAttributeBuilder : EntityBuilder<GenericAttribute>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(GenericAttribute.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(GenericAttribute.EntityId)).AsInt32().NotNullable()
                .WithColumn(nameof(GenericAttribute.KeyGroup)).AsString(256).NotNullable()
                .WithColumn(nameof(GenericAttribute.Key)).AsString(256).NotNullable()
                .WithColumn(nameof(GenericAttribute.Value)).AsString(4000).Nullable()
                .WithColumn(nameof(GenericAttribute.StoreId)).AsInt32().NotNullable()
                .WithColumn(nameof(GenericAttribute.CreatedOrUpdatedDateUTC)).AsDateTime2().Nullable();
        }
    }
}
