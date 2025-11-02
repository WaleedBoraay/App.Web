using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Registrations;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Registrations;

/// <summary>
/// Represents a FIRegistrationStatusLog entity builder
/// </summary>
public partial class FIRegistrationStatusLogBuilder : EntityBuilder<RegistrationStatusLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
        .WithColumn(nameof(RegistrationStatusLog.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(RegistrationStatusLog.RegistrationId)).AsInt32().Nullable()
            .ForeignKey(nameof(Registration), nameof(Registration.Id))

        // Enum-based statuses
        .WithColumn(nameof(RegistrationStatusLog.RegistrationStatus)).AsInt32().Nullable()
        .WithColumn(nameof(RegistrationStatusLog.ValidationStatus)).AsInt32().Nullable()
        .WithColumn(nameof(RegistrationStatusLog.ApprovalStatus)).AsInt32().Nullable()
        .WithColumn(nameof(RegistrationStatusLog.AuditStatus)).AsInt32().Nullable()

        .WithColumn(nameof(RegistrationStatusLog.PerformedBy)).AsInt32().Nullable()
        .WithColumn(nameof(RegistrationStatusLog.ActionDateUtc)).AsDateTime().Nullable()
        .WithColumn(nameof(RegistrationStatusLog.Remarks)).AsString(512).Nullable();
    }
}
