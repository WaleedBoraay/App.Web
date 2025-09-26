using App.Core.Builders;
using App.Core.Domain.Registrations;
using FluentMigrator.Builders.Create.Table;

namespace App.Data.Mapping.Builders.Registrations;

/// <summary>
/// Represents a FIContact entity builder
/// </summary>
public partial class FIContactBuilder : EntityBuilder<FIContact>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
        .WithColumn(nameof(FIContact.Id)).AsInt32().PrimaryKey().Identity()
        .WithColumn(nameof(FIContact.RegistrationId)).AsInt32().NotNullable()
            .ForeignKey(nameof(Registration), nameof(Registration.Id))

        // Enum-based FK
        .WithColumn(nameof(FIContact.ContactTypeId)).AsInt32().NotNullable()

        .WithColumn(nameof(FIContact.JobTitle)).AsString(256).Nullable()
        .WithColumn(nameof(FIContact.FirstName)).AsString(256).NotNullable()
        .WithColumn(nameof(FIContact.MiddleName)).AsString(256).Nullable()
        .WithColumn(nameof(FIContact.LastName)).AsString(256).NotNullable()
        .WithColumn(nameof(FIContact.CivilId)).AsString(100).Nullable()
        .WithColumn(nameof(FIContact.PassportId)).AsString(100).Nullable()
        .WithColumn(nameof(FIContact.CivilAttachmentId)).AsInt32().NotNullable()
        .WithColumn(nameof(FIContact.PassportAttachmentId)).AsInt32().NotNullable()
        .WithColumn(nameof(FIContact.ContactPhone)).AsString(50).NotNullable()
        .WithColumn(nameof(FIContact.BusinessPhone)).AsString(50).Nullable()
        .WithColumn(nameof(FIContact.Email)).AsString(256).NotNullable()
        .WithColumn(nameof(FIContact.NationalityCountryId)).AsInt32().NotNullable()
        .WithColumn(nameof(FIContact.CreatedOnUtc)).AsDateTime().NotNullable()
        .WithColumn(nameof(FIContact.UpdatedOnUtc)).AsDateTime().Nullable();
    }
}
