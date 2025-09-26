using App.Core.Builders;
using App.Core.Domain.Security;
using FluentMigrator.Builders.Create.Table;
using Microsoft.AspNetCore.Http.HttpResults;

namespace App.Data.Mapping.Builders.Security
{
    public partial class RolePermissionBuilder : EntityBuilder<RolePermission>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(RolePermission.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(RolePermission.RoleId)).AsInt32().NotNullable()
                    .ForeignKey(nameof(Role), nameof(Role.Id))
                .WithColumn(nameof(RolePermission.PermissionId)).AsInt32().NotNullable()
                    .ForeignKey(nameof(Permission), nameof(Permission.Id));
        }
    }
}
