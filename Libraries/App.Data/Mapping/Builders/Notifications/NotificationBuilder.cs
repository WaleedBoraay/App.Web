using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Notifications;

namespace App.Data.Mapping.Builders.Notifications
{
    /// <summary>
    /// Represents the Notification entity builder
    /// </summary>
    public partial class NotificationBuilder : EntityBuilder<Notification>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Notification.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Notification.RegistrationId)).AsInt32().Nullable()
                .WithColumn(nameof(Notification.EventTypeId)).AsInt32().NotNullable()
                .WithColumn(nameof(Notification.RecipientUserId)).AsInt32().NotNullable()
                .WithColumn(nameof(Notification.TriggeredByUserId)).AsInt32().NotNullable()
                .WithColumn(nameof(Notification.Message)).AsString(1000).Nullable()
                .WithColumn(nameof(Notification.ChannelId)).AsInt32().NotNullable()
                .WithColumn(nameof(Notification.StatusId)).AsInt32().NotNullable()
                .WithColumn(nameof(Notification.CreatedOnUtc)).AsDateTime().NotNullable();
        }
    }
}
