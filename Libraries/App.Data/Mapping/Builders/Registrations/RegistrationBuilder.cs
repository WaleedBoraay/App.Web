using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Registrations;

namespace App.Data.Mapping.Builders.Registrations
{
    /// <summary>
    /// Represents the Registration entity builder
    /// </summary>
    public partial class RegistrationBuilder : EntityBuilder<Registration>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Registration.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Registration.InstitutionId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.InstitutionName)).AsString(256).Nullable()
                .WithColumn(nameof(Registration.LicenseNumber)).AsString(64).Nullable()
                .WithColumn(nameof(Registration.LicenseSectorId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.LicenseTypeId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.FinancialDomainId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.IssueDate)).AsDateTime().Nullable()
                .WithColumn(nameof(Registration.ExpiryDate)).AsDateTime().Nullable()
                .WithColumn(nameof(Registration.CountryId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.Address)).AsString(512).Nullable()
                .WithColumn(nameof(Registration.StatusId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.CreatedByUserId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.UpdatedByUserId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.SubmittedToUserId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.CreatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Registration.SubmittedDateUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Registration.ApprovedDateUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Registration.AuditedDateUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(Registration.BusinessScaleRangeId)).AsInt32().Nullable()
                .WithColumn(nameof(Registration.EmployeeRangeId)).AsInt32().Nullable();
        }
    }
}
