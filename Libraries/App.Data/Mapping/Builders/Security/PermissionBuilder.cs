using App.Core.Builders;
using App.Core.Domain.Security;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Security
{
    public partial class PermissionBuilder : EntityBuilder<Permission>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Permission.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Permission.SystemName)).AsString(256).NotNullable().Unique()
                .WithColumn(nameof(Permission.Category)).AsString(128).Nullable()
                .WithColumn(nameof(Permission.Description)).AsString(512).Nullable()
                .WithColumn(nameof(Permission.IsActive)).AsBoolean().NotNullable();
        }
    }

}