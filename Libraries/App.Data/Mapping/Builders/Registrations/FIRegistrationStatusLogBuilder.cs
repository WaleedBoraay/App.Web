using FluentMigrator.Builders.Create.Table;
using App.Core.Domain.Registrations;
using App.Core.Builders;

namespace App.Data.Mapping.Builders.Registrations;

/// <summary>
/// Represents a FIRegistrationStatusLog entity builder
/// </summary>
public partial class FIRegistrationStatusLogBuilder : EntityBuilder<FIRegistrationStatusLog>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
        .WithColumn(nameof(FIRegistrationStatusLog.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(FIRegistrationStatusLog.RegistrationId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Registration), nameof(Registration.Id))

        // Enum-based statuses
        .WithColumn(nameof(FIRegistrationStatusLog.RegistrationStatus)).AsInt32().NotNullable()
        .WithColumn(nameof(FIRegistrationStatusLog.ValidationStatus)).AsInt32().Nullable()
        .WithColumn(nameof(FIRegistrationStatusLog.ApprovalStatus)).AsInt32().Nullable()
        .WithColumn(nameof(FIRegistrationStatusLog.AuditStatus)).AsInt32().Nullable()

        .WithColumn(nameof(FIRegistrationStatusLog.PerformedBy)).AsInt32().NotNullable()
        .WithColumn(nameof(FIRegistrationStatusLog.ActionDateUtc)).AsDateTime().NotNullable()
        .WithColumn(nameof(FIRegistrationStatusLog.Remarks)).AsString(512).Nullable();
    }
}
