using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Notifications;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Notifications;

/// <summary>
/// Represents a NotificationLog entity builder
/// </summary>
public partial class NotificationLogBuilder : EntityBuilder<NotificationLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table) => table
        .WithColumn(nameof(NotificationLog.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(NotificationLog.NotificationId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Notification), nameof(Notification.Id))
        .WithColumn(nameof(NotificationLog.Channel)).AsString(256).NotNullable()
        .WithColumn(nameof(NotificationLog.Success)).AsBoolean().NotNullable()
        .WithColumn(nameof(NotificationLog.Response)).AsString(256).Nullable()
        .WithColumn(nameof(NotificationLog.SentOnUtc)).AsDateTime2().Nullable();
}