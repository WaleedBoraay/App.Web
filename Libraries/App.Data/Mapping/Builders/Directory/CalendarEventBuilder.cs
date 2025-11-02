using App.Core.Builders;
using App.Core.Domain.Directory;
using App.Data.Mapping.Builders;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Directory
{
    public partial class CalendarEventBuilder : EntityBuilder<CalendarEvent>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CalendarEvent.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(CalendarEvent.Title)).AsString(200).Nullable()
                .WithColumn(nameof(CalendarEvent.Description)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(CalendarEvent.StartDateUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(CalendarEvent.EndDateUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(CalendarEvent.IsHoliday)).AsBoolean().Nullable()
                .WithColumn(nameof(CalendarEvent.EventType)).AsString(50).Nullable()
                .WithColumn(nameof(CalendarEvent.CreatedByUserId)).AsInt32().Nullable();
        }
    }
}