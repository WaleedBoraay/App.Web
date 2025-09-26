using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Institutions;

namespace App.Data.Mapping.Builders.Institutions
{
    /// <summary>
    /// Represents the Institution entity builder
    /// </summary>
    public partial class InstitutionBuilder : EntityBuilder<Institution>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Institution.Id)).AsInt32().PrimaryKey().Identity()
                .WithColumn(nameof(Institution.Name)).AsString(256).Nullable()
                .WithColumn(nameof(Institution.BusinessPhoneNumber)).AsString(256).Nullable()
                .WithColumn(nameof(Institution.Email)).AsString(256).Nullable()
                .WithColumn(nameof(Institution.LicenseNumber)).AsString(64).Nullable()
                .WithColumn(nameof(Institution.LicenseSectorId)).AsInt32().Nullable()
                .WithColumn(nameof(Institution.FinancialDomainId)).AsInt32().Nullable()
                .WithColumn(nameof(Institution.LicenseIssueDate)).AsDateTime2().Nullable()
                .WithColumn(nameof(Institution.LicenseExpiryDate)).AsDateTime2().Nullable()
                .WithColumn(nameof(Institution.CountryId)).AsInt32().Nullable()
                .WithColumn(nameof(Institution.Address)).AsString(512).Nullable()
                .WithColumn(nameof(Institution.IsActive)).AsBoolean().Nullable()
                .WithColumn(nameof(Institution.CreatedOnUtc)).AsDateTime2().Nullable()
                .WithColumn(nameof(Institution.UpdatedOnUtc)).AsDateTime2().Nullable();
        }
    }
}