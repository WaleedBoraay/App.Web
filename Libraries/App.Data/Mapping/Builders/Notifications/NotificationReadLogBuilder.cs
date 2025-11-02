using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Notifications;
using App.Core.Builders;
using App.Core.Domain.Users;

namespace App.Data.Mapping.Builders.Notifications;

/// <summary>
/// Represents a NotificationReadLog entity builder
/// </summary>
public partial class NotificationReadLogBuilder : EntityBuilder<NotificationReadLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(NotificationReadLog.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(NotificationReadLog.NotificationId)).AsInt32().Nullable()
            .ForeignKey(nameof(Notification), nameof(Notification.Id))
        .WithColumn(nameof(NotificationReadLog.UserId)).AsInt32().Nullable()
            .ForeignKey(nameof(AppUser), nameof(AppUser.Id))
        .WithColumn(nameof(NotificationReadLog.ReadOnUtc)).AsDateTime2().Nullable();
}