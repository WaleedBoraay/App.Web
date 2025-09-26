using App.Core.Builders;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using FluentMigrator.Builders.Create.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Mapping.Builders.Security
{
    public partial class UserPermissionOverrideBuilder : EntityBuilder<UserPermissionOverride>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(UserPermissionOverride.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(UserPermissionOverride.UserId)).AsInt32().NotNullable()
                    .ForeignKey(nameof(AppUser), nameof(AppUser.Id))
                .WithColumn(nameof(UserPermissionOverride.PermissionId)).AsInt32().NotNullable()
                    .ForeignKey(nameof(Permission), nameof(Permission.Id))
                .WithColumn(nameof(UserPermissionOverride.IsGranted)).AsBoolean().NotNullable();
        }
    }

}
