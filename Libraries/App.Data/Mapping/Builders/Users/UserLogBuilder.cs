using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Users;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Users;

/// <summary>
/// Represents a UserLog entity builder
/// </summary>
public partial class UserLogBuilder : EntityBuilder<UserLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(UserLog.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(UserLog.UserId)).AsInt32().Nullable()
            .ForeignKey(nameof(AppUser), nameof(AppUser.Id))
        .WithColumn(nameof(UserLog.Action)).AsString(256).Nullable()
        .WithColumn(nameof(UserLog.CreatedOnUtc)).AsDateTime2().Nullable()
        .WithColumn(nameof(UserLog.ClientIp)).AsString(256).Nullable();
}