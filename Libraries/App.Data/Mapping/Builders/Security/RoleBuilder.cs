using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Security;

namespace App.Data.Mapping.Builders.Security
{
    /// <summary>
    /// Represents the Role entity builder
    /// </summary>
    public partial class RoleBuilder : EntityBuilder<Role>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Role.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Role.Name)).AsString(128).Nullable()
                .WithColumn(nameof(Role.Description)).AsString(512).Nullable()
                .WithColumn(nameof(Role.IsActive)).AsBoolean().Nullable()
                .WithColumn(nameof(Role.CreatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Role.UpdatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Role.SystemName)).AsString(128).Nullable();
        }
    }
}