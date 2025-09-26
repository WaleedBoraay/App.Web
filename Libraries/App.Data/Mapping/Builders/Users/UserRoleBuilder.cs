using App.Core.Builders;
using App.Core.Domain.Security;
using App.Core.Domain.Users;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Users;

/// <summary>
/// Represents a UserRole entity builder
/// </summary>
public partial class UserRoleBuilder : EntityBuilder<UserRole>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(UserRole.Id)).AsInt32().PrimaryKey().Identity()

            .WithColumn(nameof(UserRole.UserId)).AsInt32().Nullable()
                .ForeignKey(nameof(AppUser), nameof(AppUser.Id))
            .WithColumn(nameof(UserRole.RoleId)).AsInt32().Nullable()
                .ForeignKey(nameof(Role), nameof(Role.Id))

            // Organization scope (new)
            .WithColumn(nameof(UserRole.DepartmentId)).AsInt32().Nullable()
            .WithColumn(nameof(UserRole.UnitId)).AsInt32().Nullable()
            .WithColumn(nameof(UserRole.SubUnitId)).AsInt32().Nullable();
    }
}
