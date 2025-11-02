using App.Core.Builders;
using App.Core.Domain.Directory;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Directory
{
    public partial class EventScheduleBuilder : EntityBuilder<EventSchedule>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                 .WithColumn(nameof(EventSchedule.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(EventSchedule.Title)).AsString(200).NotNullable()
                .WithColumn(nameof(EventSchedule.Description)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(EventSchedule.StartDateUtc)).AsDateTime2().NotNullable()                
                .WithColumn(nameof(EventSchedule.EndDateUtc)).AsDateTime2().NotNullable()
               .WithColumn(nameof(EventSchedule.IsAllDay)).AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn(nameof(EventSchedule.Location)).AsString(200).Nullable()
              .WithColumn(nameof(EventSchedule.EventType)).AsInt32().NotNullable().WithDefaultValue(6)
               .WithColumn(nameof(EventSchedule.Color)).AsString(50).NotNullable().WithDefaultValue("#3788d8")
                .WithColumn(nameof(EventSchedule.NotifyUsers)).AsBoolean().NotNullable().WithDefaultValue(false)
               .WithColumn(nameof(EventSchedule.NotificationSentAt)).AsDateTime2().Nullable()
               .WithColumn(nameof(EventSchedule.CreatedByUserId)).AsInt32().NotNullable();


        }
    }
}
