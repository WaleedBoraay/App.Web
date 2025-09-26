using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Security;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Security;

/// <summary>
/// Represents a RolePrivilege entity builder
/// </summary>
public partial class RolePrivilegeBuilder : EntityBuilder<RolePrivilege>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(RolePrivilege.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(RolePrivilege.RoleId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Role), nameof(Role.Id))
        .WithColumn(nameof(RolePrivilege.PrivilegeId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Privilege), nameof(Privilege.Id));
}